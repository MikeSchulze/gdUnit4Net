using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace GdUnit4.TestAdapter.Discovery;

class TestCaseDiscoverySink : ITestCaseDiscoverySink
{
    public LinkedList<TestCase> TestCases { get; private set; } = new LinkedList<TestCase>();

    public void SendTestCase(TestCase test) => TestCases.AddLast(test);
}
