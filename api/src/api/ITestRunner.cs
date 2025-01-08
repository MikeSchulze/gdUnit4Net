namespace GdUnit4.Api;

using System;
using System.Collections.Generic;
using System.Threading;

using Core.Events;

internal interface ITestRunner : IAsyncDisposable
{
    internal void RunAndWait(List<TestSuiteNode> testSuiteNodes, ITestEventListener eventListener, CancellationToken cancellationToken);

    internal void Cancel();
}
