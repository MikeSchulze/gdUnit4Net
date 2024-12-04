namespace GdUnit4.Core;

using System;
using System.Collections.Generic;

using core.runners;

using Events;

internal interface ITestRunner : IDisposable
{
    internal void RunAndWait(ITestEventListener eventListener, IList<GdUnitTestCase> tests);

    internal void Cancel();
}
