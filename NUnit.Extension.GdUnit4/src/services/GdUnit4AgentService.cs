namespace NUnit.Extension.GdUnit4.Services;

using NUnit.Engine;
using NUnit.Engine.Services;

public class GdUnit4AgentService : Service
{
    private readonly Dictionary<TestPackage, GdUnit4TestAgent> agents = new();

    public GdUnit4TestAgent CreateTestAgent(TestPackage package)
    {
        var agent = new GdUnit4TestAgent();
        agents[package] = agent;
        return agent;
    }

    public void DestroyAgent(TestPackage package)
    {
        if (agents.TryGetValue(package, out var agent))
        {
            agent.Stop();
            agents.Remove(package);
        }
    }
}

public class GdUnit4TestAgent : ITestAgent
{
    // Your TestAgent implementation here
    public bool Start() => throw new NotImplementedException();

    public void Stop() => throw new NotImplementedException();

    public ITestEngineRunner CreateRunner(TestPackage package) => throw new NotImplementedException();

    public Guid Id { get; }
}
