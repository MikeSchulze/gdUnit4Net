// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4;

using System;
using System.Threading.Tasks;

using Core.Extensions;

using Godot;

using static Assertions;

public static class GdUnitAwaiter
{
    public static async Task AwaitSignal(this Node node, string signal, params Variant[] expectedArgs)
    {
        while (true)
        {
            var signalArgs = await Engine.GetMainLoop().ToSignal(node, signal);
            if (expectedArgs?.Length == 0 || signalArgs.VariantEquals(expectedArgs))
                return;
        }
    }

    public sealed class GodotMethodAwaiter<[MustBeVariant] TVariant>
        where TVariant : notnull
    {
        public GodotMethodAwaiter(Node instance, string methodName, params Variant[] args)
        {
            Instance = instance;
            MethodName = methodName;
            Args = args;
            if (!Instance.HasMethod(MethodName) && Instance.GetType().GetMethod(methodName) == null)
                throw new MissingMethodException($"The method '{MethodName}' not exist on loaded scene.");
        }

        private string MethodName { get; }

        private Node Instance { get; }

        private Variant[] Args { get; }

        public async Task IsEqual(TVariant expected) =>
            await CallAndWaitIsFinished(current => AssertThat(current).IsEqual(expected));

        public async Task IsNull() =>
            await CallAndWaitIsFinished(current => AssertThat(current).IsNull());

        public async Task IsNotNull() =>
            await CallAndWaitIsFinished(current => AssertThat(current).IsNotNull());

        private async Task CallAndWaitIsFinished(Predicate comparator) => await Task.Run(async () =>
        {
            // sync to main thread
            await GodotObjectExtensions.SyncProcessFrame;
            var value = await GodotObjectExtensions.Invoke(Instance, MethodName, Args);
            comparator(value);
        });

        private delegate void Predicate(object? current);
    }
}
