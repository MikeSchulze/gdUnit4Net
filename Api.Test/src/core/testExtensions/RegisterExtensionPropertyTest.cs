using GdUnit4.Api;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
public class RegisterExtensionPropertyTest
{
    private const string PROPERTY_EXTENSION_VALUE = "from-property-extension";

    [RegisterExtension]
    private static ConstructedParameterResolver PropertyExtension { get; } = new(PROPERTY_EXTENSION_VALUE);

    [TestCase]
    public void TestPropertyExtensionIsDiscoveredAndResolvesParameter(string constructorParam)
    {
        AssertString(constructorParam)
            .IsEqual(PROPERTY_EXTENSION_VALUE);
    }

    [TestCase]
    [ExtendWith<CompletionTrackerExtension>]
    [ExtendWith<ContextParameterResolver>]
    public void TestCallbacksExecute(IExtensionContext context)
    {
        var beforeEachExecuted = context.Retrieve<bool>(CompletionTrackerExtension.BeforeEachExecuted);
        AssertBool(beforeEachExecuted)
            .IsTrue();
    }
}
