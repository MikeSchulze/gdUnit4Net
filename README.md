# This is the initial repository and not yet ready for use

---

## What is GdUnit4

gdUnit4.api is the C# Test api to run GdUnit4 C# tests.
gdUnit4.testadapter is the vs test adapter to run GdUnit4 C# tests inside visual studio (code).

## Setup

Add this package references to your project

```
    <ItemGroup>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
        <PackageReference Include="gdUnit4.api" Version="4.2.0-rc*" />
        <PackageReference Include="gdUnit4.testadapter" Version="1.0.0-rc*" />
    </ItemGroup>
```

## Run tests from console

`dotnet vstest /Settings:test/.runsettings /TestAdapterPath:testadapter/bin/Debug/net7.0/ /Tests:GdUnit4.Tests.Asserts.StringAssertTest test/.godot/mono/temp/bin/Debug/gdUnit4Test.dll`
