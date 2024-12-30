namespace NUnit.Extension.GdUnit4.Runners;

using NUnit.Engine;

public class GodotProcessRunner : ITestEngineRunner
{
    public void Dispose() => throw new NotImplementedException();

    public TestEngineResult Load() => throw new NotImplementedException();

    public void Unload() => throw new NotImplementedException();

    public TestEngineResult Reload() => throw new NotImplementedException();

    public int CountTestCases(TestFilter filter) => throw new NotImplementedException();

    public TestEngineResult Run(ITestEventListener listener, TestFilter filter) => throw new NotImplementedException();

    public AsyncTestEngineResult RunAsync(ITestEventListener listener, TestFilter filter) => throw new NotImplementedException();

    public void StopRun(bool force) => throw new NotImplementedException();

    public TestEngineResult Explore(TestFilter filter) => throw new NotImplementedException();
}
