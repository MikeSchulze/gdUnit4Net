// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Runners;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

using Api;

using Execution;

using Environment = Environment;

/// <summary>
///     Test runner implementation that executes tests in a separate Godot runtime process.
///     Handles process management, IPC, and test execution coordination with the Godot engine.
/// </summary>
[SuppressMessage(
    "Reliability",
    "CA2000:Dispose objects before losing scope",
    Justification = "GodotRuntimeExecutor ownership is transferred to base class which handles disposal")]
internal sealed class GodotRuntimeTestRunner : BaseTestRunner
{
    /// <summary>
    ///     Directory name for temporary test runner files.
    /// </summary>
    internal const string TEMP_TEST_RUNNER_DIR = "gdunit4_testadapter_v5";

    private readonly TestEngineSettings settings;
    private Process? process;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GodotRuntimeTestRunner" /> class.
    ///     Initializes a new instance of the GodotRuntimeTestRunner.
    /// </summary>
    /// <param name="logger">The test engine logger for diagnostic output.</param>
    /// <param name="debuggerFramework">Framework for debugging support.</param>
    /// <param name="settings">Test engine configuration settings.</param>
    internal GodotRuntimeTestRunner(ITestEngineLogger logger, IDebuggerFramework debuggerFramework, TestEngineSettings settings)
        : base(new GodotRuntimeExecutor(logger), logger, settings)
    {
        this.settings = settings;
        DebuggerFramework = debuggerFramework;
    }

    private object ProcessLock { get; } = new();

    private IDebuggerFramework DebuggerFramework { get; }

