
using System;
using GdUnit4.Core;

namespace GdUnit4.Api;

class TestReporter : ITestEventListener
{
    public bool IsFailed { get; set; } = false;

    private static readonly GdUnitConsole Console = new();

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
                Console.SaveCursor("TestCaseState");
                Console.NewLine();
                break;
            case TestEvent.TYPE.TESTCASE_AFTER:
                WriteStatus(testEvent);
                break;
            case TestEvent.TYPE.TESTSUITE_AFTER:
                Console.Print($"{testEvent.SuiteName}", ConsoleColor.Blue);
                if (testEvent.IsSuccess)
                    Console.Print(" PASSED", ConsoleColor.Green, GdUnitConsole.BOLD);
                else if (testEvent.IsWarning)
                    Console.Print(" WARNING", ConsoleColor.Yellow, GdUnitConsole.BOLD);
                else
                    Console.Print(" FAILED", ConsoleColor.Red, GdUnitConsole.BOLD);
                Console.Println($" {testEvent.ElapsedInMs.Humanize()}").NewLine();
                break;
        }

        void WriteStatus(TestEvent testEvent)
        {
            Console.SaveCursor("LastLine");
            Console.RestoreCursor("TestCaseState");

            if (testEvent.IsSkipped)
                Console.Print("SKIPPED", ConsoleColor.DarkYellow, GdUnitConsole.BOLD | GdUnitConsole.ITALIC);
            else if (testEvent.IsFailed || testEvent.IsError)
            {
                IsFailed = true;
                Console.Print("FAILED", ConsoleColor.Red, GdUnitConsole.BOLD);
            }
            else if (testEvent.IsWarning)
                Console.Print(" WARNING", ConsoleColor.Yellow, GdUnitConsole.BOLD);
            else if (testEvent.OrphanCount > 0)
                Console.Print("PASSED", ConsoleColor.Yellow, GdUnitConsole.BOLD | GdUnitConsole.UNDERLINE);
            else
                Console.Print("PASSED", ConsoleColor.Green, GdUnitConsole.BOLD);

            Console.Println($" {testEvent.ElapsedInMs.Humanize()}", ConsoleColor.Cyan);
            Console.RestoreCursor("LastLine");
            if (!testEvent.IsSuccess)
                WriteFailureReport(testEvent);
        }
    }

    private static void WriteFailureReport(TestEvent testEvent)
    {
        foreach (TestReport report in testEvent.Reports)
        {
            Console.Println(report.ToString().RichTextNormalize().Indentation(2), ConsoleColor.DarkCyan);
        }
    }
}
