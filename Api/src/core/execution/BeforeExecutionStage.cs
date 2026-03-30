// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Execution;

using System.Threading.Tasks;

using Api;

internal class BeforeExecutionStage : ExecutionStage<BeforeAttribute>
{
    public BeforeExecutionStage(TestSuite testSuite)
        : base("Before", testSuite.Instance.GetType())
    {
    }

    public override async Task Execute(ExecutionContext context, IExtensionContext extensionContext)
    {
        context.MemoryPool.SetActive(StageName, true);
        await context.ExtensionRegistry.RunBeforeAll(extensionContext)
            .ConfigureAwait(true);
        await base
            .Execute(context, extensionContext)
            .ConfigureAwait(true);
        context.FireBeforeEvent();
        context.ReportCollector.Clear();
        context.MemoryPool.StopMonitoring();
    }
}
