
<h1 align="center">GdUnit4Mono <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/v/release/MikeSchulze/gdunit4mono" width="12%"> </h1>
<h2 align="center">The C# extension for GdUnit4</h2>

<p align="center">
  <img src="https://img.shields.io/badge/Godot-v4.x.x-%23478cbf?logo=godot-engine&logoColor=cyian&color=red">
</p>

<p align="center"><a href="https://github.com/MikeSchulze/gdUnit4"><img src="https://github.com/MikeSchulze/gdUnit4/blob/master/assets/gdUnit4-animated.gif" width="100%"/></p><br/>


<p align="center">
  <img alt="GitHub branch checks state" src="https://img.shields.io/github/checks-status/MikeSchulze/gdunit4mono/master"></br>
</p>


# --------------------------------------------------------
# This is the initial repository and not yet ready for use
# --------------------------------------------------------



## What is GdUnit4Mono
GdUnit4Mono is the C# extention to enable GdUnit4 to run/write unit tests in C#.
 
 
## Features
* Configurable template for the creation of a new test-suite
* A spacious set of Asserts use to verify your code
* Fluent syntax support
* Test Fuzzing support
* Provides a scene runner to simulate interactions on a scene 
  * Simulate by Input events like mouse and/or keyboard
  * Simulate scene processing by a certain number of frames
  * Simulate scene processing by waiting for a specific signal

 
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

## Documentation
<p align="left" style="font-family: Bedrock; font-size:21pt; color:#7253ed; font-style:bold">
  <a href="https://mikeschulze.github.io/gdUnit4/first_steps/install/#gdunit4-and-c">How to Install GdUnit4Mono</a>
</p>

<p align="left" style="font-family: Bedrock; font-size:21pt; color:#7253ed; font-style:bold">
  <a href="https://mikeschulze.github.io/gdUnit4/">API Documentation</a>
</p>



---

### You are welcome to:
  * [Give Feedback](https://github.com/MikeSchulze/gdUnit4Mono/discussions)
  * [Suggest Improvements](https://github.com/MikeSchulze/gdUnit4Mono/issues/new?assignees=MikeSchulze&labels=enhancement&template=feature_request.md&title=)
  * [Report Bugs](https://github.com/MikeSchulze/gdUnit4Mono/issues/new?assignees=MikeSchulze&labels=bug%2C+task&template=bug_report.md&title=)



<h1 align="center"></h1>
<p align="left">
  <img alt="GitHub issues" src="https://img.shields.io/github/issues/MikeSchulze/gdUnit4Mono">
  <img alt="GitHub closed issues" src="https://img.shields.io/github/issues-closed-raw/MikeSchulze/gdUnit4Mono"></br>
  <!-- <img src="https://img.shields.io/packagecontrol/dm/SwitchDictionary.svg">
  <img src="https://img.shields.io/packagecontrol/dt/SwitchDictionary.svg">
   -->
  <img alt="GitHub top language" src="https://img.shields.io/github/languages/top/MikeSchulze/gdUnit4Mono">
  <img alt="GitHub code size in bytes" src="https://img.shields.io/github/languages/code-size/MikeSchulze/gdUnit4Mono">
  <img src="https://img.shields.io/badge/License-MIT-blue.svg">
</p>

<p align="left">
  <a href="https://discord.gg/rdq36JwuaJ"><img src="https://discordapp.com/api/guilds/885149082119733269/widget.png?style=banner4" alt="Join GdUnit4 Server"/></a>
</p>

### Thank you for supporting my project!
---
## Sponsors:
<!-- [<img src="https://github.com/musicm122.png" alt="musicm122" width="125"/>](https://github.com/musicm122) -->



