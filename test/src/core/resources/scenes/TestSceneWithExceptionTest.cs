namespace GdUnit4.Tests.Resources;

using System;

using Godot;

public partial class TestSceneWithExceptionTest : Control
{
    public void SomeMethodThatThrowsException()
    {
        Console.WriteLine("Throw a test exception");
        throw new InvalidOperationException("Test Exception");
    }
}
