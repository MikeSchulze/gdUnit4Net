<h2 align="center">The Unit Testing Framework in C# for Godot</h2>
<p align="center">This version of GdUnit4.api is based on Godot <strong>v4.3.stable.mono.official [77dcf97d8]</strong> (master branch)</p>

<h1 align="center">Supported Godot Versions</h1>
<p align="center">
  <img src="https://img.shields.io/badge/Godot-v4.3.0-%23478cbf?logo=godot-engine&logoColor=cyian&color=green">
  <img src="https://img.shields.io/badge/Godot-v4.4.0-%23478cbf?logo=godot-engine&logoColor=cyian&color=green">
</p>

<h1 align="center">Supported .NET Versions</h1>
<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet&logoColor=white">
  <img src="https://img.shields.io/badge/.NET-9.0-blue?logo=dotnet&logoColor=white">
  <img src="https://img.shields.io/badge/C%23-12.0-239120?logo=csharp&logoColor=white">
</p>

## What is gdUnit4Net

This project provides an API and a VS test adapter to run your Godot C# test in Visual Studio (Code) and JetBrains Rider.


<div style="border: 2px solid #FFD700; background-color: #545454; padding: 15px; border-radius: 8px; margin: 20px 0;">
  <table>
    <tr>
      <td style="vertical-align: top; padding-right: 10px; font-size: 32px;">
        ⚠️
      </td>
      <td>
        <strong style="color: #FFD700; font-size: 18px; animation: blinker 1s linear infinite;">ADVICE</strong><br>
        The documentation references version 5.0.0, which is not yet officially released. 
This is intentional as these changes will be part of the upcoming 5.0.0 release.
      </td>
    </tr>
  </table>
</div>

### Main Features

* Writing, executing and debugging tests
* Wide range of assertion methods for verifying the behavior and output of your code
* Parameterized Tests (Test Cases) for testing functions with multiple sets of inputs and expected outputs
* Scene runner for simulating different kinds of inputs and actions, such as mouse clicks and keyboard inputs<br>
  For example, you can simulate mouse clicks and keyboard inputs by calling the appropriate methods on the runner instance. Additionally, you can wait for a specific signal to be
  emitted by the scene, or you can wait for a specific function to return a certain value.
* Visual Studio Test Adapter to run and debug your tests
* Powerful test filtering capabilities to selectively run tests based on various criteria

There are three packages available in this project:

* **[gdUnit4.api](Api/README.md)** - The core package to enable writing and running unit tests in C#.
* **[gdUnit4.test.adapter](TestAdapter/README.md)** - The test adapter to integrate GdUnit4 with Visual Studio Test Platform.
* **[gdUnit4.analyzers](Analyzers/README.md)** - A Roslyn-based analyzer that provides compile-time validation for GdUnit4 test attributes.

## gdunit4.api

Checkout the [gdUnit4.api README](Api/README.md) to install and use the core testing framework.

## gdUnit4.test.adapter

Checkout the [gdUnit4.test.adapter README](TestAdapter/README.md) to install and use the test adapter. This adapter now supports
powerful [test filtering capabilities](TestAdapter/TestFilterGuide.md) for selectively running tests based on specific criteria.

## gdUnit4.analyzers

For compile-time validation of your test code, check out the [gdUnit4.analyzers README](Analyzers/README.md).

### Example Project

