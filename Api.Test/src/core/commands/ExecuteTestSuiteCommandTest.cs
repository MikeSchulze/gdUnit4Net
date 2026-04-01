// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.Commands;

using System;
using System.Reflection;
using System.Threading.Tasks;

using Api;

using GdUnit4.Core.Commands;
using GdUnit4.Core.Execution;
using GdUnit4.Core.TestExtensions;

[TestSuite]
public class ExecuteTestSuiteCommandTest
{
    [TestCase]
    public async Task ValidateExtensionExecutionOrder()
    {
        var testSuiteNode = new TestSuiteNode
        {
            AssemblyPath = Assembly.GetAssembly(typeof(ExampleSuite))?.Location ?? "",
            Id = Guid.NewGuid(),
            ManagedType = typeof(ExampleSuite).FullName ?? "",
            ParentId = Guid.Empty,
            SourceFile = "",
            Tests =
            [
                new TestCaseNode
                {
                    AttributeIndex = 0,
                    Id = Guid.NewGuid(),
                    LineNumber = -1,
                    ManagedMethod = nameof(ExampleSuite.TestCase),
                    ParentId = Guid.Empty,
                    RequireRunningGodotEngine = false
                }
            ]
        };

        var command = new ExecuteTestSuiteCommand(
            testSuiteNode,
            false,
            false
        );

        using var capture = LogCapture.Watch<ExampleTestExtension, ExampleSuite, ExtensionRegistry>();

        // Run the test execution
        await command.Execute(new NoInteractTestEventListener());

        // Verify
        AssertThat(capture.Entries)
            .ContainsExactly(
                new LogEntry(LogLevel.Debug,
                    "Scanning test suite 'GdUnit4.Tests.Core.Commands.ExecuteTestSuiteCommandTest+ExampleSuite' for registered test extensions.",
                    typeof(ExtensionRegistry)),
                new LogEntry(LogLevel.Informational, "Extension.BeforeAll", typeof(ExampleTestExtension)),
                new LogEntry(LogLevel.Informational, "Suite.Before", typeof(ExampleSuite)),
                new LogEntry(LogLevel.Informational, "Extension.BeforeEach", typeof(ExampleTestExtension)),
                new LogEntry(LogLevel.Informational, "Suite.BeforeTest", typeof(ExampleSuite)),
                new LogEntry(LogLevel.Informational, "Suite.Test", typeof(ExampleSuite)),
                new LogEntry(LogLevel.Informational, "Suite.AfterTest", typeof(ExampleSuite)),
                new LogEntry(LogLevel.Informational, "Extension.AfterEach", typeof(ExampleTestExtension)),
                new LogEntry(LogLevel.Informational, "Suite.After", typeof(ExampleSuite)),
                new LogEntry(LogLevel.Informational, "Extension.AfterAll", typeof(ExampleTestExtension))
            );
    }

    [TestSuite]
    [ExtendWith<ExampleTestExtension>]
    private class ExampleSuite
    {
        private static ITestEngineLogger Logger => LoggerFactory.GetLogger<ExampleSuite>();

        [Before]
        // ReSharper disable once UnusedMember.Local
        public void Before() => Logger.LogInfo("Suite.Before");


        [BeforeTest]
        // ReSharper disable once UnusedMember.Local
        public void BeforeTest() => Logger.LogInfo("Suite.BeforeTest");


        [TestCase]
        // ReSharper disable once UnusedMember.Local
        public void TestCase() => Logger.LogInfo("Suite.Test");


        [AfterTest]
        // ReSharper disable once UnusedMember.Local
        public void AfterTest() => Logger.LogInfo("Suite.AfterTest");


        [After]
        // ReSharper disable once UnusedMember.Local
        public void After() => Logger.LogInfo("Suite.After");
    }
}
