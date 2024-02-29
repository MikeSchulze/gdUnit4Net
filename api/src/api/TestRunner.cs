namespace GdUnit4.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;

using GdUnit4.Executions;
using GdUnit4.Core;

using Newtonsoft.Json;

public partial class TestRunner : Godot.Node
{

    public class Options
    {
        [Option(Required = false, HelpText = "If FailFast=true the test run will abort on first test failure.")]
        public bool FailFast { get; set; } = false;

        [Option(Required = false, HelpText = "Runs the Runner in test adapter mode.")]
        public bool TestAdapter { get; set; }

        [Option(Required = false, HelpText = "The test runner config.")]
        public string ConfigFile { get; set; } = "";

        [Option(Required = false, HelpText = "Adds the given test suite or directory to the execution pipeline.")]
        public string Add { get; set; } = "";
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
               var exitCode = await (o.TestAdapter
                ? RunTests(LoadTestSuites(o.ConfigFile), new TestAdapterReporter())
                : RunTests(LoadTestSuites(new DirectoryInfo(o.Add)), new TestReporter()));
               Console.WriteLine($"Testrun ends with exit code: {exitCode}, FailFast:{FailFast}");
               GetTree().Quit(exitCode);
           });
    }

    private async Task<int> RunTests(IEnumerable<TestSuite> testSuites, ITestEventListener listener)
    {
        if (!testSuites.Any())
        {
            Console.Error.WriteLine("No testsuite's specified!, Abort!");
            return -1;
        }
        using Executor executor = new();
        executor.AddTestEventListener(listener);

        foreach (var testSuite in testSuites)
        {
            await executor.ExecuteInternally(testSuite!);
            if (listener.IsFailed && FailFast)
                break;
        }
        return listener.IsFailed ? 100 : 0;
    }

    private static TestSuite? TryCreateTestSuite(KeyValuePair<string, IEnumerable<TestCaseConfig>> entry)
    {
        var testSuitePath = entry.Key;
        var testCases = entry.Value.Select(t => t.Name);
        Console.WriteLine($"Load testsuite {testSuitePath}");

        if (GdUnitTestSuiteBuilder.ParseType(testSuitePath, true) != null)
            return new TestSuite(testSuitePath, testCases);
        Console.Error.WriteLine($"Can't load testsuite {testSuitePath}! Skip it!");
        return null;
    }

    private static List<TestSuite> LoadTestSuites(string runnerConfigFile)
    {
        var runnerConfig = LoadTestRunnerConfig(runnerConfigFile);
        return runnerConfig?.Included
            .Select(TryCreateTestSuite)
            .OfType<TestSuite>() // Filter out invalid test suites
            .ToList() ?? new List<TestSuite>();
    }

    private static TestRunnerConfig? LoadTestRunnerConfig(string configFile)
    {
        var json = File.ReadAllText(configFile.Trim('\''));
        return JsonConvert.DeserializeObject<TestRunnerConfig>(json);
    }

    private static IEnumerable<TestSuite> LoadTestSuites(DirectoryInfo rootDir, string searchPattern = "*.cs")
    {
        Stack<DirectoryInfo> stack = new();
        stack.Push(rootDir);

        while (stack.Count > 0)
        {
            var currentDir = stack.Pop();
            Console.WriteLine($"Scanning for test suites in: {currentDir.FullName}");

            foreach (var filePath in Directory.EnumerateFiles(currentDir.FullName, searchPattern))
            {
                if (GdUnitTestSuiteBuilder.ParseType(filePath, true) != null)
                    yield return new TestSuite(filePath);
            }

            foreach (var directory in currentDir.GetDirectories())
                stack.Push(directory);
        }
    }
}
