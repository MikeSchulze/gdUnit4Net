
<h2 align="center">A Unit Testing Framework in C# for Godot</h2>
<p align="center">This version of GdUnit4 is based on Godot <strong>v4.2.1.stable.official [b09f793f5]</strong> (master branch)</p>
</h2>

<h1 align="center">Supported Godot Versions</h2>
<p align="center">
  <img src="https://img.shields.io/badge/Godot-v4.2.0-%23478cbf?logo=godot-engine&logoColor=cyian&color=green">
  <img src="https://img.shields.io/badge/Godot-v4.2.1-%23478cbf?logo=godot-engine&logoColor=cyian&color=green">
</p>

## What is gdUnit4Net

This project provides an API and a VS test adapter to run your Godot C# test in Visual Studio (Code) and JetBrains Rider.

### Main Features

* Writing, executing and debugging tests
* Configurable template for generating new test-suites when creating test-cases
* Wide range of assertion methods for verifying the behavior and output of your code
* Parameterized Tests (Test Cases) for testing functions with multiple sets of inputs and expected outputs
* Scene runner for simulating different kinds of inputs and actions, such as mouse clicks and keyboard inputs<br>
  For example, you can simulate mouse clicks and keyboard inputs by calling the appropriate methods on the runner instance. Additionally, you can wait for a specific signal to be emitted by the scene, or you can wait for a specific function to return a certain value.
* Visual Studio Test Adapter to run and debug your tests

There are two packages you need to install

* **gdUnit4.api** is the package to enable GdUnit4 to write unit tests in C#.
* **gdUnit4.test.adapter** is the GdUnit4 Test Adapter, designed to facilitate the integration of GdUnit4 with test frameworks supporting the Visual Studio Test Platform.

## Using the gdunit4.api

Checkout the [readme](api/README.md) to install the `gdunit4.api` package.

## Install the gdUnit4.test.adapter

Checkout the [readme](testadapter/README.md) to install the `gdunit4.test.adapater` package.

### Example Project

This [example project](https://github.com/MikeSchulze/gdUnit4Net/tree/master/example) gives you a short insight into how to set up a Godot project to use the GdUnit4 API and test adapter.
It contains a single test suite as an example with two tests, the first test will succeed and the second test will fail.

```c#
namespace Examples;

using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class ExampleTest
{
    [TestCase]
    public void success()
    {
        AssertBool(true).IsTrue();
    }


    [TestCase]
    public void failed()
    {
        AssertBool(false).IsTrue();
    }

}
```

The test run looks like this.
<p align="center"><a href="https://github.com/MikeSchulze/gdUnit4Net"><img src="https://github.com/MikeSchulze/gdUnit4Net/blob/master/example/assets/TestExplorerRun.png" width="100%"/></p><br/>

## Documentation

<p align="left" style="font-family: Bedrock; font-size:21pt; color:#7253ed; font-style:bold">
  <a href="https://mikeschulze.github.io/gdUnit4/first_steps/install/">How to Install GdUnit</a>
</p>

<p align="left" style="font-family: Bedrock; font-size:21pt; color:#7253ed; font-style:bold">
  <a href="https://mikeschulze.github.io/gdUnit4/">API Documentation</a>
</p>

### You Are Welcome To

* [Give Feedback](https://github.com/MikeSchulze/gdUnit4Net/discussions)
* [Suggest Improvements](https://github.com/MikeSchulze/gdUnit4Net/issues/new?assignees=MikeSchulze&labels=enhancement&template=feature_request.md&title=)
* [Report a Bug related to GdUnit4 API](https://github.com/MikeSchulze/gdUnit4Net/issues/new?assignees=MikeSchulze&labels=bug%2Cgdunit4.api&projects=projects%2F6&template=bug_gdunit4_api.yaml&title=GD-XXX%3A+Describe+the+issue+briefly)
* [Report a Bug related to GdUnit4 Test Adapter](https://github.com/MikeSchulze/gdUnit4Net/issues/new?assignees=MikeSchulze&labels=bug%2Cgdunit4.test.adapter&projects=projects%2F6&template=bug_gdunit4_test_adapter.yaml&title=GD-XXX%3A+Describe+the+issue+briefly)

---

### Contribution Guidelines

**Thank you for your interest in contributing to GdUnit4!**<br>
To ensure a smooth and collaborative contribution process, please review our [contribution guidelines](https://github.com/MikeSchulze/gdUnit4Net/blob/master/CONTRIBUTING.md) before getting started. These guidelines outline the standards and expectations we uphold in this project.

Code of Conduct: We strictly adhere to the Godot code of conduct in this project. As a contributor, it is important to respect and follow this code to maintain a positive and inclusive community.

Using GitHub Issues: We utilize GitHub issues for tracking feature requests and bug reports. If you have a general question or wish to engage in discussions, we recommend joining the [GdUnit Discord Server](https://discord.gg/rdq36JwuaJ) for specific inquiries.

We value your input and appreciate your contributions to make GdUnit4 even better!

<p align="left">
  <a href="https://discord.gg/rdq36JwuaJ"><img src="https://discordapp.com/api/guilds/885149082119733269/widget.png?style=banner4" alt="Join GdUnit Server"/></a>
</p>

### Thank you for supporting my project

---

## Sponsors
