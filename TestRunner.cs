using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GdUnit4.Asserts;
using GdUnit4.Executions;
using GdUnit4.Core;

namespace GdUnit4
{
    class TestReporter : ITestEventListener
    {

        private static GdUnitConsole Console = new GdUnitConsole();

        private bool _isFailed = false;

        public TestReporter()
        { }

        public void PublishEvent(TestEvent testEvent) => PrintStatus(testEvent);

        void PrintStatus(TestEvent testEvent)
        {
            switch (testEvent.Type)
            {
                case TestEvent.TYPE.TESTSUITE_BEFORE:
                    Console.WriteLine($"Run Test Suite {testEvent.ResourcePath}");
                    break;
                case TestEvent.TYPE.TESTCASE_BEFORE:
                    //_console.print_color("	Run Test: %s > %s :" % [event.resource_path(), event.test_name()], Color.antiquewhite).prints_color("STARTED", Color.forestgreen)
                    Console.Write($"	Run Test: {testEvent.SuiteName} > {testEvent.TestName} :");
                    Console.WriteLine($"STARTED", ConsoleColor.Green);
                    break;
                case TestEvent.TYPE.TESTCASE_AFTER:
                    //_console.print_color("	Run Test: %s > %s :" % [event.resource_path(), event.test_name()], Color.antiquewhite)
                    // _print_status(event)
                    // _print_failure_report(event.reports())
                    Console.Write($"	Run Test: {testEvent.SuiteName} > {testEvent.TestName} :");
                    WriteStatus(testEvent);
                    break;
                case TestEvent.TYPE.TESTSUITE_AFTER:
                    //_print_status(event)
                    //_console.prints_color("	| %d total | %d error | %d failed | %d skipped | %d orphans |\n" % [_report.test_count(), _report.error_count(), _report.failure_count(), _report.skipped_count(), _report.orphan_count()], Color.antiquewhite)
                    //Console.WriteLine($"	| %d total | %d error | %d failed | %d skipped | %d orphans |");
                    if (_isFailed)
                    {
                        //_parent.GetTree().Quit(1);
                    }
                    break;
            }

            void WriteStatus(TestEvent testEvent)
            {
                if (testEvent.IsSkipped)
                {
                    Console.PrintColored("SKIPPED", ConsoleColor.DarkYellow, GdUnitConsole.BOLD | GdUnitConsole.ITALIC);
                }
                else if (testEvent.IsFailed || testEvent.IsError)
                {
                    _isFailed = true;
                    Console.PrintColored("FAILED", ConsoleColor.Red, GdUnitConsole.BOLD);
                    Console.NewLine();
                    WriteFailureReport(testEvent);
                }
                else if (testEvent.OrphanCount > 0)
                {
                    Console.PrintColored("PASSED", ConsoleColor.Yellow, GdUnitConsole.BOLD | GdUnitConsole.UNDERLINE);
                }
                else
                {
                    Console.PrintColored("PASSED", ConsoleColor.Green, GdUnitConsole.BOLD);
                }
                Console.WriteLine("");
                //Console.Write(" %s" % LocalTime.elapsed(testEvent.elapsed_time()), Color.cornflower)

            }
        }

        private void WriteFailureReport(TestEvent testEvent)
        {
            foreach (TestReport report in testEvent.Reports)
            {
                Console.PrintColored(Core.CoreUtils.NormalizedFailureMessage(report.ToString()), ConsoleColor.DarkCyan);
            }
        }

    }

    partial class TestRunner : Godot.Node
    {



        public override async void _Ready()
        {
            var cmdArgs = Godot.OS.GetCmdlineArgs();
            Console.WriteLine($"This is From Console App {Assembly.GetExecutingAssembly()}");

            var currentDir = Directory.GetCurrentDirectory() + "/test/asserts";
            List<TestSuite> testSuites = ScanTestSuites(new DirectoryInfo(currentDir), new List<TestSuite>());
            using Executor executor = new Executor();
            executor.AddTestEventListener(new TestReporter());

            foreach (var testSuite in testSuites)
            {
                await executor.ExecuteInternally(testSuite);
            }
            Console.WriteLine("done");

            GetTree().Quit(0);
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

        public static List<TestSuite> GetTestSuites(Assembly assembly) =>
                    assembly.GetTypes()
                        .Where(type => type.IsClass && !type.IsAbstract && IsTestSuite(type))
                        .Select(type => new TestSuite(type)).ToList();

        public static bool IsTestSuite(Type type) =>
                type.IsClass && !type.IsAbstract && Attribute.IsDefined(type, typeof(TestSuiteAttribute));

    }
}
