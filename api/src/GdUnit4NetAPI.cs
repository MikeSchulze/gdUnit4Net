namespace GdUnit4;

using System;
using System.Reflection;

using GdUnit4.Core;

/// <summary>
/// The Godot Editor bridge to run C# tests inside the Godot Editor
/// </summary>
public partial class GdUnit4NetAPI : Godot.RefCounted
{
    public static Godot.Collections.Dictionary CreateTestSuite(string sourcePath, int lineNumber, string testSuitePath)
    {
        var result = GdUnitTestSuiteBuilder.Build(NormalizedPath(sourcePath), lineNumber, NormalizedPath(testSuitePath));
        // we need to return the original resource name of the test suite on Godot site e.g. `res://foo/..` or `user://foo/..`
        if (result.ContainsKey("path"))
            result["path"] = testSuitePath;
        return result.ToGodotDictionary();
    }

    public static bool IsTestSuite(string classPath)
    {
        var type = GdUnitTestSuiteBuilder.ParseType(NormalizedPath(classPath), true);
        return type != null && Attribute.IsDefined(type, typeof(TestSuiteAttribute));
    }

    public static CsNode? ParseTestSuite(string classPath) => GdUnitTestSuiteBuilder.Load(NormalizedPath(classPath));

    public static Godot.RefCounted Executor(Godot.Node listener) =>
        (Godot.RefCounted)new Executions.Executor().AddGdTestEventListener(listener);

    private static string NormalizedPath(string path) =>
         (path.StartsWith("res://") || path.StartsWith("user://")) ? Godot.ProjectSettings.GlobalizePath(path) : path;

    public static string Version() => Assembly.GetAssembly(typeof(GdUnit4NetAPI))!.GetName()!.Version!.ToString();
}
