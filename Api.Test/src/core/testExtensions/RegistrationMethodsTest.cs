using GdUnit4.Api;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
[ExtendWith<CompletionTrackerExtension>]
public class RegistrationMethodsTest
{
    private const string CONSTRUCTED_CONTEXT_VALUE = "foobar";
    
    [RegisterExtension]
    private static readonly ITestExtension ConstructedExtension = new ConstructedParameterResolver(CONSTRUCTED_CONTEXT_VALUE);
    
    [TestCase]
    [ExtendWith<ContextParameterResolver>] 
    public void TestClassLevelRegistration(IExtensionContext context)
    {
        // Verify that the BeforeAll method of the CompletionTrackerExtension was executed
        // and stored the value in the context
        var beforeAllExecuted = context.Retrieve<bool>(CompletionTrackerExtension.BeforeAllExecuted);
        var beforeEachExecuted = context.Retrieve<bool>(CompletionTrackerExtension.BeforeEachExecuted);
        
        AssertBool(beforeAllExecuted)
            .IsTrue();
        AssertBool(beforeEachExecuted)
            .IsTrue();
    }
    
    [TestCase]
    [ExtendWith<ContextParameterResolver>]
    public void TestMethodLevelRegistration(IExtensionContext context)
    {
        // Technically this is also covered by the above test, but we can also
        // verify that the context parameter is correctly resolved in this test method
        AssertString(context.TestCaseName)
            .IsEqual(nameof(TestMethodLevelRegistration));
    }


    [TestCase]
    public void TestConstructedExtensionRegistration(string constructorParam)
    {
        AssertString(constructorParam)
            .IsEqual(CONSTRUCTED_CONTEXT_VALUE);
    }
}
