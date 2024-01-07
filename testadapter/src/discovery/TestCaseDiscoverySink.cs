using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace GdUnit4.TestAdapter.Discovery;

class TestCaseDiscoverySink : ITestCaseDiscoverySink
{
    private readonly ConcurrentBag<TestCase> testCases = new();

    public IReadOnlyList<TestCase> TestCases => testCases.OrderBy(tc => tc.FullyQualifiedName).ToList();

    public void SendTestCase(TestCase test) => testCases.Add(test);
}
