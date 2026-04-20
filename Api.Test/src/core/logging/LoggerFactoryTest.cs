// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Tests.Core.Logging;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Api;

using GdUnit4.Core.Logging;

using static Assertions;

[TestSuite]
public class LoggerFactoryTest
{
    [AfterTest]
    public void AfterTest() => LoggerFactory.EndScope();

    #region Root

    [TestCase]
    public void Root_IsNeverNull()
        => AssertThat(LoggerFactory.Root).IsNotNull();

    [TestCase]
    public void Root_IsNeverAScopeLogger()
    {
        LoggerFactory.BeginScope("ctx-1");

        // Root is the immutable engine logger — BeginScope must not replace it.
        AssertThat(LoggerFactory.Root).IsNotInstanceOf<ScopeLogger>();
    }

    #endregion

    #region Current

    [TestCase]
    public void Current_FallsBackToRoot_WhenNoScopeActive()
    {
        LoggerFactory.EndScope();

        AssertThat(LoggerFactory.Current).IsSame(LoggerFactory.Root);
    }

    [TestCase]
    public void BeginScope_SetsCurrent_ToScopeLogger()
    {
        LoggerFactory.BeginScope("ctx-1");

        AssertThat(LoggerFactory.Current).IsInstanceOf<ScopeLogger>();
        AssertThat(LoggerFactory.GetScope()!.ScopeId).IsEqual("ctx-1");
    }

    [TestCase]
    public void EndScope_RestoresCurrent_ToRoot()
    {
        LoggerFactory.BeginScope("ctx-1");
        AssertThat(LoggerFactory.Current).IsInstanceOf<ScopeLogger>();

        LoggerFactory.EndScope();

        AssertThat(LoggerFactory.Current).IsSame(LoggerFactory.Root);
    }

    #endregion

    #region AsyncLocal isolation across parallel tasks

    [TestCase]
    public void BeginScope_IsolatesScopePerTask()
    {
        ScopeLogger? seenByA = null;
        ScopeLogger? seenByB = null;

        using var barrier = new Barrier(2);

        var taskA = Task.Run(() =>
        {
            LoggerFactory.BeginScope("task-a");
            barrier.SignalAndWait(TimeSpan.FromSeconds(5));
            seenByA = LoggerFactory.GetScope();
        });

        var taskB = Task.Run(() =>
        {
            LoggerFactory.BeginScope("task-b");
            barrier.SignalAndWait(TimeSpan.FromSeconds(5));
            seenByB = LoggerFactory.GetScope();
        });

        Task.WaitAll(taskA, taskB);

        AssertThat(seenByA).IsNotNull();
        AssertThat(seenByA!.ScopeId).IsEqual("task-a");
        AssertThat(seenByB).IsNotNull();
        AssertThat(seenByB!.ScopeId).IsEqual("task-b");
        AssertThat(seenByA).IsNotSame(seenByB);
    }

    [TestCase]
    public void BeginScope_DoesNotLeakIntoSiblingTask()
    {
        ScopeLogger? seenByB = null;

        // taskB starts before taskA begins its scope
        var bStarted = new ManualResetEventSlim(false);
        var aBegan = new ManualResetEventSlim(false);

        var taskB = Task.Run(() =>
        {
            bStarted.Set();
            aBegan.Wait(TimeSpan.FromSeconds(5));
            seenByB = LoggerFactory.GetScope();
        });

        var taskA = Task.Run(() =>
        {
            bStarted.Wait(TimeSpan.FromSeconds(5));
            LoggerFactory.BeginScope("task-a");
            aBegan.Set();
        });

        Task.WaitAll(taskA, taskB);

        // taskB never called BeginScope, so it must not see taskA's scope
        AssertThat(seenByB?.ScopeId).IsNotEqual("task-a");
    }

    [TestCase]
    public void BeginScope_LogsAreTaggedAndRoutedToRoot()
    {
        // Both scope loggers wrap the same immutable root; verify each task's messages
        // arrive at root tagged with the correct [scopeId] and without cross-task bleed.
        var messages = new ConcurrentQueue<string>();
        LoggerFactory.Init(new RecordingLogger(messages));
        try
        {
            using var barrier = new Barrier(2);

            var taskA = Task.Run(() =>
            {
                LoggerFactory.BeginScope("task-a");
                barrier.SignalAndWait(TimeSpan.FromSeconds(5));
                LoggerFactory.GetLogger<TaskSourceA>().LogInfo("hello");
            });

            var taskB = Task.Run(() =>
            {
                LoggerFactory.BeginScope("task-b");
                barrier.SignalAndWait(TimeSpan.FromSeconds(5));
                LoggerFactory.GetLogger<TaskSourceB>().LogInfo("hello");
            });

            Task.WaitAll(taskA, taskB);
        }
        finally
        {
            LoggerFactory.Init(NoOpTestEngineLogger.Instance);
        }

        AssertThat(messages.OrderBy(m => m).ToList())
            .ContainsExactly("[task-a] hello", "[task-b] hello");
    }

    // Distinct marker types so LogCapture scopes don't overlap between tasks
    private sealed class TaskSourceA;

    private sealed class TaskSourceB;

    // Records all forwarded messages for assertion
    private sealed class RecordingLogger(ConcurrentQueue<string> output) : ITestEngineLogger
    {
        void ITestEngineLogger.SendMessage(LogLevel logLevel, string message) => output.Enqueue(message);
    }

    #endregion
}
