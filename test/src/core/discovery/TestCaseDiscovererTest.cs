namespace GdUnit4.Tests.Core.Discovery;

using System.Collections.Generic;
using System.Threading.Tasks;

using GdUnit4.Core.Discovery;

using Mono.Cecil;
using Mono.Cecil.Cil;

using static Assertions;

public sealed class ExampleTestSuiteToDiscover
{
    [TestCase]
    public void SingleTestCase()
    {
    }

    [TestCase(TestName = "TestA")]
    public void SingleTestCaseWithCustomName()
    {
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void MultiRowTestCase(int value)
    {
    }


    [TestCase(0, TestName = "TestA")]
    [TestCase(1, TestName = "TestB")]
    [TestCase(2, TestName = "TestC")]
    public void MultiRowTestCaseWithCustomTestName(int value)
    {
    }

    [TestCase]
    public async Task ThreadedTestCase()
        => await Task.CompletedTask;

    [TestCase]
    public async Task<int> ThreadedTestCaseTyped()
    {
        await Task.CompletedTask;
        return 0;
    }
}

[TestSuite]
public class TestCaseDiscovererTest
{
    private readonly string codeFilePath = DiscoverTestUtils.GetSourceFilePath("src/core/discovery/TestCaseDiscovererTest.cs");

    [TestCase]
    public void DiscoverSingleTestCase()
    {
        var tests = DiscoverTests<ExampleTestSuiteToDiscover>(nameof(ExampleTestSuiteToDiscover.SingleTestCase));

        AssertThat(tests).ContainsExactly(new TestCaseDescriptor
        {
            SimpleName = "SingleTestCase",
            FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover.SingleTestCase",
            AssemblyPath = "/path/to/test_assembly.dll",
            ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover",
            ManagedMethod = "SingleTestCase",
            Id = tests[0].Id,
            LineNumber = 17,
            CodeFilePath = codeFilePath,
            AttributeIndex = 0,
            RequireRunningGodotEngine = false
        });
    }

    [TestCase]
    public void DiscoverSingleTestCaseWithCustomName()
    {
        var tests = DiscoverTests<ExampleTestSuiteToDiscover>(nameof(ExampleTestSuiteToDiscover.SingleTestCaseWithCustomName));

        AssertThat(tests).ContainsExactly(new TestCaseDescriptor
        {
            SimpleName = "TestA",
            FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover.TestA",
            AssemblyPath = "/path/to/test_assembly.dll",
            ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover",
            ManagedMethod = "SingleTestCaseWithCustomName",
            Id = tests[0].Id,
            LineNumber = 22,
            CodeFilePath = codeFilePath,
            AttributeIndex = 0,
            RequireRunningGodotEngine = false
        });
    }

    [TestCase]
    public void DiscoverMultiRowTestCase()
    {
        var tests = DiscoverTests<ExampleTestSuiteToDiscover>(nameof(ExampleTestSuiteToDiscover.MultiRowTestCase));

        AssertThat(tests)
            .HasSize(3)
            .ContainsExactly(
                new TestCaseDescriptor
                {
                    SimpleName = "MultiRowTestCase #0",
                    FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover.MultiRowTestCase.MultiRowTestCase (0)",
                    AssemblyPath = "/path/to/test_assembly.dll",
                    ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover",
                    ManagedMethod = "MultiRowTestCase",
                    Id = tests[0].Id,
                    LineNumber = 29,
                    CodeFilePath = codeFilePath,
                    AttributeIndex = 0,
                    RequireRunningGodotEngine = false
                },
                new TestCaseDescriptor
                {
                    SimpleName = "MultiRowTestCase #1",
                    FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover.MultiRowTestCase.MultiRowTestCase (1)",
                    AssemblyPath = "/path/to/test_assembly.dll",
                    ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover",
                    ManagedMethod = "MultiRowTestCase",
                    Id = tests[1].Id,
                    LineNumber = 29,
                    CodeFilePath = codeFilePath,
                    AttributeIndex = 1,
                    RequireRunningGodotEngine = false
                },
                new TestCaseDescriptor
                {
                    SimpleName = "MultiRowTestCase #2",
                    FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover.MultiRowTestCase.MultiRowTestCase (2)",
                    AssemblyPath = "/path/to/test_assembly.dll",
                    ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover",
                    ManagedMethod = "MultiRowTestCase",
                    Id = tests[2].Id,
                    LineNumber = 29,
                    CodeFilePath = codeFilePath,
                    AttributeIndex = 2,
                    RequireRunningGodotEngine = false
                }
            );
    }

