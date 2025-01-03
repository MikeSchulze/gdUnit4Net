namespace GdUnit4.Core.Commands;

using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Api;

using Events;

using Execution;

using Newtonsoft.Json;

using ExecutionContext = Execution.ExecutionContext;

public class ExecuteSetupTestSuiteCommand : BaseCommand
{
    [JsonConstructor]
    private ExecuteSetupTestSuiteCommand() { }

    public ExecuteSetupTestSuiteCommand(TestSuiteNode testSuite) => Suite = testSuite;

    [JsonProperty] private TestSuiteNode Suite { get; set; }

    public override Task<Response> Execute()
    {
        try
        {
            var tests = Suite.Tests;
            var type = FindTypeOnAssembly(Suite.AssemblyPath, Suite.ManagedType);
            var testInstance = new TestSuite(type, tests);
            var executionContext = new ExecutionContext(testInstance, new List<ITestEventListener>(), true);
            ExecutionContextStore.SetContext(Suite.Id, executionContext);

            return Task.FromResult(new Response
            {
                StatusCode = HttpStatusCode.OK,
                Payload = $"Test suite {Suite.ManagedType} loaded"
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new Response
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Payload = JsonConvert.SerializeObject(ex)
            });
        }
    }


    internal static Type FindTypeOnAssembly(string assemblyPath, string clazz)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(clazz);
            if (type != null) return type;
        }

        return Assembly.Load(assemblyPath).GetType(clazz) ?? throw new InvalidOperationException($"Could not find type {clazz} on assembly {assemblyPath}");
    }
}
