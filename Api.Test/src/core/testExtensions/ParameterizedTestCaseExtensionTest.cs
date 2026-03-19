using GdUnit4.Api;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
[ExtendWith<ContextParameterResolver>]
public class ParameterizedTestCaseExtensionTest
{
    [TestCase(1, "one")]
    [TestCase(2, "two")]
    [TestCase(3, "three")]
    public void TestEachTestInvocationReceivesItsOwnArguments(IExtensionContext context, int number, string name)
    {
        // Verify the extension context is live.
        AssertThat(context)
            .IsNotNull();

        // Verify the TestCase arguments reached the method unchanged.
        var expectedName = number switch
        {
            1 => "one",
            2 => "two",
            3 => "three",
            _ => string.Empty
        };

        AssertString(name)
            .IsEqual(expectedName);
    }
}
