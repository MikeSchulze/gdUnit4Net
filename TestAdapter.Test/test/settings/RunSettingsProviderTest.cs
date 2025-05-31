namespace GdUnit4.TestAdapter.Test.Settings;

using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestAdapter.Settings;

[TestClass]
public class RunSettingsProviderTest
{
    private const string XML_SETTINGS_WITHOUT_ENV =
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

    private const string XML_SETTINGS_WITHOUT_ENVIRONMENT_VARIABLES_ENTRY =
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

    private const string XML_SETTINGS =
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
        var environmentVariables = RunSettingsProvider.GetEnvironmentVariables(XML_SETTINGS);

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
        var environmentVariables = RunSettingsProvider.GetEnvironmentVariables(XML_SETTINGS_WITHOUT_ENV);

        var expected = new Dictionary<string, string>();
        CollectionAssert.AreEqual(expected, environmentVariables);
    }

    [TestMethod]
    public void GetEnvironmentVariablesWithoutEnvironmentVariablesEntry()
    {
        var environmentVariables = RunSettingsProvider.GetEnvironmentVariables(XML_SETTINGS_WITHOUT_ENVIRONMENT_VARIABLES_ENTRY);

        var expected = new Dictionary<string, string>();
        CollectionAssert.AreEqual(expected, environmentVariables);
    }
}
