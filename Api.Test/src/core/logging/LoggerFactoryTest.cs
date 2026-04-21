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
    #region Dispose

    [TestCase]
    public void Dispose_ResetsRootToNoOp()
    {
        LoggerFactory.Init(new RecordingLogger(new ConcurrentQueue<string>()));
        AssertThat(LoggerFactory.Root).IsNotInstanceOf<NoOpTestEngineLogger>();

        LoggerFactory.Dispose();

        AssertThat(LoggerFactory.Root).IsInstanceOf<NoOpTestEngineLogger>();
    }

    #endregion

    #region Root

    [TestCase]
    public void Root_IsNeverNull()
        => AssertThat(LoggerFactory.Root).IsNotNull();

    [TestCase]
    public void Root_IsNeverAScopeLogger()
    {
        using (LoggerFactory.CreateScope("ctx-1"))
        {
            // Root is the immutable engine logger — CreateScope must not replace it.
            AssertThat(LoggerFactory.Root).IsNotInstanceOf<ScopeLogger>();
        }
    }

    #endregion

    #region Current

    [TestCase]
    public void CreateScope_SetsCurrent_ToScopeLogger()
    {
        using (LoggerFactory.CreateScope("ctx-1"))
        {
            AssertThat(LoggerFactory.Current).IsInstanceOf<ScopeLogger>();
            AssertThat(LoggerFactory.GetScope()!.ScopeId).IsEqual("ctx-1");
        }
    }

    [TestCase]
    public void EndScope_RestoresCurrent_ToRoot()
    {
        using (LoggerFactory.CreateScope("ctx-1"))
            AssertThat(LoggerFactory.Current).IsInstanceOf<ScopeLogger>();

        AssertThat(LoggerFactory.Current).IsSame(LoggerFactory.Root);
    }

    #endregion

    #region AsyncLocal isolation across parallel tasks

    [TestCase]
    public void CreateScope_IsolatesScopePerTask()
    {
        ScopeLogger? seenByA = null;
        ScopeLogger? seenByB = null;

        using var barrier = new Barrier(2);

        var taskA = Task.Run(() =>
        {
            using var _ = LoggerFactory.CreateScope("task-a");
            barrier.SignalAndWait(TimeSpan.FromSeconds(5));
            seenByA = LoggerFactory.GetScope();
        });

        var taskB = Task.Run(() =>
        {
            using var _ = LoggerFactory.CreateScope("task-b");
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
    public void CreateScope_DoesNotLeakIntoSiblingTask()
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
            using var _ = LoggerFactory.CreateScope("task-a");
            aBegan.Set();
        });

        Task.WaitAll(taskA, taskB);

        // taskB never called CreateScope, so it must not see taskA's scope
        AssertThat(seenByB?.ScopeId).IsNotEqual("task-a");
    }

    [TestCase]
    public void CreateScope_LogsAreTaggedAndRoutedToRoot()
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
                using var _ = LoggerFactory.CreateScope("task-a");
                barrier.SignalAndWait(TimeSpan.FromSeconds(5));
                LoggerFactory.GetLogger<TaskSourceA>().LogInfo("hello");
            });

            var taskB = Task.Run(() =>
            {
                using var _ = LoggerFactory.CreateScope("task-b");
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
