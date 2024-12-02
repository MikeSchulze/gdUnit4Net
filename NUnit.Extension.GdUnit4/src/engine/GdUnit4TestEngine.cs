namespace NUnit.Extension.GdUnit4.Engine;

using NUnit.Engine;
using NUnit.Engine.Internal;

using Services;

using InternalTraceLevel = NUnit.Engine.InternalTraceLevel;

public class GdUnit4TestEngine : ITestEngine
{
    private static readonly Logger Log = InternalTrace.GetLogger(typeof(GdUnit4TestEngine));

    private readonly ServiceContext serviceLocator = new();


    public void Dispose()
    {
        // dispose all services
        serviceLocator.ServiceManager.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Initialize()
    {
        Log.Info("Initializing GdUnit4TestEngine");
        serviceLocator.ServiceManager.AddService(new GdUnit4AgentService());
        serviceLocator.ServiceManager.StartServices();
    }

    public ITestRunner GetRunner(TestPackage package) => serviceLocator.GetService<ITestRunner>();

    public IServiceLocator Services => serviceLocator;

    public string WorkDirectory { get; set; } = Environment.CurrentDirectory;

    public InternalTraceLevel InternalTraceLevel { get; set; } = InternalTraceLevel.Verbose;
}
