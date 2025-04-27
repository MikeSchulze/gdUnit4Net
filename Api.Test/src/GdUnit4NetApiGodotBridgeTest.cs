namespace GdUnit4.Tests;

using Godot;

using static Assertions;

[TestSuite]
public class GdUnit4NetApiGodotBridgeTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void IsTestSuite()
    {
        AssertThat(GdUnit4NetApiGodotBridge.IsTestSuite(GD.Load<CSharpScript>("./src/extractors/ValueExtractorTest.cs"))).IsTrue();
        AssertThat(GdUnit4NetApiGodotBridge.IsTestSuite(GD.Load<CSharpScript>("./src/core/resources/scenes/Spell.cs"))).IsFalse();
    }


    [TestCase]
    public void Version()
        => AssertThat(GdUnit4NetApiGodotBridge.Version()).StartsWith("4.4");
}
