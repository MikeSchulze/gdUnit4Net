namespace GdUnit4.Tests.Core.Discovery;

using GdUnit4.Core.Discovery;

using static Assertions;

[TestSuite]
public class TestCaseDiscovererTest
{
    [TestCase]
    public void DiscoverSingleTestCase()
    {
        var clazzType = typeof(ExampleTestSuite);
        var methodInfo = clazzType.GetMethod("SingleTestCase")!;
        var tests = TestCaseDiscoverer.DiscoverTestCasesFromMethod(methodInfo, "/path/to/test_assembly.dll", clazzType.FullName!);

        AssertThat(tests).ContainsExactly(new TestCaseDescriptor
        {
            SimpleName = "SingleTestCase",
            FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite.SingleTestCase",
            AssemblyPath = "/path/to/test_assembly.dll",
            ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite",
            ManagedMethod = "SingleTestCase",
            Id = tests[0].Id,
            LineNumber = 0,
            AttributeIndex = 0,
            RequireRunningGodotEngine = false
        });
    }

    [TestCase]
    public void DiscoverSingleTestCaseWithCustomName()
    {
        var clazzType = typeof(ExampleTestSuite);
        var methodInfo = clazzType.GetMethod("SingleTestCaseWithCustomName")!;
        var tests = TestCaseDiscoverer.DiscoverTestCasesFromMethod(methodInfo, "/path/to/test_assembly.dll", clazzType.FullName!);

        AssertThat(tests).ContainsExactly(new TestCaseDescriptor
        {
            SimpleName = "TestA",
            FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite.TestA",
            AssemblyPath = "/path/to/test_assembly.dll",
            ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite",
            ManagedMethod = "SingleTestCaseWithCustomName",
            Id = tests[0].Id,
            LineNumber = 0,
            AttributeIndex = 0,
            RequireRunningGodotEngine = false
        });
    }

    [TestCase]
    public void DiscoverMultiRowTestCase()
    {
        var clazzType = typeof(ExampleTestSuite);
        var methodInfo = clazzType.GetMethod("MultiRowTestCase")!;
        var tests = TestCaseDiscoverer.DiscoverTestCasesFromMethod(methodInfo, "/path/to/test_assembly.dll", clazzType.FullName!);

        AssertThat(tests)
            .HasSize(3)
            .ContainsExactly(
                new TestCaseDescriptor
                {
                    SimpleName = "MultiRowTestCase #0",
                    FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite.MultiRowTestCase.MultiRowTestCase (0)",
                    AssemblyPath = "/path/to/test_assembly.dll",
                    ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite",
                    ManagedMethod = "MultiRowTestCase",
                    Id = tests[0].Id,
                    LineNumber = 0,
                    AttributeIndex = 0,
                    RequireRunningGodotEngine = false
                },
                new TestCaseDescriptor
                {
                    SimpleName = "MultiRowTestCase #1",
                    FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite.MultiRowTestCase.MultiRowTestCase (1)",
                    AssemblyPath = "/path/to/test_assembly.dll",
                    ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite",
                    ManagedMethod = "MultiRowTestCase",
                    Id = tests[1].Id,
                    LineNumber = 0,
                    AttributeIndex = 1,
                    RequireRunningGodotEngine = false
                },
                new TestCaseDescriptor
                {
                    SimpleName = "MultiRowTestCase #2",
                    FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite.MultiRowTestCase.MultiRowTestCase (2)",
                    AssemblyPath = "/path/to/test_assembly.dll",
                    ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite",
                    ManagedMethod = "MultiRowTestCase",
                    Id = tests[2].Id,
                    LineNumber = 0,
                    AttributeIndex = 2,
                    RequireRunningGodotEngine = false
                }
            );
    }


    [TestCase]
    public void DiscoverMultiRowTestCaseWithCustomNames()
    {
        var clazzType = typeof(ExampleTestSuite);
        var methodInfo = clazzType.GetMethod("MultiRowTestCaseWithCustomTestName")!;
        var tests = TestCaseDiscoverer.DiscoverTestCasesFromMethod(methodInfo, "/path/to/test_assembly.dll", clazzType.FullName!);

        AssertThat(tests)
            .HasSize(3)
            .ContainsExactly(
                new TestCaseDescriptor
                {
                    SimpleName = "TestA #0",
                    FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite.MultiRowTestCaseWithCustomTestName.TestA (0)",
                    AssemblyPath = "/path/to/test_assembly.dll",
                    ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite",
                    ManagedMethod = "MultiRowTestCaseWithCustomTestName",
                    Id = tests[0].Id,
                    LineNumber = 0,
                    AttributeIndex = 0,
                    RequireRunningGodotEngine = false
                },
                new TestCaseDescriptor
                {
                    SimpleName = "TestB #1",
                    FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite.MultiRowTestCaseWithCustomTestName.TestB (1)",
                    AssemblyPath = "/path/to/test_assembly.dll",
                    ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite",
                    ManagedMethod = "MultiRowTestCaseWithCustomTestName",
                    Id = tests[1].Id,
                    LineNumber = 0,
                    AttributeIndex = 1,
                    RequireRunningGodotEngine = false
                },
                new TestCaseDescriptor
                {
                    SimpleName = "TestC #2",
                    FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite.MultiRowTestCaseWithCustomTestName.TestC (2)",
                    AssemblyPath = "/path/to/test_assembly.dll",
                    ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuite",
                    ManagedMethod = "MultiRowTestCaseWithCustomTestName",
                    Id = tests[2].Id,
                    LineNumber = 0,
                    AttributeIndex = 2,
                    RequireRunningGodotEngine = false
                }
            );
    }
}

internal class ExampleTestSuite
{
    [TestCase]
    public void SingleTestCase() { }

    [TestCase(TestName = "TestA")]
    public void SingleTestCaseWithCustomName() { }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void MultiRowTestCase(int value) { }


    [TestCase(0, TestName = "TestA")]
    [TestCase(1, TestName = "TestB")]
    [TestCase(2, TestName = "TestC")]
    public void MultiRowTestCaseWithCustomTestName(int value) { }
}
