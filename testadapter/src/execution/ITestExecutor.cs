namespace GdUnit4.TestAdapter.Execution;

using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

internal interface ITestExecutor : IDisposable
{
    public const int DefaultSessionTimeout = 30000;

    public void Run(IFrameworkHandle frameworkHandle, IRunContext runContext, IReadOnlyList<TestCase> testCases);

    public void Cancel();
}
