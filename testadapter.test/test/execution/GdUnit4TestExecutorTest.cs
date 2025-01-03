namespace GdUnit4.TestAdapter.Test.Execution;

using System;

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

[TestClass]
public class GdUnit4TestExecutorTest
{
    private static readonly string XmlSettings =
        """
        <?xml version="1.0" encoding="utf-8"?>
        <RunSettings>
            <RunConfiguration>
                <MaxCpuCount>1</MaxCpuCount>
                <TestAdaptersPaths>.</TestAdaptersPaths>
                <ResultsDirectory>./TestResults</ResultsDirectory>
                <TargetFrameworks>net7.0;net8.0</TargetFrameworks>
                <!-- set default session timeout to 5m -->
                <TestSessionTimeout>500000</TestSessionTimeout>
                <TreatNoTestsAsError>true</TreatNoTestsAsError>
                <EnvironmentVariables>
                    <TestEnvironmentA>23</TestEnvironmentA>
                    <TestEnvironmentB>42</TestEnvironmentB>
                </EnvironmentVariables>
            </RunConfiguration>
        </RunSettings>
        """;

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable("TestEnvironmentA", null);
        Environment.SetEnvironmentVariable("TestEnvironmentB", null);
    }

    [TestMethod]
    public void SetupRunnerEnvironment()
    {
        // pretest the environment is not set
        Assert.AreEqual(Environment.GetEnvironmentVariable("TestEnvironmentA"), null);
        Environment.SetEnvironmentVariable("TestEnvironmentB", "666");

        var frameworkHandle = new Mock<IFrameworkHandle>().Object;

        // Setup mock RunContext with RunSettings
        var mockRunContext = new Mock<IRunContext>();
        mockRunContext.SetupGet(rc => rc.RunSettings)
            .Returns(Mock.Of<IRunSettings>(rs => rs.SettingsXml == XmlSettings));

        // run
        GdUnit4TestExecutor.SetupRunnerEnvironment(mockRunContext.Object, frameworkHandle);

        // verify the TestEnvironmentA=23 and TestEnvironmentB is overwritten by 42
        Assert.AreEqual(Environment.GetEnvironmentVariable("TestEnvironmentA"), "23");
        Assert.AreEqual(Environment.GetEnvironmentVariable("TestEnvironmentB"), "42");
    }
}
