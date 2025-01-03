namespace GdUnit4.TestAdapter.Test.Discovery;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

[TestClass]
public class GdUnit4TestDiscovererTest
{
    private static readonly string XmlSettings =
        """
        <?xml version="1.0" encoding="utf-8"?>
        <RunSettings>
            <RunConfiguration>
                <MaxCpuCount>1</MaxCpuCount>
                <TestAdaptersPaths>.</TestAdaptersPaths>
                <ResultsDirectory>./TestResults</ResultsDirectory>
                <TargetFrameworks>net7.0;net8.0</TargetFrameworks>
                <!-- set default session timeout to 5m -->
                <TestSessionTimeout>500000</TestSessionTimeout>
                <TreatNoTestsAsError>true</TreatNoTestsAsError>
                <EnvironmentVariables>
                    <TestEnvironmentA>23</TestEnvironmentA>
                    <TestEnvironmentB>42</TestEnvironmentB>
                </EnvironmentVariables>
            </RunConfiguration>

            <GdUnit4>
                <Parameters></Parameters>
                <DisplayName>FullyQualifiedName</DisplayName>
            </GdUnit4>
        </RunSettings>
        """;

    [TestMethod]
    public void DiscoverOnNoTestAssembly()
    {
        var frameworkHandle = new Mock<IFrameworkHandle>();
        var logMessages = new List<string>();

        frameworkHandle
            .Setup(logger => logger.SendMessage(It.IsAny<TestMessageLevel>(), It.IsAny<string>()))
            .Callback<TestMessageLevel, string>((level, message) => logMessages.Add($"{level}: {message}"));

        // Setup the mock to capture discovered tests
        var mockDiscoverySink = new Mock<ITestCaseDiscoverySink>();
        var discoveredTests = new List<TestCase>();
        mockDiscoverySink
            .Setup(ds => ds.SendTestCase(It.IsAny<TestCase>()))
            .Callback<TestCase>(test => discoveredTests.Add(test));

        // Setup mock RunContext with RunSettings
        var mockRunContext = new Mock<IRunContext>();
        mockRunContext.SetupGet(rc => rc.RunSettings)
            .Returns(Mock.Of<IRunSettings>(rs => rs.SettingsXml == XmlSettings));

        // the first one should be excluded because it is 'TestAdapter' in the name
        // the second assembly do not contain any tests
        var assemblyPath = typeof(GdUnit4TestDiscovererTest).Assembly.Location;
        var discoverer = new GdUnit4TestDiscoverer();
        discoverer.DiscoverTests(new[] { "MSTest.TestAdapter.dll", assemblyPath }, mockRunContext.Object, frameworkHandle.Object, mockDiscoverySink.Object);

        // Verify SendTestCase was never called
        mockDiscoverySink.Verify(ds => ds.SendTestCase(It.IsAny<TestCase>()), Times.Never());
        Assert.AreEqual(0, discoveredTests.Count, "Should not discover any tests for empty assembly");

        // Verify log messages
        // @formatter:off
        CollectionAssert.AreEquivalent(
            new[]
            {
                $"Informational: Running on GdUnit4 test engine version: {ITestEngine.EngineVersion()}",
                $"Informational: Discover tests from assembly: {assemblyPath}",
                "Informational: Discover tests done, no tests found."
            },
            logMessages,
            "Log messages don't match expected messages"
        );
        // @formatter:on

        Assert.IsFalse(
            logMessages.Any(msg => msg.StartsWith("Error:")), "They should not contain any errors"
        );
    }

