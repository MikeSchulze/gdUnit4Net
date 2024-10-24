namespace GdUnit4.Api;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using CommandLine;

using Core;

using Executions;

using Godot;

using Newtonsoft.Json;

public partial class TestRunner : Node
{
    private bool FailFast { get; set; } = true;

    public async Task RunTests()
    {
        var cmdArgs = OS.GetCmdlineArgs();

        await new Parser(with =>
            {
                with.EnableDashDash = true;
                with.IgnoreUnknownArguments = true;
            })
            .ParseArguments<Options>(cmdArgs)
            .WithParsedAsync(async o =>
            {
                FailFast = o.FailFast;

                var runnerConfig = LoadTestRunnerConfig(o.ConfigFile);
                if (runnerConfig == null)
                {
                    await Console.Error.WriteLineAsync($"Can't read the runner config file '{o.ConfigFile}', Abort!");
                    GetTree().Quit(200);
                }

                var testSuites = LoadTestSuites(runnerConfig!);
                var exitCode = await RunTests(testSuites, runnerConfig!, new TestAdapterReporter());

                Console.WriteLine($"Test run ends with exit code: {exitCode}, FailFast:{FailFast}");
                GetTree().Quit(exitCode);
            });
    }

    private async Task<int> RunTests(List<TestSuite> testSuites, TestRunnerConfig runnerConfig, ITestEventListener listener)
    {
        using (listener)
        {
            if (testSuites.Count == 0)
            {
                await Console.Error.WriteLineAsync("No test suite's are specified!, Abort!");
                return -1;
            }

            using Executor executor = new();
            executor.AddTestEventListener(listener);

            foreach (var testSuite in testSuites)
            {
                await executor.ExecuteInternally(testSuite, runnerConfig);
                if (listener.IsFailed && FailFast)
                    break;
            }

            return listener.IsFailed ? 100 : 0;
        }
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

    private static List<TestSuite> LoadTestSuites(TestRunnerConfig runnerConfig) => runnerConfig.Included
        .Select(TryCreateTestSuite)
        .OfType<TestSuite>() // Filter out invalid test suites
        .ToList();

    private static TestRunnerConfig? LoadTestRunnerConfig(string configFile)
    {
        var json = File.ReadAllText(configFile.Trim('\''));
        return JsonConvert.DeserializeObject<TestRunnerConfig>(json);
    }

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
}
