namespace GdUnit4.TestAdapter.Execution;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Api;

using Extensions;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using Newtonsoft.Json;

using Settings;

using Environment = System.Environment;

internal abstract class BaseTestExecutor
{
    protected static string GodotBin
    {
        get
        {
            var godotPath = Environment.GetEnvironmentVariable("GODOT_BIN");
            if (string.IsNullOrEmpty(godotPath))
                throw new InvalidOperationException(
                    "Godot runtime is not configured. The environment variable 'GODOT_BIN' is not set or empty. Please set it to the Godot executable path.");
            if (!File.Exists(godotPath))
                throw new InvalidOperationException($"The Godot executable was not found at path: {godotPath}");
            return godotPath;
        }
    }

    protected static EventHandler ExitHandler(IFrameworkHandle? frameworkHandle) => (sender, _) =>
    {
        Console.Out.Flush();
        if (sender is Process p)
            frameworkHandle?.SendMessage(TestMessageLevel.Informational, $"Godot ends with exit code: {p.ExitCode}");
    };

    protected static DataReceivedEventHandler StdErrorProcessor(IFrameworkHandle frameworkHandle) => (_, args) =>
    {
        var message = args.Data?.Trim();
        if (string.IsNullOrEmpty(message))
            return;
        // we do log errors to stdout otherwise running `dotnet test` from console will fail with exit code 1
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"{message}");
    };

    protected static string WriteTestRunnerConfig(Dictionary<string, List<TestCase>> groupedTestSuites, GdUnit4Settings gdUnit4Settings)
    {
        try
        {
            CleanupRunnerConfigurations();
        }
        catch (Exception)
        {
            // ignored
        }

        var fileName = $"GdUnitRunner_{Guid.NewGuid()}.cfg";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

        var testConfig = new TestRunnerConfig
        {
            Included = groupedTestSuites.ToDictionary(
                suite => suite.Key,
                suite => suite.Value.Select(t =>
                    new TestCaseConfig { Name = t.GetPropertyValue(TestCaseExtensions.TestCaseNameProperty, t.FullyQualifiedName) })
            ),
            CaptureStdOut = gdUnit4Settings.CaptureStdOut
        };

        File.WriteAllText(filePath, JsonConvert.SerializeObject(testConfig, Formatting.Indented));
        return filePath;
    }


    private static void CleanupRunnerConfigurations()
        => Directory.GetFiles(Directory.GetCurrentDirectory(), "GdUnitRunner_*.cfg")
            .ToList()
            .ForEach(File.Delete);

    protected static void AttachDebuggerIfNeed(IRunContext runContext, IFrameworkHandle frameworkHandle, Process process)
    {
        if (runContext.IsBeingDebugged && frameworkHandle is IFrameworkHandle2 fh2)
            fh2.AttachDebuggerToProcess(process.Id);
    }

    protected static string LookupGodotProjectPath(string classPath)
    {
        var currentDir = new DirectoryInfo(classPath).Parent;
        while (currentDir != null)
        {
            if (currentDir.EnumerateFiles("project.godot").Any())
                return currentDir.FullName;
            currentDir = currentDir.Parent;
        }

        throw new FileNotFoundException("Godot project file '\"project.godot' does not exist");
    }
}
