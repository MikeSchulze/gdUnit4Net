namespace GdUnit4.Tests;

using static Assertions;

[TestSuite]
public class GdUnit4NetApiGodotBridgeTest
{
    [TestCase]
    public void Version()
        => AssertThat(GdUnit4NetApiGodotBridge.Version()).StartsWith("5.0.");
}
