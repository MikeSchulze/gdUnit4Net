namespace GdUnit4.Tests;

using Godot;

using static Assertions;

[TestSuite]
[RequireGodotRuntime]
public class Example
{
    [Before]
    public void Setup()
    {
        new Node();
        AssertObject(null).IsNotNull();
    }

    [After]
    public void TearDown() =>
        AssertObject(null).IsNotNull();


    [TestCase]
    public void Foo()
        => AssertThat(1).IsEqual(2);


    [TestCase]
    public void Bar()
        => AssertObject(null).IsNull();
}
