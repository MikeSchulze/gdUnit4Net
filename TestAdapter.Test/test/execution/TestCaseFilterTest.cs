#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace GdUnit4.TestAdapter.Test.Execution;

using System.Reflection;

using Api;

using Core.Discovery;

using Extensions;

using Microsoft.VisualStudio.TestPlatform.Common.Filtering;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Adapter;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Discovery;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestAdapter.Execution;
using TestAdapter.Settings;

[TestClass]
public class TestCaseFilterTest
{
    private static readonly ITestEngineLogger Logger = new NoOpLogger();

    // ReSharper disable once InconsistentNaming
    private static TestCase testNamespace_ExampleTestSuiteA_TestA;

    // ReSharper disable once InconsistentNaming
    private static TestCase testNamespace_ExampleTestSuiteA_TestB;

    // ReSharper disable once InconsistentNaming
    private static TestCase otherNamespace_ExampleTestSuiteB_TestA;

    // ReSharper disable once InconsistentNaming
    private static TestCase otherNamespace_ExampleTestSuiteB_TestB;

    private static List<TestCase> testsExamples;

    [ClassInitialize]
    public static void SetUp(TestContext testContext)
    {
        _ = testContext; // Explicitly discard to show intent and satisfy IDE0060
        var testDescriptors = new List<TestCaseDescriptor>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ManagedType = "TestNamespace.ExampleTestSuiteA",
                ManagedMethod = "TestA",
                FullyQualifiedName = "TestNamespace.ExampleTestSuiteA.TestA",
                AssemblyPath = "/debug/examples.dll",
                AttributeIndex = 0,
                CodeFilePath = "/tests/core/ExampleTestSuiteA.cs",
                LineNumber = 12,
                RequireRunningGodotEngine = false,
                Categories = new List<string>
                {
                    "UnitTest",
                    "Fast"
                },
                Traits = new Dictionary<string, List<string>>
                {
                    ["Owner"] = ["TeamA"],
                    ["Priority"] = ["High"]
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                ManagedType = "TestNamespace.ExampleTestSuiteA",
                ManagedMethod = "TestB",
                FullyQualifiedName = "TestNamespace.ExampleTestSuiteA.TestB",
                AssemblyPath = "/debug/examples.dll",
                AttributeIndex = 0,
                CodeFilePath = "/tests/core/ExampleTestSuiteA.cs",
                LineNumber = 22,
                RequireRunningGodotEngine = false,
                Categories = new List<string> { "IntegrationTest" },
                Traits = new Dictionary<string, List<string>>
                {
                    ["Owner"] = ["TeamB"],
                    ["Priority"] = ["Medium"],
                    ["Component"] = ["Database"]
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                ManagedType = "OtherNamespace.ExampleTestSuiteB",
                ManagedMethod = "TestA",
                FullyQualifiedName = "OtherNamespace.ExampleTestSuiteB.TestA",
                AssemblyPath = "/debug/examples.dll",
                AttributeIndex = 0,
                CodeFilePath = "/tests/core/ExampleTestSuiteB.cs",
                LineNumber = 32,
                RequireRunningGodotEngine = false,
                Categories = new List<string> { "SlowTest" },
                Traits = new Dictionary<string, List<string>>
                {
                    ["Owner"] = ["TeamC"],
                    ["Priority"] = ["Low"],
                    ["Feature"] = ["Reporting"]
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                ManagedType = "OtherNamespace.ExampleTestSuiteB",
                ManagedMethod = "TestB",
                FullyQualifiedName = "OtherNamespace.ExampleTestSuiteB.TestB",
                AssemblyPath = "/debug/examples.dll",
                AttributeIndex = 0,
                CodeFilePath = "/tests/core/ExampleTestSuiteB.cs",
                LineNumber = 45,
                RequireRunningGodotEngine = false,
                Categories = new List<string>
                {
                    "UnitTest",
                    "SlowTest"
                },
                Traits = new Dictionary<string, List<string>>
                {
                    ["Owner"] =
                    [
                        "TeamA",
                        "TeamC"
                    ], // Multiple values for a trait
                    ["Priority"] = ["Medium"],
                    ["Component"] = ["UI"]
                }
            }
        };
        var settings = new GdUnit4Settings { DisplayName = DisplayNameOptions.FullyQualifiedName };

        testsExamples = testDescriptors
            .Select(descriptor =>
            {
                // Use the actual BuildTestCase method from GdUnit4TestDiscoverer
                var testCase = GdUnit4TestDiscoverer.BuildTestCase(descriptor, settings);
                return testCase;
            })
            .ToList();

        testNamespace_ExampleTestSuiteA_TestA = testsExamples[0];
        testNamespace_ExampleTestSuiteA_TestB = testsExamples[1];
        otherNamespace_ExampleTestSuiteB_TestA = testsExamples[2];
        otherNamespace_ExampleTestSuiteB_TestB = testsExamples[3];
    }

    [TestMethod]
    public void PropertyProvider()
    {
        var provider = TestCaseExtensions.GetPropertyProvider();

        Assert.AreEqual(TestCaseExtensions.NamespaceProperty, provider.Invoke("Namespace"));
        Assert.AreEqual(TestCaseExtensions.ClassProperty, provider.Invoke("Class"));
        Assert.AreEqual(TestCaseProperties.DisplayName, provider.Invoke("Name"));
        Assert.AreEqual(TestCaseProperties.FullyQualifiedName, provider.Invoke("FullyQualifiedName"));
        Assert.AreEqual(TestCaseExtensions.RequireRunningGodotEngineProperty, provider.Invoke("RequireRunningGodotEngine"));
        Assert.AreEqual(TestCaseExtensions.TestCategoryProperty, provider.Invoke("TestCategory"));
    }

    [TestMethod]
    public void GetPropertyValueTestCategoryAsProperty()
    {
        var test = new TestCase("Test1", new Uri(GdUnit4TestExecutor.EXECUTOR_URI), "GdUnit4Net");
        test.SetPropertyValue(TestCaseExtensions.TestCategoryProperty, new[] { "CategoryA", "CategoryB" });
        var value = test.GetPropertyValue("TestCategory") as string[];

        CollectionAssert.AreEqual(new[] { "CategoryA", "CategoryB" }, value);
    }

    [TestMethod]
    public void FilterNone()
    {
        var filteredTests = new TestCaseFilter(new RunContext(), Logger).Execute(testsExamples);
        Assert.AreEqual(testsExamples.Count, filteredTests.Count);
        CollectionAssert.AllItemsAreInstancesOfType(filteredTests, typeof(TestCase));
    }

    [TestMethod]
    public void FilterByFullyQualifiedName()
    {
        // Create a run context with a filter for FullyQualifiedName
        var runContext = new TestRunContext().WithFilter("FullyQualifiedName=TestNamespace.ExampleTestSuiteA.TestA");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        CollectionAssert.AreEquivalent(new[] { testNamespace_ExampleTestSuiteA_TestA }, filteredTests);
    }

    [TestMethod]
    public void FilterByFullyQualifiedNamePattern()
    {
        // Create a run context with a filter using ~ (contains) operator
        var runContext = new TestRunContext().WithFilter("FullyQualifiedName~ExampleTestSuiteA");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        CollectionAssert.AreEquivalent(new[] { testNamespace_ExampleTestSuiteA_TestA, testNamespace_ExampleTestSuiteA_TestB }, filteredTests);
    }

    [TestMethod]
    public void FilterByDisplayName()
    {
        var runContext = new TestRunContext().WithFilter("Name=TestA");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        Assert.AreEqual(2, filteredTests.Count);
        Assert.IsTrue(filteredTests.All(t => t.DisplayName == "TestA"));
    }

    [TestMethod]
    public void FilterByClass()
    {
        var runContext = new TestRunContext().WithFilter("Class=TestNamespace.ExampleTestSuiteA");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        CollectionAssert.AreEquivalent(new[] { testNamespace_ExampleTestSuiteA_TestA, testNamespace_ExampleTestSuiteA_TestB }, filteredTests);
    }

    [TestMethod]
    public void FilterByNamespace()
    {
        var runContext = new TestRunContext().WithFilter("Namespace=TestNamespace");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        CollectionAssert.AreEquivalent(new[] { testNamespace_ExampleTestSuiteA_TestA, testNamespace_ExampleTestSuiteA_TestB }, filteredTests);
    }

    [TestMethod]
    public void FilterByTestCategoryAttribute()
    {
        var runContext = new TestRunContext().WithFilter("TestCategory=UnitTest");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        // verify the correct tests are filtered
        CollectionAssert.AreEquivalent(new[] { testNamespace_ExampleTestSuiteA_TestA, otherNamespace_ExampleTestSuiteB_TestB }, filteredTests);
    }

    [TestMethod]
    public void FilterByMultipleCategories()
    {
        var runContext = new TestRunContext().WithFilter("TestCategory=UnitTest&TestCategory=Fast");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        CollectionAssert.AreEquivalent(new[] { testNamespace_ExampleTestSuiteA_TestA }, filteredTests);
    }

    [TestMethod]
    public void FilterByTraitWithSingleValue()
    {
        var runContext = new TestRunContext().WithFilter("Trait.Owner=TeamA");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        Assert.AreEqual(2, filteredTests.Count);
        Assert.IsTrue(
            filteredTests.All(t =>
                t.Traits.Any(tr => tr.Name == "Owner" && tr.Value == "TeamA")));
    }

    [TestMethod]
    public void FilterByTraitContains()
    {
        var runContext = new TestRunContext().WithFilter("Trait.Component~Data");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        CollectionAssert.AreEquivalent(new[] { testNamespace_ExampleTestSuiteA_TestB }, filteredTests);
    }

    [TestMethod]
    public void FilterByMultipleTraits()
    {
        var runContext = new TestRunContext().WithFilter("Trait.Owner=TeamC&Trait.Priority=Low");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        CollectionAssert.AreEquivalent(new[] { otherNamespace_ExampleTestSuiteB_TestA }, filteredTests);
    }

    [TestMethod]
    public void FilterByCombiningCategoryAndTrait()
    {
        var runContext = new TestRunContext().WithFilter("TestCategory=UnitTest&Trait.Component=UI");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        CollectionAssert.AreEquivalent(new[] { otherNamespace_ExampleTestSuiteB_TestB }, filteredTests);
    }

    [TestMethod]
    public void FilterWithOrOperator()
    {
        var runContext = new TestRunContext().WithFilter("TestCategory=UnitTest|TestCategory=SlowTest");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        CollectionAssert.AreEquivalent(
            new[] { testNamespace_ExampleTestSuiteA_TestA, otherNamespace_ExampleTestSuiteB_TestA, otherNamespace_ExampleTestSuiteB_TestB },
            filteredTests);
    }

    [TestMethod]
    public void FilterWithNotOperator()
    {
        var runContext = new TestRunContext().WithFilter("TestCategory!=IntegrationTest");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        Assert.AreEqual(3, filteredTests.Count);
        Assert.IsFalse(
            filteredTests.Any(t =>
                t.Traits.Any(trait => trait.Name == "Category" && trait.Value == "IntegrationTest")));
    }

    [TestMethod]
    public void FilterWithComplexExpression()
    {
        var runContext = new TestRunContext().WithFilter("(Namespace=TestNamespace|Trait.Feature=Reporting)&TestCategory!=IntegrationTest");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        CollectionAssert.AreEquivalent(new[] { testNamespace_ExampleTestSuiteA_TestA, otherNamespace_ExampleTestSuiteB_TestA }, filteredTests);
    }

    [TestMethod]
    public void FilterWithVeryComplexExpressionCombiningMultipleTraits()
    {
        var runContext = new TestRunContext().WithFilter("(TestCategory=UnitTest|Trait.Priority=Low)&(Trait.Owner=TeamA|Trait.Owner=TeamC)&TestCategory!=IntegrationTest");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        CollectionAssert.AreEquivalent(
            new[] { testNamespace_ExampleTestSuiteA_TestA, otherNamespace_ExampleTestSuiteB_TestA, otherNamespace_ExampleTestSuiteB_TestB },
            filteredTests);
    }

    [TestMethod]
    public void InvalidFilterWithUnbalancedParentheses()
    {
        // Unbalanced parentheses in the expression
        var runContext = new TestRunContext().WithFilter("(TestCategory=UnitTest&(Namespace=TestNamespace");
        var filteredTests = new TestCaseFilter(runContext, Logger).Execute(testsExamples);

        // Should handle this gracefully rather than throwing exceptions
        Assert.AreEqual(testsExamples.Count, filteredTests.Count);
    }

    private sealed class TestRunContext : RunContext
    {
        public TestRunContext WithFilter(string filter)
        {
            // Set the FilterExpressionWrapper property on the RunContext
            var property = typeof(DiscoveryContext).GetProperty(
                "FilterExpressionWrapper",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var filterExpressionWrapper = new FilterExpressionWrapper(filter);
            property?.SetValue(this, filterExpressionWrapper);

            return this;
        }
    }
}
