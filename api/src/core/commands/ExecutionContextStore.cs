namespace GdUnit4.Core.Commands;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Execution;

internal static class ExecutionContextStore
{
    private static readonly ConcurrentDictionary<Guid, ExecutionContext> Contexts = new();

    public static void SetContext(Guid testId, ExecutionContext context) =>
        Contexts.AddOrUpdate(testId, context, (_, _) => context);

    public static ExecutionContext? GetContext(Guid testId) => Contexts.GetValueOrDefault(testId);

    public static void RemoveContext(Guid testId) => Contexts.TryRemove(testId, out _);
}
