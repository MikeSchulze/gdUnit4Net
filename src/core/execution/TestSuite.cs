using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using GdUnit3.Core;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GdUnit3.Executions
{
    internal sealed class TestSuite : IDisposable
    {
        private Lazy<IEnumerable<Executions.TestCase>> _testCases = new Lazy<IEnumerable<Executions.TestCase>>();

        public int TestCaseCount => TestCases.Count<Executions.TestCase>();

        public IEnumerable<Executions.TestCase> TestCases => _testCases.Value;

        public string ResourcePath { get; set; }

        public string Name { get; set; }
        public string FullName => GetType().FullName;

        public object Instance { get; set; }

        public bool FilterDisabled { get; set; } = false;

        public TestSuite(string classPath, List<string>? includedTests = null)
        {
            Type? type = GdUnitTestSuiteBuilder.ParseType(classPath);
            if (type == null)
                throw new ArgumentException($"Can't parse testsuite {classPath}");
            Instance = Activator.CreateInstance(type);
            Name = type.Name;
            ResourcePath = classPath;
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(classPath)).WithFilePath(classPath).GetCompilationUnitRoot();
            // we do lazy loding to only load test case one times
            _testCases = new Lazy<IEnumerable<Executions.TestCase>>(() => LoadTestCases(type, syntaxTree, includedTests));
        }

        public TestSuite(Type type)
        {
            Instance = Activator.CreateInstance(type);
            Name = type.Name;
            ResourcePath = Assembly.GetAssembly(type).Location;
            // we do lazy loding to only load test case one times
            _testCases = new Lazy<IEnumerable<Executions.TestCase>>(() => LoadTestCases(type, null));
        }

        private IEnumerable<Executions.TestCase> LoadTestCases(Type type, CompilationUnitSyntax? syntaxTree, List<string>? includedTests = null)
        {
            return type.GetMethods()
                .Where(m => m.IsDefined(typeof(TestCaseAttribute)))
                .Where(m => includedTests?.Contains(m.Name) ?? true)
                .Select(mi =>
                {
                    var lineNumber = syntaxTree != null ? GdUnitTestSuiteBuilder.TestCaseLineNumber(syntaxTree, mi.Name) : -1;
                    return new Executions.TestCase(mi, lineNumber);
                });
        }

        public void Dispose()
        {
            if (Instance is IDisposable)
                ((IDisposable)Instance).Dispose();
        }
    }
}
