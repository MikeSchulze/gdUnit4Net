# The C# GdUnit4 Analyzers

## What is GdUnit4.Analyzer

GdUnit4.Analyzer is a Roslyn-based analyzer package designed to enhance the development experience when writing tests with GdUnit4. It provides compile-time validation for GdUnit4
test attributes and helps developers catch configuration errors early in the development process.

## Features

The analyzer is automatically included when referencing the gdUnit4.api package - no additional configuration required:

`<PackageReference Include="gdUnit4.api" Version="4.4.0-rc3" />`

* Attribute Validation

  The analyzer enforces correct usage of GdUnit4 test attributes:

  #### Example: DataPoint and TestCase Combination Validation

  Validates proper combination of DataPoint and TestCase attributes:

    ```csharp
    // ✅ Valid: Single TestCase with DataPoint
    [TestCase]
    [DataPoint(nameof(TestData))]
    public void ValidTest(int a, int b) { }
    
    // ❌ Invalid: Multiple TestCase with DataPoint
    [TestCase]
    [TestCase]                         // GdUnit0201 error: Method 'InvalidTest' cannot have multiple TestCase attributes when DataPoint attribute is present
    [DataPoint(nameof(TestData))]
    public void InvalidTest(int a, int b) { }
    ```

## Technical Details

The analyzer is built using:

* .NET Standard 2.0
* Roslyn Analyzer Framework
* Microsoft.CodeAnalysis.CSharp

### You are welcome to

* [Give Feedback](https://github.com/MikeSchulze/gdUnit4Net/discussions)
* [Suggest Improvements](https://github.com/MikeSchulze/gdUnit4Net/issues/new?assignees=MikeSchulze&labels=enhancement&template=feature_request.md&title=)
* [Report Bugs](https://github.com/MikeSchulze/gdUnit4Net/issues/new?assignees=MikeSchulze&labels=bug%2C+task&template=bug_report.md&title=)
