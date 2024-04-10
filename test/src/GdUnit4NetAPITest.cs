namespace GdUnit4.Tests;

using static GdUnit4.Assertions;

[TestSuite]
public class GdUnit4NetAPITest
{

    [TestCase]
    public void IsTestSuite()
    {
        AssertThat(GdUnit4NetAPI.IsTestSuite("./src/extractors/ValueExtractorTest.cs")).IsTrue();
        AssertThat(GdUnit4NetAPI.IsTestSuite("./src/core/resources/scenes/Spell.cs")).IsFalse();
    }


    [TestCase]
    public void Version()
        => AssertThat(GdUnit4NetAPI.Version()).StartsWith("4.2");
}
