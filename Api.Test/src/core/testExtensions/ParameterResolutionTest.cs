using GdUnit4.Api;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
[ExtendWith<ContextParameterResolver>]
public class ParameterResolutionTest
{
    [TestCase]
    public void TestCaseWithNoParameter()
    {
        AssertBool(true)
            .IsTrue();
    }
    
    [TestCase("foobar")]
    public void TestCaseWithTestCaseParameter(string testParameter)
    {
        AssertString(testParameter)
            .IsEqual("foobar");
    }

    [TestCase]
    public void TestCaseWithResolvedParameter(IExtensionContext context)
    {
        AssertString(context.TestCaseName)
            .IsEqual(nameof(TestCaseWithResolvedParameter));
    }

    [TestCase("hello world")]
    public void TestCaseWithResolvedParameterAndTestCaseParameter(IExtensionContext context, string testParameter)
    {
        AssertString(context.TestCaseName)
            .IsEqual(nameof(TestCaseWithResolvedParameterAndTestCaseParameter));
        AssertString(testParameter)
            .IsEqual("hello world");
    }

    [TestCase("bat country")]
    public void TestCaseWithReversedParameters(string testParameter, IExtensionContext context)
    {
        AssertString(context.TestCaseName)
            .IsEqual(nameof(TestCaseWithReversedParameters));
        AssertString(testParameter)
            .IsEqual("bat country");
    }
}
