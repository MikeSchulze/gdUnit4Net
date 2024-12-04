namespace NUnit.Extension.GdUnit4;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

using VisualStudio.TestAdapter;

public class GdUnit4TestAdapter : NUnitTestAdapter, ITestExecutor
{
    public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle) => throw new NotImplementedException();

    public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle) => throw new NotImplementedException();

    public void Cancel() => throw new NotImplementedException();
}
