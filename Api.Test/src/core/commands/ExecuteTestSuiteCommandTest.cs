// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using GdUnit4.Api;
using GdUnit4.Core.Commands;
using GdUnit4.Core.Execution;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.Commands;

[TestSuite]
public class ExecuteTestSuiteCommandTest
{
    private static readonly List<string> ExecutionLog = [];

    private class LoggingExtension : IBeforeAllCallback, IBeforeEachCallback, IAfterEachCallback, IAfterAllCallback
    {
        public Task BeforeAll(IExtensionContext context)
        {
            ExecutionLog.Add("Extension.BeforeAll");
            return Task.CompletedTask;
        }


        public Task BeforeEach(IExtensionContext context)
        {
            ExecutionLog.Add("Extension.BeforeEach");
            return Task.CompletedTask;
        }


        public Task AfterEach(IExtensionContext context)
        {
            ExecutionLog.Add("Extension.AfterEach");
            return Task.CompletedTask;
        }


        public Task AfterAll(IExtensionContext context)
        {
            ExecutionLog.Add("Extension.AfterAll");
            return Task.CompletedTask;
        }
    }

    [TestSuite]
    private class ExampleSuite
    {
        [RegisterExtension] public static readonly ITestExtension LogExtension = new LoggingExtension();


        [Before]
        public void Before()
        {
            ExecutionLog.Add("Suite.Before");
        }


        [BeforeTest]
        public void BeforeTest()
        {
            ExecutionLog.Add("Suite.BeforeTest");
        }


        [TestCase]
        public void TestCase()
        {
            ExecutionLog.Add("Suite.Test");
        }


        [AfterTest]
        public void AfterTest()
        {
            ExecutionLog.Add("Suite.AfterTest");
        }


        [After]
        public void After()
        {
            ExecutionLog.Add("Suite.After");
        }
    }


    [BeforeTest]
    public void ClearLog()
    {
        ExecutionLog.Clear();
    }


    [TestCase]
    public async Task ValidateExecutionOrder()
    {
        var testCaseNode = new TestCaseNode
        {
            AttributeIndex = 0,
            Id = Guid.NewGuid(),
            LineNumber = -1,
            ManagedMethod = nameof(ExampleSuite.TestCase),
            ParentId = Guid.Empty,
            RequireRunningGodotEngine = false
        };

        var testSuiteNode = new TestSuiteNode
        {
            AssemblyPath = Assembly.GetAssembly(typeof(ExampleSuite))?.Location ?? "",
            Id = Guid.NewGuid(),
            ManagedType = typeof(ExampleSuite).FullName ?? "",
            ParentId = Guid.Empty,
            SourceFile = "",
            Tests =
            [
                testCaseNode
            ]
        };

        var command = new ExecuteTestSuiteCommand(
            testSuiteNode,
            false,
            false
        );

        await command.Execute(new NoInteractTestEventListener());

        AssertThat(ExecutionLog)
            .ContainsExactly(
                "Extension.BeforeAll",
                "Suite.Before",
                "Extension.BeforeEach",
                "Suite.BeforeTest",
                "Suite.Test",
                "Suite.AfterTest",
                "Extension.AfterEach",
                "Suite.After",
                "Extension.AfterAll"
            );
    }
}