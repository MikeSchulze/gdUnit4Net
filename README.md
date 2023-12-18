# This is the initial repository and not yet ready for use

---

## What is GdUnit4

gdUnit4.api is the C# Test api to run GdUnit4 C# tests.
gdUnit4.testadapter is the vs test adapter to run GdUnit4 C# tests inside visual studio (code).

## Setup


### Precondisions
* Setup your project to include the GdUnit4 test API
    Add this package references to your project

    ```
        <ItemGroup>
            <PackageReference Include="gdUnit4.api" Version="4.2.0*" />
        </ItemGroup>
    ```


## Install the GdUnit4 Test Adapter
The GdUnit4.testadapter implements the Microsoft test adapter framework.
https://github.com/Microsoft/vstest-docs/blob/main/RFCs/0004-Adapter-Extensibility.md#adapter-specific-settings

### Precondisions
* Setup your project
    Add this package references to your project

    ```
        <ItemGroup>
            <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
            <PackageReference Include="gdUnit4.api" Version="4.2.0-rc*" />
            <PackageReference Include="gdUnit4.testadapter" Version="1.0.0-rc*" />
        </ItemGroup>
    ```
* Install the C# Dev Kit
    Detailed instacruction can be found here https://code.visualstudio.com/docs/csharp/testing
* Setup your test settings
    It is important you use the right C# Dev Kit version! (Is actual a PreRelease)
    The property is newly introduced by this issue https://github.com/microsoft/vscode-dotnettools/issues/156

    Open your settings.json and add this property to setup your custom test run settings.
    ```
        "dotnet.unitTests.runSettingsPath": "./test/.runsettings"
    ````


## Run tests from console

`dotnet vstest /Settings:test/.runsettings /TestAdapterPath:testadapter/bin/Debug/net7.0/ /Tests:GdUnit4.Tests.Asserts.StringAssertTest test/.godot/mono/temp/bin/Debug/gdUnit4Test.dll`
