namespace GdUnit4.Tests.Core;

using System;
using System.Threading.Tasks;


// will be ignored because of missing `[TestSuite]` annotation
// used by executor integration test
public class TestSuiteAllTestsFailWithExceptions
{

    [GodotTestCase]
    public void ExceptionIsThrownOnSceneInvoke()
    {
        var runner = ISceneRunner.Load("res://src/core/resources/scenes/TestSceneWithExceptionTest.tscn");

        runner.Invoke("SomeMethodThatThrowsException");
    }

    [TestCase]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task ExceptionAtAsyncMethod()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        => throw new ArgumentException("outer exception", new ArgumentNullException("inner exception"));

    [TestCase]
    public void ExceptionAtSyncMethod()
        => throw new ArgumentException("outer exception", new ArgumentNullException("inner exception"));

}
