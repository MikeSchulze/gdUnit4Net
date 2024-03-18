namespace GdUnit4.Tests.Core;

using System;

using static Assertions;

[TestSuite]
public class TestSuiteFailWithExceptions
{

    [TestCase]
    public void ExceptionIsThrownOnSceneInvoke()
    {
        var runner = ISceneRunner.Load("res://src/core/resources/scenes/TestSceneWithExceptionTest.tscn");

        AssertThrown(() => runner.Invoke("SomeMethodThatThrowsException"))
            .IsInstanceOf<InvalidOperationException>()
            .HasFileLineNumber(12)
            .HasFileName("src/core/resources/scenes/TestSceneWithExceptionTest.cs")
            .HasMessage("Test Exception");
    }
}
