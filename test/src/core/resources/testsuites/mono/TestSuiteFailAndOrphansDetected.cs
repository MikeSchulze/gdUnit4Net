namespace GdUnit4.Tests.Resources;

using System;
using System.Collections.Generic;

using static Assertions;

// will be ignored because of missing `[TestSuite]` annotation
// used by executor integration test
//[TestSuite]
public class TestSuiteFailAndOrphansDetected : IDisposable
{
    private readonly List<Godot.Node> orphans = new();

    [Before]
    public void SetupSuite()
    {
        AssertString("Suite Before()").IsEqual("Suite Before()");
        orphans.Add(new Godot.Node());
    }

    [After]
    public void TearDownSuite()
        => AssertString("Suite After()").IsEqual("Suite After()");

    [BeforeTest]
    public void SetupTest()
    {
        AssertString("Suite BeforeTest()").IsEqual("Suite BeforeTest()");
        orphans.Add(new Godot.Node());
        orphans.Add(new Godot.Node());
    }

    [AfterTest]
    public void TearDownTest()
        => AssertString("Suite AfterTest()").IsEqual("Suite AfterTest()");

    [TestCase]
    public void TestCase1()
    {
        orphans.Add(new Godot.Node());
        orphans.Add(new Godot.Node());
        orphans.Add(new Godot.Node());
        AssertString("TestCase1").IsEqual("TestCase1");
    }

    [TestCase]
    public void TestCase2()
    {
        orphans.Add(new Godot.Node());
        orphans.Add(new Godot.Node());
        orphans.Add(new Godot.Node());
        orphans.Add(new Godot.Node());
        AssertString("TestCase2").IsEmpty();
    }

    // finally, we manually release the orphans from the simulated test suite to avoid memory leaks
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    {
        orphans.ForEach(n => n.Free());
        orphans.Clear();
    }
}
