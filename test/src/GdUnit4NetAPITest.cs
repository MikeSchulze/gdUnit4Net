namespace GdUnit4.Tests;

using Godot;

using static Assertions;

[TestSuite]
public class GdUnit4NetAPITest
{
    [TestCase]
    [RequireGodotRuntime]
    public void IsTestSuite()
    {
        AssertThat(GdUnit4NetAPI.IsTestSuite(GD.Load<CSharpScript>("./src/extractors/ValueExtractorTest.cs"))).IsTrue();
        AssertThat(GdUnit4NetAPI.IsTestSuite(GD.Load<CSharpScript>("./src/core/resources/scenes/Spell.cs"))).IsFalse();
    }


    [TestCase]
    public void Version()
        => AssertThat(GdUnit4NetAPI.Version()).StartsWith("4.4");
}
