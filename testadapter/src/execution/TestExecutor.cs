using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System;
using GdUnit4.TestAdapter.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GdUnit4.TestAdapter.Execution;

internal class TestExecutor : BaseTestExecutor, ITestExecutor
{
    private Process? pProcess = null;
    private readonly GdUnit4Settings gdUnit4Settings;

    private int ParallelTestCount { get; set; }

    private int SessionTimeOut { get; set; }

    public TestExecutor(RunConfiguration configuration, GdUnit4Settings gdUnit4Settings)
    {
        ParallelTestCount = configuration.MaxCpuCount == 0
            ? 1
            : configuration.MaxCpuCount;
        SessionTimeOut = (int)(configuration.TestSessionTimeout == 0
                ? ITestExecutor.DEFAULT_SESSION_TIMEOUT
                : configuration.TestSessionTimeout);

        this.gdUnit4Settings = gdUnit4Settings;
    }

    public void Run(IFrameworkHandle frameworkHandle, IRunContext runContext, IEnumerable<TestCase> testCases)
    {
        // TODO split into multiple threads by using 'ParallelTestCount'
        Dictionary<string, List<TestCase>> groupedTests = testCases
            .GroupBy(t => t.CodeFilePath!)
            .ToDictionary(group => group.Key, group => group.ToList());

        var thread = new Thread(() =>
        {
            var workingDirectory = LookupGodotProjectPath(groupedTests.First().Key);
            var configName = WriteTestRunnerConfig(groupedTests);

            //var filteredTestCases = filterExpression != null
            //    ? testCases.FindAll(t => filterExpression.MatchTestCase(t, (propertyName) =>
            //    {
            //        SupportedProperties.TryGetValue(propertyName, out TestProperty? testProperty);
            //        return t.GetPropertyValue(testProperty);
            //    }) == false)
            //    : testCases;
            var processStartInfo = new ProcessStartInfo(@$"{GodotBin}", @$"-d --path {workingDirectory} --testadapter --configfile='{configName}' {gdUnit4Settings.Parameters}")
            {
                StandardOutputEncoding = Encoding.Default,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = @$"{workingDirectory}"
            };

            using (pProcess = new() { StartInfo = processStartInfo })
            {
                pProcess.EnableRaisingEvents = true;
                pProcess.OutputDataReceived += TestEventProcessor(frameworkHandle, testCases);
                pProcess.ErrorDataReceived += StdErrorProcessor(frameworkHandle);
                pProcess.Exited += ExitHandler(frameworkHandle);
                pProcess.Start();
                AttachDebuggerIfNeed(runContext, frameworkHandle, pProcess);

                pProcess.BeginErrorReadLine();
                pProcess.BeginOutputReadLine();
                pProcess.WaitForExit(SessionTimeOut);
                File.Delete(configName);
            };
        });
        thread.Start();
        thread.Join();
    }

    public void Cancel()
    {
        lock (this)
        {
            Console.WriteLine("Cancel triggered");
            pProcess?.Kill();
        }
    }
}
