using Godot;
using System;
using GdUnit3.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace GdUnit3
{
    public partial class GdUnit3MonoAPI : Reference
    {
        public static Godot.Collections.Dictionary CreateTestSuite(string sourcePath, int lineNumber, string testSuitePath)
        {
            var result = GdUnitTestSuiteBuilder.Build(NormalisizePath(sourcePath), lineNumber, NormalisizePath(testSuitePath));
            // we need to return the original resource name of the test suite on Godot site e.g. `res://foo/..` or `user://foo/..`
            if (result.ContainsKey("path"))
                result["path"] = testSuitePath;
            return new Godot.Collections.Dictionary(result);
        }

        public static bool IsTestSuite(string classPath)
        {
            var type = GdUnitTestSuiteBuilder.ParseType(NormalisizePath(classPath));
            return type != null ? Attribute.IsDefined(type, typeof(TestSuiteAttribute)) : false;
        }


        public static Godot.Node? ParseTestSuite(string classPath) => GdUnitTestSuiteBuilder.Load(NormalisizePath(classPath));

        public static GdUnit3.IExecutor Executor(Godot.Node listener) =>
            new GdUnit3.Executions.Executor().AddGdTestEventListener(listener);

        private static string NormalisizePath(string path) =>
             (path.StartsWith("res://") || path.StartsWith("user://")) ? Godot.ProjectSettings.GlobalizePath(path) : path;

        private static IEnumerable<GdUnit3.Executions.TestCase> LoadTestCases(Type type) => type.GetMethods()
            .Where(m => m.IsDefined(typeof(TestCaseAttribute)))
            .Select(mi =>
            {
                TestCaseAttribute testCaseAttribute = mi.GetCustomAttribute<TestCaseAttribute>();
                return new GdUnit3.Executions.TestCase(mi, testCaseAttribute.Line);
            });
    }
}
