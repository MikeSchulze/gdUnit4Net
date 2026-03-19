// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Tests.Core.TestExtensions;

using System;

using GdUnit4.Core.TestExtensions;

using static GdUnit4.Assertions;

[TestSuite]
public class SceneRunnerExtensionErrorPathTest
{
    [TestCase]
    public void TestBeforeEachThrowsWhenNoPathProvided()
    {
        var extension = new SceneRunnerExtension(); // no default path
        var context = new ExtensionContext(
            typeof(SceneRunnerExtensionErrorPathTest),
            this,
            null,
            nameof(TestBeforeEachThrowsWhenNoPathProvided),
            []); // no TestCase arguments — no string path to consume

        AssertThrown(() => extension.BeforeEach(context))
            .IsInstanceOf<InvalidOperationException>()
            .HasMessage(
                $"[{nameof(TestBeforeEachThrowsWhenNoPathProvided)}]: No scene path provided. " +
                "Set a default via [RegisterExtension] or specify per-test via [TestCase(\"res://...\")].");
    }
}
