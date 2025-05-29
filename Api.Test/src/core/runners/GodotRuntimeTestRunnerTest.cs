namespace GdUnit4.Tests.Core.Runners;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using Api;

using GdUnit4.Core.Runners;

using Moq;

using static Assertions;

/// <summary>
///     Tests for GodotRuntimeTestRunner's InstallTestRunnerClasses method
/// </summary>
[TestSuite]
public class GodotRuntimeTestRunnerTest
{
    // TODO implement the TempDirectory annotation
    // [TempDirectory]
    private string? TestTempDirectory { get; set; }

    public required string MockGodotBinPath { get; set; }
    public required Mock<IDebuggerFramework> DebuggerFrameworkMock { get; set; }
    public required Mock<ITestEngineLogger> LoggerMock { get; set; }


    /// <summary>
    ///     Set up test environment and create mocks
    /// </summary>
    [Before]
    public void Before()
    {
        // TODO remove if [TempDirectory] implemented
        TestTempDirectory = Path.Combine(Path.GetTempPath(), "test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(TestTempDirectory);

        // Create mock Godot executable - we'll use a batch script for Windows or shell script for Unix
        // Windows batch script
        MockGodotBinPath = Path.Combine(TestTempDirectory, Environment.OSVersion.Platform == PlatformID.Win32NT
            ? "mock_godot.bat"
            // Unix shell script
            : "mock_godot.sh");

        // Setup mocks
        LoggerMock = new Mock<ITestEngineLogger>();
        DebuggerFrameworkMock = new Mock<IDebuggerFramework>();
    }

    private GodotRuntimeTestRunner CreateTestRunner(int timeout) => new(
        LoggerMock.Object,
        DebuggerFrameworkMock.Object,
        new TestEngineSettings { CompileProcessTimeout = timeout });

    /// <summary>
    ///     Clean up after tests
    /// </summary>
    [After]
    public void After()
    {
        try
        {
            // Clean up temp directory if it wasn't handled by the annotation
            // TODO remove if [TempDirectory] implemented
            if (TestTempDirectory == null || !Directory.Exists(TestTempDirectory))
                return;

            Directory.Delete(TestTempDirectory, true);
        }
        catch
        {
            // Ignore cleanup failures
        }
    }

    [AfterTest]
    public void AfterTest()
    {
        // Reset mocks to clear any recorded invocations
        LoggerMock.Reset();
        DebuggerFrameworkMock.Reset();
    }

    /// <summary>
    ///     Test successful execution of InstallTestRunnerClasses
    /// </summary>
    [TestCase]
    public void TestInstallTestRunnerClassesSuccess()
    {
        // Arrange
        CreateSuccessScript();

        // Create a separate temp working directory
        var workingDirectory = Path.Combine(TestTempDirectory!, "working_dir");
        Directory.CreateDirectory(workingDirectory);

        // Act
        var result = CreateTestRunner(1000).InitRuntimeEnvironment(workingDirectory, MockGodotBinPath);

        // Assert
        AssertThat(result).OverrideFailureMessage("InstallTestRunnerClasses should return true for successful compilation").IsTrue();

        // Verify logger was called with success messages
        VerifyLoggerInfo("Installing GdUnit4 Godot Runtime Test Runner");
        VerifyLoggerInfo("Rebuild Godot Project ends with exit code: 0");

        // Verify the runner file was created in the correct location
        var runnerPath = Path.Combine(workingDirectory, GodotRuntimeTestRunner.TEMP_TEST_RUNNER_DIR, "GdUnit4TestRunnerScene.cs");
        AssertThat(File.Exists(runnerPath)).OverrideFailureMessage($"Runner file should exist at {runnerPath}").IsTrue();
    }

    /// <summary>
    ///     Test a process that takes nearly the full timeout but still completes successfully
    /// </summary>
    [TestCase]
    public void TestInstallTestRunnerClassesNearTimeout()
    {
        // Create a script that runs for 4 seconds (near the timeout but should complete)
        CreateNearTimeoutScript();

        // Create a separate temp working directory
        var workingDirectory = Path.Combine(TestTempDirectory!, "working_dir_near_timeout");
        Directory.CreateDirectory(workingDirectory);

        // Act, Set a longer timeout for this test
        var result = CreateTestRunner(5000).InitRuntimeEnvironment(workingDirectory, MockGodotBinPath);

        // Assert
        AssertThat(result).OverrideFailureMessage("InstallTestRunnerClasses should return true for a process that completes just before timeout").IsTrue();

        // Verify success messages were logged
        VerifyLoggerInfo("Installing GdUnit4 Godot Runtime Test Runner");
        VerifyLoggerInfo("Rebuild Godot Project ends with exit code: 0");

        // Verify the runner file was created and not cleaned up
        var runnerPath = Path.Combine(workingDirectory, GodotRuntimeTestRunner.TEMP_TEST_RUNNER_DIR, "GdUnit4TestRunnerScene.cs");
        AssertThat(File.Exists(runnerPath)).OverrideFailureMessage($"Runner file should exist at {runnerPath}").IsTrue();

        // Verify that no timeout error was logged
        LoggerMock.Verify(l => l.LogError(It.Is<string>(s =>
            s.Contains("Godot compilation TIMEOUT"))), Times.Never());
    }

    /// <summary>
    ///     Test timeout scenario in InstallTestRunnerClasses
    /// </summary>
    [TestCase]
    public void TestInstallTestRunnerClassesTimeout()
    {
        // Arrange
        CreateTimeoutScript();

        // Create a separate temp working directory
        var workingDirectory = Path.Combine(TestTempDirectory!, "working_dir_timeout");
        Directory.CreateDirectory(workingDirectory);

        // Act
        var result = CreateTestRunner(1000).InitRuntimeEnvironment(workingDirectory, MockGodotBinPath);

        // Assert
        AssertThat(result).OverrideFailureMessage("InstallTestRunnerClasses should return false on timeout").IsFalse();

        // Verify timeout error was logged
        VerifyLoggerError("Godot compilation TIMEOUT");
        var errorCode = Environment.OSVersion.Platform == PlatformID.Win32NT ? -1 : 137;
        VerifyLoggerError($"Rebuild Godot Project ends with exit code: {errorCode}");

        // Verify the runner file was cleaned up after timeout
        var runnerPath = Path.Combine(workingDirectory, GodotRuntimeTestRunner.TEMP_TEST_RUNNER_DIR, "GdUnit4TestRunnerScene.cs");
        AssertThat(File.Exists(runnerPath)).OverrideFailureMessage("Runner file should be cleaned up after timeout").IsFalse();
    }

    /// <summary>
    ///     Test compilation failure scenario
    /// </summary>
    [TestCase]
    public void TestInstallTestRunnerClassesCompilationFailure()
    {
        // Arrange
        CreateFailureScript();

        // Create a separate temp working directory
        var workingDirectory = Path.Combine(TestTempDirectory!, "working_dir_failure");
        Directory.CreateDirectory(workingDirectory);

        // Act
        var result = CreateTestRunner(1000).InitRuntimeEnvironment(workingDirectory, MockGodotBinPath);

        // Assert
        AssertThat(result).OverrideFailureMessage("InstallTestRunnerClasses should return false on compilation failure").IsFalse();

        // Verify error message was logged
        VerifyLoggerError("Rebuild Godot Project ends with exit code: 1");

        // Verify the runner file was cleaned up after failure
        var runnerPath = Path.Combine(workingDirectory, GodotRuntimeTestRunner.TEMP_TEST_RUNNER_DIR, "GdUnit4TestRunnerScene.cs");
        AssertThat(File.Exists(runnerPath)).OverrideFailureMessage("Runner file should be cleaned up after compilation failure").IsFalse();
    }

    #region Helper Methods

    /// <summary>
    ///     Create a script that succeeds quickly
    /// </summary>
    private void CreateSuccessScript()
    {
        var content = Environment.OSVersion.Platform == PlatformID.Win32NT
            ?
            // Windows batch file that succeeds immediately
            """
            @echo off

            echo Godot Engine v4.1.stable.mono - https://godotengine.org
            echo Godot Engine v4.1.stable.mono
            echo Running...
            echo Compilation successful!
            exit 0
            """
            :
            // Unix shell script that succeeds immediately
            """
            #!/bin/bash

            echo 'Godot Engine v4.1.stable.mono - https://godotengine.org'
            echo 'Godot Engine v4.1.stable.mono'
            echo 'Running...'
            echo 'Compilation successful!'
            exit 0
            """;

        File.WriteAllText(MockGodotBinPath, content);
        SetAsExecutable(MockGodotBinPath);
    }

    /// <summary>
    ///     Create a script that takes nearly the full timeout (4 seconds) but still completes
    /// </summary>
    private void CreateNearTimeoutScript()
    {
        var content = Environment.OSVersion.Platform == PlatformID.Win32NT
            ?
            // Windows batch file that sleeps for 4 seconds
            """
            @echo off

            echo Godot Engine v4.1.stable.mono - https://godotengine.org
            echo Godot Engine v4.1.stable.mono
            echo Running...
            echo Starting compilation (will take 4 seconds)...
            ping 127.0.0.1 -n 5 > nul
            echo Compilation complete!
            exit 0
            """
            :
            // Unix shell script that sleeps for 4 seconds
            """
            #!/bin/bash

            echo 'Godot Engine v4.1.stable.mono - https://godotengine.org'
            echo 'Godot Engine v4.1.stable.mono'
            echo 'Running...'
            echo 'Starting compilation (will take 4 seconds)...'
            sleep 5
            echo 'Compilation complete!'
            exit 0
            """;

        File.WriteAllText(MockGodotBinPath, content);
        SetAsExecutable(MockGodotBinPath);
    }

    /// <summary>
    ///     Create a script that times out
    /// </summary>
    private void CreateTimeoutScript()
    {
        var content = Environment.OSVersion.Platform == PlatformID.Win32NT
            ?
            // Windows batch file that sleeps longer than our timeout
            """
            @echo off

            echo Godot Engine v4.1.stable.mono - https://godotengine.org
            echo Godot Engine v4.1.stable.mono
            echo Running...
            ping 127.0.0.1 -n 10 > nul
            exit 0
            """
            :
            // Unix shell script that sleeps longer than our timeout
            """
            #!/bin/bash

            echo 'Godot Engine v4.1.stable.mono - https://godotengine.org'
            echo 'Godot Engine v4.1.stable.mono'
            echo 'Running...'
            sleep 10
            exit 0
            """;

        File.WriteAllText(MockGodotBinPath, content);
        SetAsExecutable(MockGodotBinPath);
    }

    /// <summary>
    ///     Create a script that fails with an error code
    /// </summary>
    private void CreateFailureScript()
    {
        var content = Environment.OSVersion.Platform == PlatformID.Win32NT
            ?
            // Windows batch file that exits with error code 1
            """
            echo off
            echo Godot Engine v4 .1.stable.mono - https: //godotengine.org
            echo Godot Engine v4 .1.stable.mono
            echo Running...
            echo
            echo ERROR: Compilation failed
            exit 1
            """
            :
            // Unix shell script that exits with error code 1
            """
            #!/bin/bash
            echo 'Godot Engine v4.1.stable.mono - https://godotengine.org'
            echo 'Godot Engine v4.1.stable.mono'
            echo 'Running...'
            echo 'ERROR: Compilation failed'
            exit 1
            """;

        File.WriteAllText(MockGodotBinPath, content);
        SetAsExecutable(MockGodotBinPath);
    }

    [SuppressMessage("Interoperability", "CA1416")]
    private static void SetAsExecutable(string filePath)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            File.SetUnixFileMode(filePath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
    }

    /// <summary>
    ///     Verify that logger.LogInfo was called with a message containing the specified text
    /// </summary>
    private void VerifyLoggerInfo(string expectedText)
    {
        try
        {
            LoggerMock.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains(expectedText))), Times.AtLeastOnce());
        }
        catch (MockException)
        {
            // Capture all invocations to provide context
            var invocations = LoggerMock.Invocations
                .Where(i => i.Method.Name == "LogInfo")
                .Select(i => i.Arguments[0]?.ToString() ?? "null")
                .ToList();

            var message = $"Expected log message containing '{expectedText}' was not found.\n\n" +
                          $"Actual LogInfo calls ({invocations.Count}):\n" +
                          string.Join("\n", invocations.Select((msg, i) => $"  {i + 1}. {msg}"));

            AssertBool(true).OverrideFailureMessage(message).IsFalse();
        }
    }

    /// <summary>
    ///     Verify that logger.LogError was called with a message containing the specified text
    /// </summary>
    private void VerifyLoggerError(string expectedText)
    {
        try
        {
            LoggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains(expectedText))), Times.AtLeastOnce());
        }
        catch (MockException)
        {
            // Capture all invocations to provide context
            var invocations = LoggerMock.Invocations
                .Where(i => i.Method.Name == "LogError")
                .Select(i => i.Arguments[0]?.ToString() ?? "null")
                .ToList();

            var message = $"Expected error message containing '{expectedText}' was not found.\n\n" +
                          $"Actual LogError calls ({invocations.Count}):\n" +
                          string.Join("\n", invocations.Select((msg, i) => $"  {i + 1}. {msg}"));

            AssertBool(true).OverrideFailureMessage(message).IsFalse();
        }
    }

    #endregion
}
