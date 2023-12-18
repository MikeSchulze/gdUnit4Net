using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace GdUnit4.TestAdapter.Execution;

internal interface ITestExecutor
{

    public const int DEFAULT_SESSION_TIMEOUT = 30000;

    public void Run(IFrameworkHandle frameworkHandle, IRunContext runContext, IEnumerable<TestCase> testCases);

    public void Cancel();
}
