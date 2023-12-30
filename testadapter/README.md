
# GdUnit4 Test Adapter

This is the GdUnit4 Test Adapter, designed to facilitate the integration of GdUnit4 with test frameworks supporting the Visual Studio Test Platform.

## Getting Started


### Precondisions
* Install the C# Dev Kit
    Detailed instacruction can be found here https://code.visualstudio.com/docs/csharp/testing
* Setup your test settings
    It is important you use the right C# Dev Kit version! (Is actual a PreRelease)
    The property is newly introduced by this issue https://github.com/microsoft/vscode-dotnettools/issues/156

    Open your settings.json and add this property to setup your custom test run settings.
    ```
        "dotnet.unitTests.runSettingsPath": "./test/.runsettings"
    ````

1. Install the GdUnit4 Test Adapter NuGet package:

   ```bash
   dotnet add package GdUnit4.TestAdapter
   ```

2. Add a reference to the GdUnit4 library in your test project:

   ```bash
   dotnet add package GdUnit4
   ```

3. Configure your test project to use GdUnit4 by adding the following to your `.csproj` file:

```xml
<Project Sdk="Godot.NET.Sdk">

    <!-- ... other project settings ... -->

    <ItemGroup>
        <PackageReference Include="gdUnit4.api" Version="your-version" />
        <ProjectReference Include="gdUnit4.test.adapter" Version="your-version"/>
    </ItemGroup>

</Project>
```

## .runsettings Configuration

To configure GdUnit4 test execution, you can use a `.runsettings` file. Below is an example `.runsettings` file:

```xml
<GdUnit4>
    <!-- Additional Godot runtime parameters -->
    <!-- These parameters are crucial for configuring the Godot runtime to work in headless environments,
         such as those used in automated testing or CI/CD pipelines. -->
    <Parameters>--verbose</Parameters>
    
    <!-- Controls the Display name attribute of the TestCase.
         Allowed values are SimpleName and FullyQualifiedName.
         This likely determines how the test names are displayed in the test results. -->
    <DisplayName>SimpleName</DisplayName>
</GdUnit4>
```

Ensure to customize the values inside the `<Parameters>` element based on your specific requirements. This configuration is crucial for successful test execution, especially in headless environments.


## Run tests from console

`dotnet test ./test/your-project.csproj --settings ./test/.runsettings`

## Contributing

If you encounter issues or have suggestions for improvements, please feel free to open an issue or submit a pull request on the [GitHub repository](https://github.com/MikeSchulze/gdUnit4Mono/issues/new/choose).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---