    [TestMethod]
    public void DiscoverOnTestAssembly()
    {
        var frameworkHandle = new Mock<IFrameworkHandle>();
        var logMessages = new List<string>();

        frameworkHandle
            .Setup(logger => logger.SendMessage(It.IsAny<TestMessageLevel>(), It.IsAny<string>()))
            .Callback<TestMessageLevel, string>((level, message) => logMessages.Add($"{level}: {message}"));

        // Setup the mock to capture discovered tests
        var mockDiscoverySink = new Mock<ITestCaseDiscoverySink>();
        var discoveredTests = new List<TestCase>();
        mockDiscoverySink
            .Setup(ds => ds.SendTestCase(It.IsAny<TestCase>()))
            .Callback<TestCase>(test => discoveredTests.Add(test));

        // Setup mock RunContext with RunSettings
        var mockRunContext = new Mock<IRunContext>();
        mockRunContext.SetupGet(rc => rc.RunSettings)
            .Returns(Mock.Of<IRunSettings>(rs => rs.SettingsXml == XmlSettings));

        //var assemblyPath = "D:\\development\\workspace\\gdUnit4Net\\test\\.godot\\mono\\temp\\bin\\Debug\\gdUnit4Test.dll"; //AssemblyPaths.LibraryPath;
        var assemblyPath = AssemblyPaths.LibraryPath;
        Console.WriteLine($"AssemblyPaths.LibraryPath: '{AssemblyPaths.LibraryPath}'");
        Assert.IsTrue(File.Exists(AssemblyPaths.LibraryPath), $"Can find the test assembly: '{AssemblyPaths.LibraryPath}'");
        var discoverer = new GdUnit4TestDiscoverer();
        discoverer.DiscoverTests(new[] { assemblyPath }, mockRunContext.Object, frameworkHandle.Object, mockDiscoverySink.Object);

        // Verify SendTestCase was never called
        mockDiscoverySink.Verify(ds => ds.SendTestCase(It.IsAny<TestCase>()), Times.Exactly(13));
        Assert.AreEqual(13, discoveredTests.Count, "Should discover any tests from the assembly");

        // Verify log messages
        // @formatter:off
        CollectionAssert.AreEquivalent(
            new[]
            {
                $"Informational: Running on GdUnit4 test engine version: {ITestEngine.EngineVersion()}",
                $"Informational: Discover tests from assembly: {assemblyPath}",
                "Informational: Discover:  TestSuite Examples.ExampleTest with 4 TestCases found.",
                "Informational: Discover:  TestSuite Example.Tests.API.Asserts.AssertionsTest with 9 TestCases found.",
                "Informational: Discover tests done, 2 TestSuites and total 13 Tests found."
            },
            logMessages,
            "Log messages don't match expected messages"
        );
        // @formatter:on
        Assert.IsFalse(
            logMessages.Any(msg => msg.StartsWith("Error:")), "They should not contain any errors"
        );

        // Verify discovered tests
        Assert.AreEqual(13, discoveredTests.Count, "Should discover any tests from assembly");
        // Verify all properties exemplary for one test
        AssertTestCase(discoveredTests,
            "Examples.ExampleTest.Success",
            "Success",
            assemblyPath,
            @"example\test\ExampleTest.cs",
            14);
    }

    private void AssertTestCase(IEnumerable<TestCase> tests, string fullyQualifiedName, string displayName, string source, string codeFilePath, int lineNumber)
    {
        var test = tests.FirstOrDefault(t => t.FullyQualifiedName == fullyQualifiedName);
        Assert.IsNotNull(test, $"Test {fullyQualifiedName} not found");
        Assert.IsNotNull(test.Id);
        Assert.AreEqual(fullyQualifiedName, test.FullyQualifiedName);
        Assert.AreEqual(displayName, test.DisplayName);
        Assert.AreEqual(new Uri(GdUnit4TestExecutor.ExecutorUri), test.ExecutorUri);
        Assert.AreEqual(source, test.Source);
        var expectedPath = codeFilePath.Replace('\\', Path.DirectorySeparatorChar);
        var actualPath = test.CodeFilePath?.Replace('\\', Path.DirectorySeparatorChar);
        StringAssert.EndsWith(actualPath, expectedPath);
        Assert.AreEqual(lineNumber, test.LineNumber);
    }
}
