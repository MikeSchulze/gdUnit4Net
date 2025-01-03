namespace GdUnit4.Core.Commands;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Api;

using Execution;

using Newtonsoft.Json;

using ExecutionContext = Execution.ExecutionContext;

public class ExecuteTestSetupCommand : BaseCommand
{
    [JsonConstructor]
    private ExecuteTestSetupCommand() { }

    public ExecuteTestSetupCommand(TestCaseNode test) => Test = test;

    [JsonProperty] private TestCaseNode Test { get; set; } = null!;

    public override async Task<Response> Execute()
    {
        try
        {
            var executionContext = CurrentExecutionContext(Test.ParentId);
            var stage = new BeforeTestExecutionStage(executionContext!.TestSuite);


            var testCase = executionContext.TestSuite.TestCases.First(t => t.Name == Test.ManagedMethod);
            var attribute = testCase.TestCaseAttributes.ElementAt(Test.AttributeIndex);

            ExecutionContext testCaseContext = new(executionContext, testCase, attribute);
            ExecutionContextStore.SetContext(Test.Id, testCaseContext);


            await stage.Execute(testCaseContext);
            return new Response
            {
                StatusCode = HttpStatusCode.OK,
                Payload = $"Test setup {Test.ManagedMethod} executed"
            };
        }
        catch (Exception e)
        {
            return new Response
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Payload = $"Test setup for {Test.ManagedMethod} failed"
            };
        }
    }
}
