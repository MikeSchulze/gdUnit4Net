// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Core;
using Core.Discovery;
using Core.Execution;
using Core.Extensions;

using Godot;
using Godot.Collections;

/// <summary>
///     The Godot Editor bridge to run C# tests inside the Godot Editor.
/// </summary>
// ReSharper disable all MemberCanBePrivate.Global
public partial class GdUnit4NetApiGodotBridge : RefCounted
{
    public static Dictionary CreateTestSuite(string sourcePath, int lineNumber, string testSuitePath)
    {
        var result = GdUnitTestSuiteBuilder.Build(NormalizedPath(sourcePath), lineNumber, NormalizedPath(testSuitePath));

        // we need to return the original resource name of the test suite on Godot site e.g. `res://foo/..` or `user://foo/..`
        if (result.ContainsKey("path"))
            result["path"] = testSuitePath;
        return result.ToGodotDictionary();
    }

    public static bool IsTestSuite(CSharpScript script)
    {
        var type = GdUnitTestSuiteBuilder.ParseType(NormalizedPath(script.ResourcePath), true);
        return type != null && Attribute.IsDefined(type, typeof(TestSuiteAttribute));
    }

    public static List<TestCaseDescriptor> DiscoverTestsFromScript(CSharpScript sourceScript) =>
        TestCaseDiscoverer.DiscoverTestCasesFromScript(sourceScript);

    public static Task ExecuteAsync(List<TestSuiteNode> testSuiteNodes, Callable eventListener, CancellationToken cancellationToken) =>
        new GdUnit4RuntimeExecutorGodotBridge().ExecuteAsync(testSuiteNodes, eventListener, cancellationToken);

    private static string NormalizedPath(string path) =>
        path.StartsWith("res://") || path.StartsWith("user://") ? ProjectSettings.GlobalizePath(path) : path;

    public static string Version() => Assembly.GetAssembly(typeof(GdUnit4NetApiGodotBridge))!.GetName()!.Version!.ToString();
}
