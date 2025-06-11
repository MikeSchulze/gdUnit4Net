// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.TestAdapter.Discovery;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

internal sealed class TestCaseDiscoverySink : ITestCaseDiscoverySink
{
    private readonly ConcurrentBag<TestCase> testCases = new();

    public IReadOnlyList<TestCase> TestCases => testCases.OrderBy(tc => tc.FullyQualifiedName).ToList();

    public void SendTestCase(TestCase test) => testCases.Add(test);
}
