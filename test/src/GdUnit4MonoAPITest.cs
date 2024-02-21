
namespace GdUnit4.Tests;

using static GdUnit4.Assertions;

[TestSuite]
public class GdUnit4MonoAPITest
{

    [TestCase]
    public void IsTestSuite()
    {
        AssertThat(GdUnit4MonoAPI.IsTestSuite("./src/extractors/ValueExtractorTest.cs")).IsTrue();
        AssertThat(GdUnit4MonoAPI.IsTestSuite("./src/core/resources/scenes/Spell.cs")).IsFalse();
    }


    [TestCase]
    public void Version()
    {
        AssertThat(GdUnit4MonoAPI.Version()).StartsWith("4.2");
    }
}
