using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GdUnit4.Executions;
using GdUnit4.Core;

namespace GdUnit4
{
    class TestReporter : ITestEventListener
    {
        public bool Failed { get; private set; } = false;

        private static GdUnitConsole Console = new GdUnitConsole();

        public TestReporter()
        { }

        public void PublishEvent(TestEvent testEvent) => PrintStatus(testEvent);

        private void PrintStatus(TestEvent testEvent)
        {
            switch (testEvent.Type)
            {
                case TestEvent.TYPE.TESTSUITE_BEFORE:
                    Console.Println($"Run Test Suite {testEvent.ResourcePath}", ConsoleColor.Blue);
                    break;
                case TestEvent.TYPE.TESTCASE_BEFORE:
                    Console.Print($"    {testEvent.SuiteName}", ConsoleColor.Cyan);
                    Console.Print($":{testEvent.TestName.PadRight(80 - testEvent.SuiteName.Length)} ", ConsoleColor.DarkCyan, GdUnitConsole.BOLD);
                    break;
                case TestEvent.TYPE.TESTCASE_AFTER:
                    WriteStatus(testEvent);
                    break;
                case TestEvent.TYPE.TESTSUITE_AFTER:
                    Console.Print($"{testEvent.SuiteName}", ConsoleColor.Blue);
                    if (testEvent.IsSuccess)
                        Console.Print(" PASSED", ConsoleColor.Green, GdUnitConsole.BOLD);
                    else
                        Console.Print(" FAILED", ConsoleColor.Red, GdUnitConsole.BOLD);
                    Console.Println($" {testEvent.ElapsedInMs.Humanize()}").NewLine();
                    break;
            }

            void WriteStatus(TestEvent testEvent)
            {
                if (testEvent.IsSkipped)
                    Console.Print("SKIPPED", ConsoleColor.DarkYellow, GdUnitConsole.BOLD | GdUnitConsole.ITALIC);
                else if (testEvent.IsFailed || testEvent.IsError)
                {
                    Failed = true;
                    Console.Print("FAILED", ConsoleColor.Red, GdUnitConsole.BOLD);
                }
                else if (testEvent.OrphanCount > 0)
                    Console.Print("PASSED", ConsoleColor.Yellow, GdUnitConsole.BOLD | GdUnitConsole.UNDERLINE);
                else
                    Console.Print("PASSED", ConsoleColor.Green, GdUnitConsole.BOLD);

                Console.Println($" {testEvent.ElapsedInMs.Humanize()}", ConsoleColor.Cyan);

                if (testEvent.IsFailed || testEvent.IsError)
                    WriteFailureReport(testEvent);
            }
        }

        private void WriteFailureReport(TestEvent testEvent)
        {
            foreach (TestReport report in testEvent.Reports)
            {
                Console.Println(Core.CoreUtils.NormalizedFailureMessage(report.ToString()), ConsoleColor.DarkCyan);
            }
        }
    }

    partial class TestRunner : Godot.Node
    {
        private bool FailFast { get; set; } = true;

        public override async void _Ready()
        {
            var cmdArgs = Godot.OS.GetCmdlineArgs();
            Console.Clear();
            Console.Title = "GdUnit4TestRunner";
            Console.WriteLine($"This is From Console App {Assembly.GetExecutingAssembly()}");

            var currentDir = Directory.GetCurrentDirectory() + "/test/asserts";
            List<TestSuite> testSuites = ScanTestSuites(new DirectoryInfo(currentDir), new List<TestSuite>());
            using Executor executor = new Executor();
            TestReporter listener = new TestReporter();
            executor.AddTestEventListener(listener);

            foreach (var testSuite in testSuites)
            {
                if (!testSuite.Name.Equals("SignalAssertTest"))
                    continue;
                await executor.ExecuteInternally(testSuite);
                if (listener.Failed && FailFast)
                    break;
            }
            var exitCode = listener.Failed ? 100 : 0;
            Console.WriteLine($"Testrun ends with exit code: {exitCode}, FailFast:{FailFast}");
            GetTree().Quit(exitCode);
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

        private static bool IsTestSuite(Type type) =>
            type.IsClass && !type.IsAbstract && Attribute.IsDefined(type, typeof(TestSuiteAttribute));

    }
}
