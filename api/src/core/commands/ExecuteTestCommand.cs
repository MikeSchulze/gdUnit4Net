namespace GdUnit4.Core.Commands;

using System.Threading.Tasks;

using Api;

using Execution;

using Newtonsoft.Json;

using ExecutionContext = Execution.ExecutionContext;

public class ExecuteTestCommand : BaseCommand
{
    [JsonConstructor]
    private ExecuteTestCommand() { }

    public ExecuteTestCommand(TestCaseNode test) => Test = test;

    [JsonProperty] private TestCaseNode Test { get; set; } = null!;

    public override async Task<Response> Execute()
    {
        var executionContext = CurrentExecutionContext(Test.Id)!;

        var testCase = executionContext.CurrentTestCase;
        using var testCaseContext = new ExecutionContext(executionContext, testCase.Arguments);

        await new TestCaseExecutionStage(testCaseContext.TestCaseName, testCase, testCase.TestCaseAttribute).Execute(testCaseContext);

        return new Response();
    }
}
