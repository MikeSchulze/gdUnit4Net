namespace GdUnit4.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discovery;

internal sealed class GdUnit4TestEngine : ITestEngine
{
    public GdUnit4TestEngine(TestEngineSettings settings, ITestEngineLogger logger)
    {
        Settings = settings;
        Logger = logger;
    }

    private TestEngineSettings Settings { get; }
    private ITestEngineLogger Logger { get; }


    public Task<int> Execute(List<ITestCase> testCases) => throw new NotImplementedException();

    public List<TestCaseDescriptor> Discover(string testAssembly) => TestCaseDiscoverer.Discover(Settings, Logger, testAssembly);
}