    /// <summary>
    ///     Gets the path to the Godot executable from environment variables.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when Godot executable path is not configured or found.</exception>
    private string GodotBin
    {
        get
        {
            var godotPath = Environment.GetEnvironmentVariable("GODOT_BIN");
            if (string.IsNullOrEmpty(godotPath))
            {
                var message = "Godot runtime is not configured. The environment variable 'GODOT_BIN' is not set or empty. Please set it to the Godot executable path.";
                Logger.LogError(message);
                throw new InvalidOperationException(message);
            }

            if (File.Exists(godotPath))
                return godotPath;

            var errorMessage = $"The Godot executable was not found at path: {godotPath}";
            Logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
    }

    private DataReceivedEventHandler StdErrorProcessor => (_, args) =>
    {
        var message = args.Data?.Trim();
        if (string.IsNullOrEmpty(message))
            return;

        // we do log errors to stdout otherwise running `dotnet test` from console will fail with exit code 1
        Logger.LogInfo($":stderr: {message}");
    };

    public override void Cancel()
    {
        base.Cancel();
        lock (ProcessLock)
        {
            process?.Kill(true);
            _ = process?.WaitForExit(1000);
            CloseProcess(process);
        }
    }

    public new void RunAndWait(List<TestSuiteNode> testSuiteNodes, ITestEventListener eventListener, CancellationToken cancellationToken)
    {
        lock (ProcessLock)
        {
            var godotBinary = GodotBin;
            if (!VerifyGodotCSharpSupport(godotBinary))
                return;

            if (!InstallTestRunnerClasses(Environment.CurrentDirectory))
                return;

            if (!ReCompileGodotProject(Environment.CurrentDirectory, godotBinary))
                return;
            Logger.LogInfo("======== Running GdUnit4 Godot Runtime Test Runner ========");

            var processStartInfo =
                new ProcessStartInfo(godotBinary, BuildGodotArguments(settings))
                {
                    StandardOutputEncoding = Encoding.Default,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = false,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = Environment.CurrentDirectory
                };

            if (DebuggerFramework.IsDebugProcess)
                process = DebuggerFramework.LaunchProcessWithDebuggerAttached(processStartInfo);
            else
            {
                process = new Process
                {
                    StartInfo = processStartInfo,
                    EnableRaisingEvents = true
                };
                process.OutputDataReceived += (_, args) =>
                {
                    var message = args.Data?.Trim();
                    if (string.IsNullOrEmpty(message))
                        return;

                    Logger.LogInfo($":stdout: {message}");
                };
                process.ErrorDataReceived += StdErrorProcessor;
                process.Exited += ExitHandler("GdUnit4 Godot Runtime Test Runner");
                _ = process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                if (DebuggerFramework.IsDebugAttach)
                    _ = DebuggerFramework.AttachDebuggerToProcess(process);
            }

            base.RunAndWait(testSuiteNodes, eventListener, cancellationToken);

            _ = process.WaitForExit(2000);

            // wait until the process has finished
            var waitRetry = 0;
            while (!process.HasExited && waitRetry++ < 10)
                Thread.Sleep(100);

            // If the process not finished until 10 retries, we kill it manually
            if (!process.HasExited)
            {
                Logger.LogInfo("GdUnit4 Godot Runtime Test Runner is not terminated, force process kill.");
                process.Kill(true);
            }

            CloseProcess(process);
        }
    }

    internal bool VerifyGodotCSharpSupport(string godotBinary)
    {
        using var godotProcess = new Process();
        try
        {
            // recompile the project
            var processStartInfo = new ProcessStartInfo($"{godotBinary}", "--help")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                RedirectStandardInput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Environment.CurrentDirectory
            };

            var hasCSharpOptions = false;
            godotProcess.StartInfo = processStartInfo;
            godotProcess.EnableRaisingEvents = true;
            godotProcess.OutputDataReceived += (_, args) =>
            {
                var message = args.Data?.Trim();
                if (string.IsNullOrEmpty(message))
                    return;

                hasCSharpOptions = hasCSharpOptions || message.Contains("--build-solutions", StringComparison.OrdinalIgnoreCase);
            };

            if (!godotProcess.Start())
            {
                Logger.LogError("Checking Godot C# supports fails on process start, exit ..");
                return false;
            }

            godotProcess.BeginOutputReadLine();
            _ = godotProcess.WaitForExit(500);

            if (!hasCSharpOptions)
            {
                Logger.LogWarning(
                    $"""

                     ╔═════════════════════ NO Godot C# SUPPORT NOT DETECTED ═══════════════════════════════╗

                       The Godot binary at '{godotBinary}' does not appear to support C# development.

                       SOLUTION:
                       Please ensure you're using a Godot build with C# support:

                        1. Verify your .runsettings file (Recommended for projects):
                           Add the GODOT_BIN setting to <EnvironmentVariables>:

                             <RunSettings>
                                 <RunConfiguration>
                                     <EnvironmentVariables>
                                         <GODOT_BIN>D:\path\to\Godot_v4.x-stable_mono_win64.exe</GODOT_BIN>
                                     </EnvironmentVariables>
                                 </RunConfiguration>
                             </RunSettings>

                       2. Or set the GODOT_BIN environment variable:
                          - Windows: set GODOT_BIN=C:\path\to\Godot_v4.x-stable_mono_win64.exe
                          - Linux:   export GODOT_BIN=/path/to/Godot_v4.x-stable_mono_linux.x86_64
                          - macOS:   export GODOT_BIN=/path/to/Godot.app/Contents/MacOS/Godot

                       RESULT:
                       All Godot runtime tests will be skipped until C# support is available.

                     ╚═══════════════════════════════════════════════════════════════════════════════════════╝

                     """);
                return false;
            }

            return godotProcess.ExitCode == 0;
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            Logger.LogError($"Verifying the Godot binary fails with: {e.Message}\n {e.StackTrace}");
            return false;
        }
        finally
        {
            CloseProcess(godotProcess);
        }
    }

