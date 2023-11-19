namespace GdUnit4.Api;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using GdUnit4.Executions;
using GdUnit4.Core;

partial class TestRunner : Godot.Node
{

    public class Options
    {
        [Option(Required = false, HelpText = "If failfast=true the test run will abort on first test failure.")]
        public bool FailFast { get; set; } = true;

        [Option(Required = false, HelpText = "Runs the Runner in test adapter mode.")]
        public bool TestAdapter { get; set; }

        [Option(Required = false, HelpText = "Runs the Runner in test adapter mode.")]
        public IEnumerable<string>? TestSuites { get; set; }
    }

    private bool FailFast { get; set; } = true;

    public async Task RunTests()
    {
        var cmdArgs = Godot.OS.GetCmdlineArgs();
        await new Parser(with =>
        {
            with.EnableDashDash = true;
            with.IgnoreUnknownArguments = true;
        })
           .ParseArguments<Options>(cmdArgs)
           .WithParsedAsync(async o =>
           {
               FailFast = o.FailFast;
               var exitCode = await (o.TestAdapter ? RunTestByAdapter(o.TestSuites!) : RunAllTests());
               Console.WriteLine($"Testrun ends with exit code: {exitCode}, FailFast:{FailFast}");
               GetTree().Quit(exitCode);
           });
    }

    private async Task<int> RunAllTests()
    {
        Console.ForegroundColor = ConsoleColor.White;
        // TODO check this line, it results into a crash when resizing the terminal
        //Console.BufferHeight = 100;
        Console.Clear();
        Console.Title = "GdUnit4TestRunner";
        Console.WriteLine($"This is From Console App {Assembly.GetExecutingAssembly()}");

        var currentDir = $"{Directory.GetCurrentDirectory()}/src";
        List<TestSuite> testSuites = ScanTestSuites(new DirectoryInfo(currentDir), new List<TestSuite>());
        using Executor executor = new Executor();
        ITestEventListener listener = new TestReporter();
        executor.AddTestEventListener(listener);

        foreach (var testSuite in testSuites)
        {
            //if (!testSuite.Name.Equals("GodotObjectExtensionsTest"))
            //    continue;
            await executor.ExecuteInternally(testSuite);
            if (listener.IsFailed && FailFast)
                break;
        }
        return listener.IsFailed ? 100 : 0;
    }

    private async Task<int> RunTestByAdapter(IEnumerable<string> testSuites)
    {
        Console.Title = "GdUnit4TestRunner";
        if (testSuites == null || testSuites.Count() == 0)
        {
            Console.Error.WriteLine("No testsuite's specified!, Abort!");
            return -1;
        }
        using Executor executor = new();
        TestAdapterReporter listener = new();
        executor.AddTestEventListener(listener);

        foreach (var path in testSuites)
        {
            var testSuitePath = path.TrimStart('\'').TrimEnd('\'');
            var testSuite = LoadTestSuite(testSuitePath!);
            if (testSuite == null)
            {
                Console.Error.WriteLine($"Can't load testsuite {testSuitePath}!, Abort!");
                return -1;
            }
            await executor.ExecuteInternally(testSuite!);
            if (listener.IsFailed && FailFast)
                break;
        }
        return listener.IsFailed ? 100 : 0;
    }

    private static List<TestSuite> ScanTestSuites(DirectoryInfo currentDir, List<TestSuite> acc)
    {
        Console.WriteLine($"Scanning for test suites in: {currentDir.FullName}");
        foreach (var file in currentDir.GetFiles("*.cs"))
        {
            Type? type = GdUnitTestSuiteBuilder.ParseType(file.FullName);
            if (type != null && IsTestSuite(type))
                acc.Add(new TestSuite(file.FullName));
        }
        foreach (var directory in currentDir.GetDirectories())
            ScanTestSuites(directory, acc);
        return acc;
    }

    private static TestSuite? LoadTestSuite(string path)
    {
        Type? type = GdUnitTestSuiteBuilder.ParseType(path);
        if (type != null && IsTestSuite(type))
            return new TestSuite(path);
        return null;
    }

    private static bool IsTestSuite(Type type) =>
        type.IsClass && !type.IsAbstract && Attribute.IsDefined(type, typeof(TestSuiteAttribute));

}
