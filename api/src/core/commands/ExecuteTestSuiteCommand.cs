namespace GdUnit4.Core.Commands;

using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Api;

using Events;

using Execution;

using Extensions;

using Newtonsoft.Json;

public class ExecuteTestSuiteCommand : BaseCommand
{
    [JsonConstructor]
    private ExecuteTestSuiteCommand() { }

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
            var tests = Suite.Tests;
            var type = FindTypeOnAssembly(Suite.AssemblyPath, Suite.ManagedType);
            var testSuite = new TestSuite(type, tests);

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

    private static Type FindTypeOnAssembly(string assemblyPath, string clazz)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(clazz);
            if (type != null) return type;
        }

        return Assembly.Load(assemblyPath).GetType(clazz) ?? throw new InvalidOperationException($"Could not find type {clazz} on assembly {assemblyPath}");
    }
}
