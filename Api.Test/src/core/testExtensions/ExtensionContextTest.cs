using System;
using System.Collections.Generic;
using GdUnit4.Api;
using GdUnit4.Core.TestExtensions;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
public class ExtensionContextTest
{
    private static IExtensionContext MakeContext(IExtensionContext? parentContext = null, string? methodName = null)
    {
        if (parentContext == null)
            return new ExtensionContext(
                typeof(ExtensionContextTest),
                new TestSuiteNode
                {
                    ManagedType = "",
                    Tests = [],
                    AssemblyPath = "",
                    SourceFile = "",
                    Id = Guid.NewGuid(),
                    ParentId = Guid.NewGuid()
                });

        var method = typeof(ExtensionContextTest).GetMethod(methodName ?? nameof(MakeContext));
        return new ExtensionContext(
            parentContext,
            method,
            method?.Name,
            []);
    }

}
