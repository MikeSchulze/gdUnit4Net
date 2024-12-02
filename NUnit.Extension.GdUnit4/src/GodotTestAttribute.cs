namespace NUnit.Extension.GdUnit4;

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using Framework;
using Framework.Interfaces;
using Framework.Internal;
using Framework.Internal.Commands;

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
            Console.WriteLine($"Execute test {context.CurrentTest.MethodName}, debug: {isDebug}");


            var result = context.CurrentResult;

            try
            {
                var processStartInfo =
                    new ProcessStartInfo(@"D:\development\Godot_v4.4-dev3_mono_win64\Godot_v4.4-dev3_mono_win64.exe", BuildGodotArguments(context))
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

                var pProcess = new Process { StartInfo = processStartInfo };
                pProcess.EnableRaisingEvents = true;
                pProcess.ErrorDataReceived += StdErrorProcessor();
                pProcess.Exited += ExitHandler();
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
                    return result;
                }

                if (pProcess.ExitCode != 0)
                {
                    result.SetResult(ResultState.Failure,
                        $"Godot process failed with exit code {pProcess.ExitCode}");
                    return result;
                }

                // Process succeeded
                result.SetResult(ResultState.Success);
                return result;
            }
            catch (Exception ex)
            {
                result.RecordException(ex);
                return result;
            }
        }

        private static int? GetDebuggerPort()
        {
            // Check various environment variables that Rider might use
            var debugPort = Environment.GetEnvironmentVariable("RESHARPER_TEST_RUNNER_PORT") ??
                            Environment.GetEnvironmentVariable("RIDER_DEBUGGER_PORT") ??
                            Environment.GetEnvironmentVariable("VSTEST_HOST_DEBUG_PORT");

            if (int.TryParse(debugPort, out var port))
                return port;

            // Try to parse from JetBrains debugging arguments
            var coreFlagsValue = Environment.GetEnvironmentVariable("CORECLR_ENABLE_PROFILING");
            var profilerValue = Environment.GetEnvironmentVariable("CORECLR_PROFILER");

            if (coreFlagsValue == "1" && !string.IsNullOrEmpty(profilerValue))
            {
                // JetBrains debugger is active, try to get port from command line
                var cmdLine = Environment.CommandLine;
                var portMatch = Regex.Match(cmdLine, @"--debugger-agent=.*address=.*:(\d+)");
                if (portMatch.Success && int.TryParse(portMatch.Groups[1].Value, out port))
                    return port;
            }

            return null;
        }

        protected static EventHandler ExitHandler() => (sender, _) =>
        {
            Console.Out.Flush();
            //if (sender is Process p)
            //  frameworkHandle?.SendMessage(TestMessageLevel.Informational, $"Godot ends with exit code: {p.ExitCode}");
        };

        protected static DataReceivedEventHandler StdErrorProcessor() => (_, args) =>
        {
            var message = args.Data?.Trim();
            if (string.IsNullOrEmpty(message))
                return;
            // we do log errors to stdout otherwise running `dotnet test` from console will fail with exit code 1
            Console.WriteLine($"{message}");
        };

        private string BuildGodotArguments(TestExecutionContext context)
        {
            var assembly = context.CurrentTest.TypeInfo.Assembly;

            var args =
                $"--path . -d -s res://GodotTestRunner.cs --test-assembly {assembly.Location} --test-suite {context.CurrentTest.ClassName} --test-case {context.CurrentTest.MethodName}";


            //args += " --remote-debug tcp://127.0.0.1:64308 --verbose";

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
