# GdUnit4 Test Adapter

This is the GdUnit4 Test Adapter, designed to facilitate the integration of GdUnit4 with test frameworks supporting the Visual Studio Test Platform.

The GdUnit4.testadapter implements the Microsoft test adapter framework [VSTest](https://github.com/microsoft/vstest?tab=readme-ov-file#vstest).

## Features

- **Seamless Integration** with Visual Studio, VS Code, and JetBrains Rider
- **Test Discovery** to find all GdUnit4 tests in your project
- **Test Execution** to run tests directly from your IDE
- **Test Debugging** to step through your tests
- **Error Navigation** to jump directly to test failures
- **Solution Configuration** with test config files
- **Powerful Test Filtering** to selectively run tests based on various criteria

## Supported IDE's

| IDE                                | Test Discovery | Test Run | Test Debug | Jump to Failure | Solution test config file | Test Filter | Parallel Test Execution |
|------------------------------------|----------------|----------|------------|-----------------|---------------------------|-------------|-------------------------|
| Visual Studio                      | ✅              | ✅        | ✅          | ✅               | ✅                         | ✅           | ❌                       |
| Visual Studio Code                 | ✅              | ✅        | ✅          | ✅               | ✅                         | ✅           | ❌                       |
| JetBrains Rider min version 2024.2 | ✅              | ✅        | ✅          | ✅               | ✅                         | ✅           | ❌                       |

> ✅ - supported<br>
> ☑️ - supported by a workaround (link)<br>
> ❌ - not supported<br>

## Installation

Add the Test Adapter to your test project:

```xml
<PackageReference Include="gdUnit4.test.adapter" Version="2.1.0" />
```

## Test Filtering

GdUnit4 now supports powerful test filtering capabilities, allowing you to selectively run tests based on various criteria such as test name, class, namespace, category, and custom
traits.

### Basic Filter Syntax

```
PropertyName=Value
```

### Example Filters

```
# Run only tests in a specific class
Class=CalculatorTests

# Run tests with a specific category
TestCategory=UnitTest

# Run tests with specific traits
Trait.Priority=High

# Combine filters with logical operators
TestCategory=UnitTest&Trait.Owner=TeamA
```

For detailed information about test filtering capabilities, including syntax, operators, examples, and best practices, see the [Test Filter Guide](TestFilterGuide.md).

## Related Packages

* [gdUnit4.api](../api/README.md) - The core testing framework
* [gdUnit4.analyzers](../Analyzers/README.md) - Add compile-time validation for your test code

## Documentation

The full documentation can be found [here](https://mikeschulze.github.io/gdUnit4/csharp_project_setup/vstest-adapter/).

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

## License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

---
