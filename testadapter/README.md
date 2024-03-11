
# GdUnit4 Test Adapter

This is the GdUnit4 Test Adapter, designed to facilitate the integration of GdUnit4 with test frameworks supporting the Visual Studio Test Platform.

## Getting Started


### Preconditions
* Install the C# Dev Kit. Detailed instructions can be found [here](https://code.visualstudio.com/docs/csharp/testing).
* Setup your test settings:
  - It is important to use the correct C# Dev Kit version, which is currently a PreRelease. The property is newly introduced by this [issue](https://github.com/microsoft/vscode-dotnettools/issues/156).
  - For **Visual Studio Code** prjects open your `.vscode/settings.json` and add the following property to set up your custom test run settings:
    ```json
    "dotnet.unitTests.runSettingsPath": "./test/.runsettings"
    ```
  - For **Visual Studio** projects you need to add this settings in your project file:
    ```
    <PropertyGroup>
      <RunSettingsFilePath>$(MSBuildProjectDirectory)\.runsettings</RunSettingsFilePath>
    </PropertyGroup>
    ```
  - And you need to setup the Godot run environment `GODOT_BIN`, the full path to the godot executable.
    Check the .settings you have set the **EnvironmentVariables**

## Install the gdunit NuGet Packages
1. Add the `gdunit4.api` project reference to your test project:

   ```bash
   dotnet add package gdunti.api
   ```
2. Add the `gdunit4.test.adapter` project reference to your test project:

   ```bash
   dotnet add package gdunit4.test.adapter
   ```

## Manually Add the gdunit NuGet Packages
Configure your test project to use GdUnit4 by adding the following to your .csproj file:

```xml
<Project Sdk="Godot.NET.Sdk">

    <!-- ... other project settings ... -->

    <ItemGroup>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
        <PackageReference Include="gdUnit4.api" Version="<version>" />
        <ProjectReference Include="gdUnit4.test.adapter" Version="<version>"/>
    </ItemGroup>

</Project>
```

## .runsettings Configuration

To configure GdUnit4 test execution, you can use a .runsettings file. Below is an example .runsettings file:
The full guide to configure the settings can be found [here](https://learn.microsoft.com/en-us/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file?view=vs-2022)

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
    <RunConfiguration>
        <MaxCpuCount>1</MaxCpuCount>
        <ResultsDirectory>./TestResults</ResultsDirectory>
        <TargetFrameworks>net7.0;net8.0</TargetFrameworks>
        <TestSessionTimeout>180000</TestSessionTimeout>
        <TreatNoTestsAsError>true</TreatNoTestsAsError>
        <EnvironmentVariables>
            <GODOT_BIN>c:\programs\Godot_v4.2.1-stable_mono_win64.exe</GODOT_BIN>
        </EnvironmentVariables>
    </RunConfiguration>

    <LoggerRunSettings>
        <Loggers>
            <Logger friendlyName="console" enabled="True">
                <Configuration>
                    <Verbosity>detailed</Verbosity>
                </Configuration>
            </Logger>
            <Logger friendlyName="html" enabled="True">
                <Configuration>
                    <LogFileName>test-result.html</LogFileName>
                </Configuration>
            </Logger>
            <Logger friendlyName="trx" enabled="True">
                <Configuration>
                    <LogFileName>test-result.trx</LogFileName>
                </Configuration>
            </Logger>
        </Loggers>
    </LoggerRunSettings>

    <GdUnit4>
        <!-- Additonal Godot runtime parameters-->
        <Parameters></Parameters>
        <!-- Controlls the Display name attribute of the TestCase. Allowed values are SimpleName and FullyQualifiedName.
             This likely determines how the test names are displayed in the test results.-->
        <DisplayName>FullyQualifiedName</DisplayName>
    </GdUnit4>
</RunSettings>
```

Ensure to customize the values inside the `<Parameters>` element based on your specific requirements. This configuration is crucial for successful test execution, especially in headless environments.


## Run Tests from Terminal

`dotnet test exampleProject.csproj --settings .runsettings`

## Contributing

If you encounter issues or have suggestions for improvements, please feel free to open an issue or submit a pull request on the [GitHub repository](https://github.com/MikeSchulze/gdUnit4Mono/issues/new/choose).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---
