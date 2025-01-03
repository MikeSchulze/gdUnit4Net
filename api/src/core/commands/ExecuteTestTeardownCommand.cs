namespace GdUnit4.Core.Commands;

using System;
using System.Net;
using System.Threading.Tasks;

using Api;

using Execution;

using Newtonsoft.Json;

public class ExecuteTestTeardownCommand : BaseCommand
{
    [JsonConstructor]
    private ExecuteTestTeardownCommand() { }

    public ExecuteTestTeardownCommand(TestCaseNode test) => Test = test;

    [JsonProperty] private TestCaseNode Test { get; set; } = null!;

    public override async Task<Response> Execute()
    {
        try
        {
            var executionContext = ExecutionContextStore.GetContext(Test.Id);
            var stage = new AfterTestExecutionStage(executionContext!.TestSuite);

            await stage.Execute(executionContext);

            var result = executionContext.TearDownTestEvent(Test.Id);
            ExecutionContextStore.RemoveContext(Test.Id);
            executionContext.Dispose();
            return new Response
            {
                StatusCode = HttpStatusCode.OK,
                Payload = JsonConvert.SerializeObject(result, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Formatting = Formatting.Indented
                })
            };
        }
        catch (Exception e)
        {
            return new Response
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Payload = JsonConvert.SerializeObject(e)
            };
        }
    }
}
