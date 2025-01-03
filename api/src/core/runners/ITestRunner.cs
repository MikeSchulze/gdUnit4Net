namespace GdUnit4.Core.Runners;

using System;
using System.Collections.Generic;
using System.Threading;

using Api;

using Events;

internal interface ITestRunner : IAsyncDisposable
{
    internal void RunAndWait(List<TestSuiteNode> testSuiteNodes, ITestEventListener eventListener, CancellationToken cancellationToken);

    internal void Cancel();
}
