# The C# GdUnit4 API

## What is GdUnit4.Api

gdUnit4.api is the C# package to enable GdUnit4 to run/write unit tests in C#.

## Features

* **Writing And Executing Tests** in C# for Net7.0 and Net8.0
* **Convenient interface** for running test-suites directly from Godot<br>
  One of the main features of GdUnit4 is the ability to run test-suites directly from the Godot editor using the context menu. You can run test-suites from the FileSystem panel,
  the ScriptEditor, or the GdUnit Inspector. To do this, simply right-click on the desired test-suite or test-case and select "Run Test(s)" from the context menu. This will run the
  selected tests and display the results in the GdUnit Inspector.<br>
  You can create new test cases directly from the ScriptEditor by right-clicking on the function you want to test and selecting "Create TestCase" from the context menu.
* **Fluent Syntax** for writing test cases that's easy to read and understand
* **Wide range of Assertions** for verifying the behavior and output of your code
* **Test Fuzzing support:** for generating random inputs to test edge cases and boundary conditions
* **Parameterized Tests:** (Test Cases) for testing functions with multiple sets of inputs and expected outputs
* **Dynamic Tests Data:** 'DataPoint' attribute to define test data sets
* **Scene runner:** for simulating different kinds of inputs and actions, such as mouse clicks and keyboard inputs<br>
  For example, you can simulate mouse clicks and keyboard inputs by calling the appropriate methods on the runner instance. Additionally, you can wait for a specific signal to be
  emitted by the scene, or you can wait for a specific function to return a certain value.
* **GdUnit4.Analyzer:** is a Roslyn-based analyzer package designed to enhance the development experience when writing tests with GdUnit4. It provides compile-time validation for
  GdUnit4
  test attributes and helps developers catch configuration errors early in the development process.

## Short Example

```
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
    }
 }
```

---

### You are welcome to

* [Give Feedback](https://github.com/MikeSchulze/gdUnit4Net/discussions)
* [Suggest Improvements](https://github.com/MikeSchulze/gdUnit4Net/issues/new?assignees=MikeSchulze&labels=enhancement&template=feature_request.md&title=)
* [Report Bugs](https://github.com/MikeSchulze/gdUnit4Net/issues/new?assignees=MikeSchulze&labels=bug%2C+task&template=bug_report.md&title=)

### Thank you for supporting my project

---

## Sponsors