This [example project](https://github.com/MikeSchulze/gdUnit4Net/tree/master/example) gives you a short insight into how to set up a Godot project to use the GdUnit4 API and test
adapter.
It contains a single test suite as an example with two tests, the first test will succeed and the second test will fail.

```csharp
namespace GdUnit4.Tests
{
    using static Assertions;

    [TestSuite]
    public class StringAssertTest
    {
        [TestCase]
        public void IsEqual()
        {
            AssertThat("This is a test message").IsEqual("This is a test message");
        }
        
        [TestCase] 
        [RequireGodotRuntime] // ← Add this for Godot-dependent tests
        public void IsEqual()
        {
             AssertThat(new Node2D()).IsNotNull();
        }
        
        [Test]
        [RequireGodotRuntime]
        [GodotExceptionMonitor]  // ← Monitor Godot exceptions
        public void TestNodeCallback()
        {
            var node = new MyNode(); // Will catch exceptions in _Ready()
            AddChild(node);
        }
        
        [Test]
        [DataPoint(nameof(TestData))]  // ← Data-driven tests
        public void TestCalculations(int a, int b, int expected)
        {
            AssertThat(Calculator.Add(a, b)).IsEqual(expected);
        }
        
        [Test]
        [ThrowsException(typeof(ArgumentNullException), "Value cannot be null")]
        public void TestValidation()
        {
            Calculator.Add(null, 5); // Expects specific exception
        }
        
        // Data source for parameterized tests
        public static IEnumerable<object[]> TestData => new[]
        {
            new object[] { 1, 2, 3 },
            new object[] { 5, 7, 12 }
        };
    }
 }
```

The test run looks like this.
<p align="center"><a href="https://github.com/MikeSchulze/gdUnit4Net"><img src="https://github.com/MikeSchulze/gdUnit4Net/blob/master/Example/assets/TestExplorerRun.png" width="100%"/></p><br/>

## Documentation

<p align="left" style="font-family: Bedrock; font-size:21pt; color:#7253ed; font-style:bold">
  <a href="https://mikeschulze.github.io/gdUnit4/first_steps/install/">How to Install GdUnit</a>
</p>

<p align="left" style="font-family: Bedrock; font-size:21pt; color:#7253ed; font-style:bold">
  <a href="https://mikeschulze.github.io/gdUnit4/">API Documentation</a>
</p>

<p align="left" style="font-family: Bedrock; font-size:21pt; color:#7253ed; font-style:bold">
  <a href="TestAdapter/TestFilterGuide.md">Test Filtering Guide</a>
</p>

### You Are Welcome To

* [Give Feedback](https://github.com/MikeSchulze/gdUnit4Net/discussions)
* [Suggest Improvements](https://github.com/MikeSchulze/gdUnit4Net/issues/new?assignees=MikeSchulze&labels=enhancement&template=feature_request.md&title=)
* [Report a Bug related to GdUnit4 API](https://github.com/MikeSchulze/gdUnit4Net/issues/new?assignees=MikeSchulze&labels=bug%2Cgdunit4.api&projects=projects%2F6&template=bug_gdunit4_api.yaml&title=GD-XXX%3A+Describe+the+issue+briefly)
* [Report a Bug related to GdUnit4 Test Adapter](https://github.com/MikeSchulze/gdUnit4Net/issues/new?assignees=MikeSchulze&labels=bug%2Cgdunit4.test.adapter&projects=projects%2F6&template=bug_gdunit4_test_adapter.yaml&title=GD-XXX%3A+Describe+the+issue+briefly)

---

### Contribution Guidelines

**Thank you for your interest in contributing to GdUnit4!**<br>
To ensure a smooth and collaborative contribution process, please review our [contribution guidelines](https://github.com/MikeSchulze/gdUnit4Net/blob/master/CONTRIBUTING.md) before
getting started. These guidelines outline the standards and expectations we uphold in this project.

Code of Conduct: We strictly adhere to the Godot code of conduct in this project. As a contributor, it is important to respect and follow this code to maintain a positive and
inclusive community.

Using GitHub Issues: We utilize GitHub issues for tracking feature requests and bug reports. If you have a general question or wish to engage in discussions, we recommend joining
the [GdUnit Discord Server](https://discord.gg/rdq36JwuaJ) for specific inquiries.

We value your input and appreciate your contributions to make GdUnit4 even better!

<p align="left">
  <a href="https://discord.gg/rdq36JwuaJ"><img src="https://discordapp.com/api/guilds/885149082119733269/widget.png?style=banner4" alt="Join GdUnit Server"/></a>
</p>

### Thank you for supporting my project

## Sponsors

[<img src="https://avatars.githubusercontent.com/u/4674635?v=4)" alt="Jeff" width="125"/>](https://github.com/jlb0170) Jeff

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