    internal bool InstallTestRunnerClasses(string workingDirectory, bool reCompile = true)
    {
        var destinationFolderPath = Path.Combine(workingDirectory, @$"{TEMP_TEST_RUNNER_DIR}");
        if (!Directory.Exists(destinationFolderPath))
            _ = Directory.CreateDirectory(destinationFolderPath);

        var sceneRunnerSource = Path.Combine(destinationFolderPath, "GdUnit4TestRunnerScene.cs");

        // check if the scene runner already installed
        if (File.Exists(sceneRunnerSource))
            return true;

        Logger.LogInfo($"Installing GdUnit4TestRunnerScene at {destinationFolderPath}");

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("GdUnit4.src.core.runners.GdUnit4TestRunnerSceneTemplate.cs");
        using var reader = new StreamReader(stream!);
        var content = reader.ReadToEnd();
        content = content.Replace("GdUnit4TestRunnerSceneTemplate", "GdUnit4TestRunnerScene", StringComparison.Ordinal);
        File.WriteAllText(sceneRunnerSource, content, Encoding.UTF8);

        if (!reCompile)
            return true;
        var isSuccess = RunDotnetRestore(workingDirectory);
        if (!isSuccess)
            CleanupRunnerOnFailure(sceneRunnerSource);
        return isSuccess;
    }

