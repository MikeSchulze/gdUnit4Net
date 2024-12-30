namespace NUnit.Extension.GdUnit4;

using System.Diagnostics;
using System.Text;

using Framework;
using Framework.Interfaces;
using Framework.Internal;
using Framework.Internal.Commands;

using TestResult = Framework.Internal.TestResult;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class GodotTestAttribute : NUnitAttribute, ISimpleTestBuilder, IWrapTestMethod
{
    public GodotTestAttribute(params object?[] args) => Arguments = args;


    internal object?[] Arguments { get; set; }
    public string? TestName { get; set; }

    // ISimpleTestBuilder implementation
    public TestMethod BuildFrom(IMethodInfo method, Test? suite)
    {
        var testMethod = new TestMethod(method, suite);

        if (TestName != null)
            testMethod.Name = TestName;

        return testMethod;
    }

    // IWrapTestMethod implementation
    public TestCommand Wrap(TestCommand command) => new GodotTestCommand(command);

    private class GodotTestCommand : DelegatingTestCommand
    {
        public GodotTestCommand(TestCommand innerCommand)
            : base(innerCommand)
        {
        }

        protected static DataReceivedEventHandler StdErrorProcessor => (_, args) =>
        {
            var message = args.Data?.Trim();
            if (string.IsNullOrEmpty(message))
                return;
            // we do log errors to stdout otherwise running `dotnet test` from console will fail with exit code 1
            Console.WriteLine($"{message}");
        };

        protected static EventHandler ExitHandler => (sender, _) =>
        {
            Console.Out.Flush();
            //if (sender is Process p)
            //  frameworkHandle?.SendMessage(TestMessageLevel.Informational, $"Godot ends with exit code: {p.ExitCode}");
        };

        [DebuggerNonUserCode]
        public override TestResult Execute(TestExecutionContext context)
        {
            var workingDirectory = LookupGodotProjectPath(Environment.CurrentDirectory);
            _ = workingDirectory ?? throw new InvalidOperationException("Cannot determine the godot.project! The workingDirectory is not set");

            if (Directory.Exists(workingDirectory))
            {
                Directory.SetCurrentDirectory(workingDirectory);
                Console.WriteLine("Current directory set to: " + Directory.GetCurrentDirectory());
            }

            var isDebug = Debugger.IsAttached;


            var debugPort = DebuggerUtils.GetDebugPort();
            Console.WriteLine($"Execute test {context.CurrentTest.MethodName}, debug: {isDebug}:{debugPort}");


            var result = context.CurrentResult;

            try
            {
                DebuggerUtils.ListDebuggerTypes();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing framework handle: {ex.Message}");
                // Continue with normal execution
            }

            try
            {
                var processStartInfo =
                    new ProcessStartInfo(@"D:\development\Godot_v4.4-dev3_mono_win64\Godot_v4.4-dev3_mono_win64.exe", BuildGodotArguments(context, debugPort))
                    {
                        StandardOutputEncoding = Encoding.Default,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        WorkingDirectory = Environment.CurrentDirectory
                    };


                // Port is not directly available in .NET but can be inferred
                //processStartInfo.EnvironmentVariables.Add("GODOT_MONO_DEBUGGER_AGENT", $"--debugger-agent=transport=dt_socket,address=127.0.0.1:{debugPort},server=n,suspend=y");

                var pProcess = new Process { StartInfo = processStartInfo };
                pProcess.EnableRaisingEvents = true;
                pProcess.ErrorDataReceived += StdErrorProcessor;
                pProcess.Exited += ExitHandler;

                // Pass debug environment from parent process
                // Copy all debug-related environment variables to child process
                if (isDebug)
                {
                    var parentPid = Environment.GetEnvironmentVariable("RESHARPER_HOST_PARENT_PROCESS_PID");

                    // VSTest debugger flags
                    processStartInfo.EnvironmentVariables["VSTEST_RUNNER_DEBUG_ATTACH"] = "1";
                    processStartInfo.EnvironmentVariables["VSTEST_RUNNER_DEBUG_PROCESSID"] = parentPid;

                    // Rider specific debug flags
                    processStartInfo.EnvironmentVariables["RIDER_DEBUGGER_LOG_DIR"] = Environment.GetEnvironmentVariable("RESHARPER_HOST_LOG_DIR");
                    processStartInfo.EnvironmentVariables["JETBRAINS_RIDER_DEBUG"] = "1";
                    processStartInfo.EnvironmentVariables["DEBUGGER_PORT"] = debugPort.ToString();


                    processStartInfo.EnvironmentVariables["RESHARPER_TESTRUNNER"] = "Debug";
                    processStartInfo.EnvironmentVariables["MONO_DEBUGGER_AGENT"] = "--debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:0,suspend=y";
                    processStartInfo.EnvironmentVariables["DOTNET_WAIT_FOR_DEBUGGER"] = "1";

                    // Add debugging command line arguments to Godot
                    //processStartInfo.Arguments += $" --remote-debug tcp://127.0.0.1:{debugPort}";
                }

                pProcess.Start();


                pProcess.BeginErrorReadLine();
                pProcess.BeginOutputReadLine();
                // --> i need here to attach the current running debugger
                Console.WriteLine($"Debugger: {Debugger.IsAttached}");
                //DebuggerUtils.AttachDebuggerToProcess(pProcess.Id);

                if (!pProcess.WaitForExit(30000)) // 30 second timeout
                {
                    pProcess.Kill();

                    result.SetResult(ResultState.Failure, "Godot process timed out after 30 seconds");
                }
                else
                {
                    if (pProcess.ExitCode != 0)
                        result.SetResult(ResultState.Failure, $"Godot process failed with exit code {pProcess.ExitCode}");
                    else
                        result.SetResult(ResultState.Success);
                }


                pProcess.CancelErrorRead();
                pProcess.CancelOutputRead();
                pProcess.ErrorDataReceived -= StdErrorProcessor;
                pProcess.Exited -= ExitHandler;
                pProcess.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                result.RecordException(ex);
                return result;
            }
        }

        private string BuildGodotArguments(TestExecutionContext context, int? debugPort)
        {
            var assembly = context.CurrentTest.TypeInfo.Assembly;

            var args =
                $"--path . -d -s res://GodotTestRunner.cs --test-assembly {assembly.Location} --test-suite {context.CurrentTest.ClassName} --test-case {context.CurrentTest.MethodName}";


            return args;
        }

        protected static string? LookupGodotProjectPath(string classPath)
        {
            Console.WriteLine($"search godot.project: {classPath}");
            var currentDir = new DirectoryInfo(classPath);
            while (currentDir != null)
            {
                if (currentDir.EnumerateFiles("project.godot").Any())
                    return currentDir.FullName;
                currentDir = currentDir.Parent;
            }

            return null;
        }
    }
}
