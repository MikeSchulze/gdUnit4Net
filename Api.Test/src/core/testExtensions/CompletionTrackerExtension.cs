using System;
using System.Threading.Tasks;
using GdUnit4.Api;

namespace GdUnit4.Tests.Core.TestExtensions;

internal class CompletionTrackerExtension(Action<string>? callback) : IBeforeAllCallback, IBeforeEachCallback, IAfterEachCallback,
    IAfterAllCallback
{
    public CompletionTrackerExtension() : this(null)
    {
    }

    public const string BeforeAllExecuted = "BeforeAllExecuted";

    public const string BeforeEachExecuted = "BeforeEachExecuted";

    public const string AfterEachExecuted = "AfterEachExecuted";

    public const string AfterAllExecuted = "AfterAllExecuted";

    public Task BeforeAll(IExtensionContext context)
    {
        context.Store(BeforeAllExecuted, true);
        callback?.Invoke(BeforeAllExecuted);
        return Task.CompletedTask;
    }

    public Task BeforeEach(IExtensionContext context)
    {
        context.Store(BeforeEachExecuted, true);
        callback?.Invoke(BeforeEachExecuted);
        return Task.CompletedTask;
    }

    public Task AfterEach(IExtensionContext context)
    {
        context.Store(AfterEachExecuted, true);
        callback?.Invoke(AfterEachExecuted);
        return Task.CompletedTask;
    }

    public Task AfterAll(IExtensionContext context)
    {
        context.Store(AfterAllExecuted, true);
        callback?.Invoke(AfterAllExecuted);
        return Task.CompletedTask;
    }
}
