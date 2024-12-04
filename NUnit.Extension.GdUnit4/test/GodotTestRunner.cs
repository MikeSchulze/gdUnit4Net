//!/usr/bin/env -S godot -s

namespace NUnit.Extension.GdUnit4;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Godot;

public partial class GodotTestRunner : SceneTree
{
    public override void _Initialize()
    {
        try
        {
            //Thread.Sleep(1000);
            var args = OS.GetCmdlineArgs();
            // Parse command line arguments
            string? assemblyPath = null;
            string? testSuite = null;
            string? testCase = null;

            for (var i = 0; i < args.Length; i++)
                switch (args[i])
                {
                    case "--test-assembly":
                        if (i + 1 < args.Length) assemblyPath = args[i + 1];
                        break;
                    case "--test-suite":
                        if (i + 1 < args.Length) testSuite = args[i + 1];
                        break;
                    case "--test-case":
                        if (i + 1 < args.Length) testCase = args[i + 1];
                        break;
                }

            GD.PrintS("Running test:", $"Assembly: {assemblyPath}, Suite: {testSuite}, Case: {testCase}");

            if (string.IsNullOrEmpty(assemblyPath) || string.IsNullOrEmpty(testSuite) || string.IsNullOrEmpty(testCase))
                throw new ArgumentException("Missing required test parameters");

            var testCaseRunner = new TestCaseRunner(testSuite, testCase);
            Root.AddChild(testCaseRunner);
        }
        catch (Exception e)
        {
            GD.PrintS("Exception", e.Message);
            Quit(1); // Exit with error code
        }
    }


    public override void _Finalize() => base._Finalize();


    private partial class TestCaseRunner : Node
    {
        private readonly string testCase;
        private readonly string testClass;
        private MethodInfo? method;

        public TestCaseRunner(string testClass, string testCase)
        {
            this.testClass = testClass;
            this.testCase = testCase;
        }


        public override void _Ready() => _ = RunTest();

        public async Task RunTest()
        {
            try
            {
                var iteration = 0;
                while (!Debugger.IsAttached && iteration++ < 50)
                {
                    await Godot.Engine.GetMainLoop().ToSignal(Godot.Engine.GetMainLoop(), SceneTree.SignalName.ProcessFrame);
                    Thread.Sleep(100);
                    GD.PrintS($"Godot Debugger: {Debugger.IsAttached}");
                }

                GD.PrintS("Test Started:", testCase);
                Type? type = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(testClass);
                    if (type != null)
                    {
                        GD.PrintS("found on assemblyName", $"Name={type.Name} Location={assembly.Location}");
                        break;
                    }
                    //return type;
                }

                // Create instance if method is not static
                var instance = Activator.CreateInstance(type)
                               ?? throw new InvalidOperationException(
                                   $"Cannot create an instance of '{type.FullName}' because it does not have a public parameterless constructor.");
                var testArguments = Array.Empty<object?>();
                method = type.GetMethod(testCase);

                await GetTree().ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

                using var tokenSource = new CancellationTokenSource();
                var timeout = TimeSpan.FromMilliseconds(10000);
                //Godot.GD.PrintS("Execute", StageName);//, context.MethodArguments.Formatted());
                var obj = method?.Invoke(instance, testArguments);
                var task = obj is Task t ? t : Task.Run(() => { });
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, tokenSource.Token));
                tokenSource.Cancel();
                if (completedTask == task)
                    // Very important in order to propagate exceptions
                    await task;
                else
                    throw new Exception($"The execution has timed out after {timeout}.");


                GD.PrintS("Test Completed Successfully:", method?.Name);
                GetTree().Quit(); // Success
            }
            catch (Exception e)
            {
                GD.PrintS("Test Failed:", method, e.Message);
                GetTree().Quit(1); // Failure
            }
        }
    }
}