    [TestCase]
    public void DiscoverMultiRowTestCaseWithCustomNames()
    {
        var tests = DiscoverTests<ExampleTestSuiteToDiscover>(nameof(ExampleTestSuiteToDiscover.MultiRowTestCaseWithCustomTestName));

        AssertThat(tests)
            .HasSize(3)
            .ContainsExactly(
                new TestCaseDescriptor
                {
                    SimpleName = "TestA #0",
                    FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover.MultiRowTestCaseWithCustomTestName.TestA (0)",
                    AssemblyPath = "/path/to/test_assembly.dll",
                    ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover",
                    ManagedMethod = "MultiRowTestCaseWithCustomTestName",
                    Id = tests[0].Id,
                    LineNumber = 37,
                    CodeFilePath = codeFilePath,
                    AttributeIndex = 0,
                    RequireRunningGodotEngine = false
                },
                new TestCaseDescriptor
                {
                    SimpleName = "TestB #1",
                    FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover.MultiRowTestCaseWithCustomTestName.TestB (1)",
                    AssemblyPath = "/path/to/test_assembly.dll",
                    ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover",
                    ManagedMethod = "MultiRowTestCaseWithCustomTestName",
                    Id = tests[1].Id,
                    LineNumber = 37,
                    CodeFilePath = codeFilePath,
                    AttributeIndex = 1,
                    RequireRunningGodotEngine = false
                },
                new TestCaseDescriptor
                {
                    SimpleName = "TestC #2",
                    FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover.MultiRowTestCaseWithCustomTestName.TestC (2)",
                    AssemblyPath = "/path/to/test_assembly.dll",
                    ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover",
                    ManagedMethod = "MultiRowTestCaseWithCustomTestName",
                    Id = tests[2].Id,
                    LineNumber = 37,
                    CodeFilePath = codeFilePath,
                    AttributeIndex = 2,
                    RequireRunningGodotEngine = false
                }
            );
    }

    [TestCase]
    public void DiscoverThreadedTestCase()
    {
        var tests = DiscoverTests<ExampleTestSuiteToDiscover>(nameof(ExampleTestSuiteToDiscover.ThreadedTestCase));

        AssertThat(tests).ContainsExactly(new TestCaseDescriptor
        {
            SimpleName = "ThreadedTestCase #0",
            FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover.ThreadedTestCase",
            AssemblyPath = "/path/to/test_assembly.dll",
            ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover",
            ManagedMethod = "ThreadedTestCase",
            Id = tests[0].Id,
            LineNumber = 42,
            CodeFilePath = codeFilePath,
            AttributeIndex = 0,
            RequireRunningGodotEngine = false
        });
    }

    [TestCase]
    public void DiscoverThreadedTestCaseTyped()
    {
        var tests = DiscoverTests<ExampleTestSuiteToDiscover>(nameof(ExampleTestSuiteToDiscover.ThreadedTestCaseTyped));

        AssertThat(tests).ContainsExactly(new TestCaseDescriptor
        {
            SimpleName = "ThreadedTestCaseTyped #0",
            FullyQualifiedName = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover.ThreadedTestCaseTyped",
            AssemblyPath = "/path/to/test_assembly.dll",
            ManagedType = "GdUnit4.Tests.Core.Discovery.ExampleTestSuiteToDiscover",
            ManagedMethod = "ThreadedTestCaseTyped",
            Id = tests[0].Id,
            LineNumber = 46,
            CodeFilePath = codeFilePath,
            AttributeIndex = 0,
            RequireRunningGodotEngine = false
        });
    }

    private List<TestCaseDescriptor> DiscoverTests<TClassType>(string testMethod)
    {
        var clazzType = typeof(TClassType);
        var readerParameters = new ReaderParameters
        {
            ReadSymbols = true,
            SymbolReaderProvider = new PortablePdbReaderProvider()
        };
        using var assemblyDefinition = AssemblyDefinition.ReadAssembly(clazzType.Module.FullyQualifiedName, readerParameters);
        var methodDefinition = DiscoverTestUtils.FindMethodDefinition(assemblyDefinition, clazzType, testMethod);
        return TestCaseDiscoverer.DiscoverTestCasesFromMethod(methodDefinition, false, "/path/to/test_assembly.dll", clazzType.FullName!);
    }
}
