﻿namespace GdUnit4.Core.Commands;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Api;

using Execution;

using Extensions;

using Newtonsoft.Json;

/// <summary>
///     Command to execute a test suite with configurable execution options.
/// </summary>
public class ExecuteTestSuiteCommand : BaseCommand
{
    [JsonConstructor]
    private ExecuteTestSuiteCommand() { }

    /// <summary>
    ///     Initializes a new instance of the ExecuteTestSuiteCommand.
    /// </summary>
    /// <param name="testSuite">The test suite to execute</param>
    /// <param name="isCaptureStdOut">Whether to capture standard output during test execution</param>
    /// <param name="isReportOrphanNodesEnabled">Whether to report orphaned nodes after test execution</param>
    public ExecuteTestSuiteCommand(TestSuiteNode testSuite, bool isCaptureStdOut, bool isReportOrphanNodesEnabled)
    {
        Suite = testSuite;
        IsCaptureStdOut = isCaptureStdOut;
        IsReportOrphanNodesEnabled = isReportOrphanNodesEnabled;
        IsEngineMode = Suite.Tests.First().RequireRunningGodotEngine;
    }

    [JsonProperty] private TestSuiteNode Suite { get; set; } = null!;
    [JsonProperty] private bool IsCaptureStdOut { get; set; }
    [JsonProperty] private bool IsEngineMode { get; set; }
    [JsonProperty] private bool IsReportOrphanNodesEnabled { get; set; }

    public override async Task<Response> Execute(ITestEventListener testEventListener)
    {
        try
        {
            var testSuite = new TestSuite(Suite);

            try
            {
                if (!IsReportOrphanNodesEnabled)
                    Console.WriteLine("Warning!!! Reporting orphan nodes is disabled. Please check GdUnit settings.");

                using ExecutionContext context = new(
                    testSuite,
                    new[] { testEventListener },
                    IsReportOrphanNodesEnabled,
                    IsEngineMode);
                context.IsCaptureStdOut = IsCaptureStdOut;
                if (context.IsEngineMode)
                    await GodotObjectExtensions.SyncProcessFrame;
                await new TestSuiteExecutionStage(testSuite).Execute(context);
            }
            // handle unexpected exceptions
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync($"Unexpected Exception: {e.Message} \nStackTrace: {e.StackTrace}");
            }
            finally
            {
                testSuite.Dispose();
            }

            return new Response
            {
                StatusCode = HttpStatusCode.OK,
                Payload = $"Test suite {Suite.ManagedType} executed successfully."
            };
        }
        catch (Exception ex)
        {
            return new Response
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Payload = JsonConvert.SerializeObject(ex)
            };
        }
    }
}