    internal bool ReCompileGodotProject(string workingDirectory, string godotBinary)
    {
        using var compileProcess = new Process();
        try
        {
            // recompile the project
            var processStartInfo = new ProcessStartInfo($"{godotBinary}", @"--path . -e --headless --quit-after 100 --verbose")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory
            };

            Logger.LogInfo($"Working dir {workingDirectory}");
            Logger.LogInfo($"Rebuild Godot Project ... {godotBinary} {processStartInfo.Arguments}");
            compileProcess.StartInfo = processStartInfo;
            compileProcess.EnableRaisingEvents = true;
            compileProcess.OutputDataReceived += (_, args) =>
            {
                var message = args.Data?.Trim();
                if (string.IsNullOrEmpty(message))
                    return;

                Logger.LogInfo($":stdout: {message}");
            };
            compileProcess.ErrorDataReceived += StdErrorProcessor;
            compileProcess.Exited += ExitHandler("Rebuild Godot Project");
            if (!compileProcess.Start())
            {
                Logger.LogError("Rebuild Godot Project fails on process start, exit ..");
                return false;
            }

            compileProcess.BeginErrorReadLine();
            compileProcess.BeginOutputReadLine();
            _ = compileProcess.WaitForExit(100);

            // The compile project can take a while, and we need to wait until it finishes
            // Calculate how many iterations we need based on the compile process timeout
            const int checkIntervalMs = 100; // Check every 100ms
            var maxRetries = settings.CompileProcessTimeout / checkIntervalMs;

            var waitRetry = 0;
            while (!compileProcess.HasExited && waitRetry++ < maxRetries)
                Thread.Sleep(checkIntervalMs);

            // If the process has not finished within the timeout period, we kill it manually
            if (!compileProcess.HasExited)
            {
                Logger.LogError(
                    $"""

                     ╔═══════════════════════ Godot compilation TIMEOUT ═════════════════════════════════════════════════════════════════════╗

                       Godot project compilation did not complete within the configured timeout of {settings.CompileProcessTimeout}ms.

                       Possible reasons:
                       - Your Godot project may be large or complex, requiring more time to compile
                       - Your system may be under heavy load or has limited resources
                       - There might be a compilation issue causing Godot to hang

                       ACTION REQUIRED:
                       To increase the compilation timeout, set the 'CompileProcessTimeout' property in your GdUnit4 settings.

                       Add or modify the following in your .runsettings file:
                       <GdUnit4>
                           <CompileProcessTimeout>60000</CompileProcessTimeout>  <!-- 60 seconds -->
                       </GdUnit4>

                       The process will now be forcefully terminated, which may result in incomplete compilation.

                     ╚══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
                     """);

                compileProcess.Kill(true);
            }

            return compileProcess.ExitCode == 0;
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            Logger.LogError($"Install GdUnit4 `TestRunner` fails with: {e.Message}\n {e.StackTrace}");

            return false;
        }
        finally
        {
            CloseProcess(compileProcess);
        }
    }

    private static string BuildGodotArguments(TestEngineSettings testEngineSettings)
        => $"--path . -d -s res://{TEMP_TEST_RUNNER_DIR}/GdUnit4TestRunnerScene.cs {testEngineSettings.Parameters}";

    private bool RunDotnetRestore(string workingDirectory)
    {
        try
        {
            Logger.LogInfo("Running dotnet build to ensure dependencies are available...");
            var arguments = "build --configuration Debug "
                            + "--verbosity normal "
                            + "--no-restore "
                            + "/p:BuildProjectReferences=false " // Don't rebuild project refs
                            + "/p:_GetChildProjectCopyToOutputDirectoryItems=false " // Don't copy child project items
                            + "/p:SkipCopyingFrameworkReferences=true " // Skip framework refs (already present)
                            + "/p:EnforceCodeStyleInBuild=false "
                            + "/p:TreatWarningsAsErrors=false ";
            var processStartInfo = new ProcessStartInfo("dotnet", arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory
            };

            using var restoreProcess = new Process();
            restoreProcess.StartInfo = processStartInfo;
            restoreProcess.EnableRaisingEvents = true;

            restoreProcess.OutputDataReceived += (_, args) =>
            {
                var message = args.Data?.Trim();
                if (!string.IsNullOrEmpty(message))
                    Logger.LogInfo($":build: {message}");
            };

            restoreProcess.ErrorDataReceived += (_, args) =>
            {
                var message = args.Data?.Trim();
                if (!string.IsNullOrEmpty(message))
                    Logger.LogInfo($":stderr: {message}");
            };

            if (!restoreProcess.Start())
            {
                Logger.LogError("Failed to start dotnet build process");
                return false;
            }

            restoreProcess.BeginErrorReadLine();
            restoreProcess.BeginOutputReadLine();

            // Wait for restore to complete (should be quick)
            var completed = restoreProcess.WaitForExit(30000); // 30 second timeout

            if (!completed)
            {
                Logger.LogWarning("dotnet build timed out after 30 seconds");
                restoreProcess.Kill(true);
                return false;
            }

            var success = restoreProcess.ExitCode == 0;
            if (success)
            {
                Logger.LogInfo("dotnet build completed successfully");
                return success;
            }

            Logger.LogError($"dotnet build failed with exit code: {restoreProcess.ExitCode}");
            return false;
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            Logger.LogError($"Error running build restore: {ex.Message}");
            return false;
        }
    }

    private EventHandler ExitHandler(string source = "") => (sender, _) =>
    {
        Console.Out.Flush();
        if (sender is Process p)
        {
            if (p.ExitCode == 0)
                Logger.LogInfo($"{source} ends with exit code: {p.ExitCode}\n");
            else
                Logger.LogError($"{source} ends with exit code: {p.ExitCode}\n");
        }
    };

    private void CloseProcess(Process? processToClose)
    {
        if (processToClose == null)
            return;
        try
        {
            processToClose.CancelErrorRead();
            processToClose.CancelOutputRead();
            processToClose.ErrorDataReceived -= StdErrorProcessor;
            processToClose.Exited -= ExitHandler();
            processToClose.Dispose();
        }
#pragma warning disable CA1031
        catch (Exception)
#pragma warning restore CA1031
        {
            // ignore
        }
        finally
        {
            lock (ProcessLock)
                process = null;
        }
    }

    /// <summary>
    ///     Cleans up the installed runner file when compilation fails to ensure
    ///     a fresh installation on the next run.
    /// </summary>
    /// <param name="runnerFilePath">Path to the runner file to remove.</param>
    private void CleanupRunnerOnFailure(string runnerFilePath)
    {
        try
        {
            if (File.Exists(runnerFilePath))
            {
                Logger.LogInfo($"Cleaning up runner file at {runnerFilePath} due to compilation failure");
                File.Delete(runnerFilePath);
            }
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            Logger.LogError($"Failed to clean up runner file: {ex.Message}");

            // We don't want to throw here as this is just cleanup
        }
    }
}
