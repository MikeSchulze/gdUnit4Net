namespace GdUnit4.TestAdapter.Test.settings;

using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Settings;

[TestClass]
public class RunSettingsProviderTest
{
    private static readonly string XmlSettingsWithoutEnv =
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
                </EnvironmentVariables>
            </RunConfiguration>
        </RunSettings>
        """;

    private static readonly string XmlSettingsWithoutEnvironmentVariablesEntry =
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
            </RunConfiguration>
        </RunSettings>
        """;


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
                    <GODOT_BIN>D:\development\Godot_v4.2.2-stable_mono_win64\Godot_v4.2.2-stable_mono_win64.exe</GODOT_BIN>
                    <EnvValue>42</EnvValue>
                </EnvironmentVariables>
            </RunConfiguration>
        </RunSettings>
        """;


    [TestMethod]
    public void GetEnvironmentVariables()
    {
        var environmentVariables = RunSettingsProvider.GetEnvironmentVariables(XmlSettings);

        var expected = new Dictionary<string, string>
        {
            { "GODOT_BIN", "D:\\development\\Godot_v4.2.2-stable_mono_win64\\Godot_v4.2.2-stable_mono_win64.exe" },
            { "EnvValue", "42" }
        };
        CollectionAssert.AreEqual(expected, environmentVariables);
    }

    [TestMethod]
    public void GetEnvironmentVariablesWithoutEnv()
    {
        var environmentVariables = RunSettingsProvider.GetEnvironmentVariables(XmlSettingsWithoutEnv);

        var expected = new Dictionary<string, string>();
        CollectionAssert.AreEqual(expected, environmentVariables);
    }

    [TestMethod]
    public void GetEnvironmentVariablesWithoutEnvironmentVariablesEntry()
    {
        var environmentVariables = RunSettingsProvider.GetEnvironmentVariables(XmlSettingsWithoutEnvironmentVariablesEntry);

        var expected = new Dictionary<string, string>();
        CollectionAssert.AreEqual(expected, environmentVariables);
    }
}
