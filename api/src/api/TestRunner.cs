namespace GdUnit4.Api;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GdUnit4.Executions;
using GdUnit4.Core;

partial class TestRunner : Godot.Node
{
    private bool FailFast { get; set; } = true;

    public async Task RunTests()
    {
        var cmdArgs = Godot.OS.GetCmdlineArgs();
        Console.ForegroundColor = ConsoleColor.White;
        // TODO check this line, it results into a crash when resizing the terminal
        //Console.BufferHeight = 100;
        Console.Clear();
        Console.Title = "GdUnit4TestRunner";
        Console.WriteLine($"This is From Console App {Assembly.GetExecutingAssembly()}");

        var currentDir = $"{Directory.GetCurrentDirectory()}/src";
        List<TestSuite> testSuites = ScanTestSuites(new DirectoryInfo(currentDir), new List<TestSuite>());
        using Executor executor = new Executor();
        TestReporter listener = new TestReporter();
        executor.AddTestEventListener(listener);

        foreach (var testSuite in testSuites)
        {
            //if (!testSuite.Name.Equals("DictionaryAssertTest"))
            //    continue;
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
