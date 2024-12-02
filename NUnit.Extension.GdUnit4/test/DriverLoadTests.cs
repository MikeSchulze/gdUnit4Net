namespace NUnit.Extension.GdUnit4;

using System;
using System.Reflection;

using Driver;

using Framework;

using NUnit.Engine;

public class DriverLoadTests
{
    private IDriverService _driverService;
    private ITestEngine _engine;

    [OneTimeSetUp]
    public void Setup()
    {
        _engine = TestEngineActivator.CreateInstance();
        _engine.Initialize();
        _driverService = _engine.Services.GetService<IDriverService>();
    }

    [Test]
    public void ShouldLoadGdUnit4Driver()
    {
        // Get the current test assembly path
        var assemblyPath = Assembly.GetExecutingAssembly().Location;

        // Try to get our driver with all required parameters
        var driver = _driverService.GetDriver(AppDomain.CurrentDomain, assemblyPath, "net8.0", true);

        // Verify it's our driver type
        Assert.That(driver, Is.Not.Null);
        Assert.That(driver, Is.TypeOf<GdUnit4Driver>());
    }


    [OneTimeTearDown]
    public void Cleanup() => _engine.Dispose();
}
