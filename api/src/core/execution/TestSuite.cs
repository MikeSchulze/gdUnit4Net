namespace GdUnit4.Executions;

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using GdUnit4.Core;

internal sealed class TestSuite : IDisposable
{
    private readonly Lazy<IEnumerable<TestCase>> testCases = new();

    public int TestCaseCount => TestCases.Count();

    public IEnumerable<TestCase> TestCases => testCases.Value;

    public string ResourcePath { get; set; }

    public string Name { get; set; }

    public string? FullName => GetType().FullName;

    public object Instance { get; set; }

    public bool FilterDisabled { get; set; }

    public TestSuite(string classPath, IEnumerable<string>? includedTests = null, bool checkIfTestSuite = true, bool primitiveFilter = false)
    {
        var type = GdUnitTestSuiteBuilder.ParseType(classPath, checkIfTestSuite)
            ?? throw new ArgumentException($"Can't parse testsuite {classPath}");
        Instance = Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Cannot create an instance of '{type.FullName}' because it does not have a public parameterless constructor.");

        Name = type.Name;
        ResourcePath = classPath;
        var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(classPath)).WithFilePath(classPath).GetCompilationUnitRoot();
        // we do lazy loading to only load test case one times
        testCases = new Lazy<IEnumerable<TestCase>>(() => LoadTestCases(type, syntaxTree, includedTests, primitiveFilter));
    }

    private List<TestCase> LoadTestCases(Type type, CompilationUnitSyntax? syntaxTree, IEnumerable<string>? includedTests = null, bool primitiveFilter = false)
        => type.GetMethods()
            .Where(m => m.IsDefined(typeof(TestCaseAttribute)))
            .Where(FilterByTestCaseName(includedTests, primitiveFilter))
            .Select(mi =>
            {
                var lineNumber = syntaxTree != null ? GdUnitTestSuiteBuilder.TestCaseLineNumber(syntaxTree, mi.Name) : -1;
                return new TestCase(mi, lineNumber);
            })
            .ToList();

    /// <summary>
    /// filters test by given list, if primitiveFilter is set we do simply filter by test name ignoring the arguments
    /// TODO: the primitiveFilter is just a temporary workaround to run selective c# tests from Godot Editor
    /// </summary>
    /// <param name="includedTests"></param>
    /// <param name="primitiveFilter"></param>
    /// <returns></returns>
    private Func<MethodInfo, bool> FilterByTestCaseName(IEnumerable<string>? includedTests, bool primitiveFilter)
        => mi =>
        {
            var testCases = mi.GetCustomAttributes(typeof(TestCaseAttribute))
                .Cast<TestCaseAttribute>()
                .Select(attr => primitiveFilter ? mi.Name : TestCase.BuildFullyQualifiedName(mi.DeclaringType!.FullName!, mi.Name, attr))
                .ToList();
            return includedTests?.Any(testName => testCases.Contains(testName)) ?? true;
        };

    public void Dispose()
    {
        if (Instance is IDisposable disposable)
            disposable.Dispose();
    }
}
