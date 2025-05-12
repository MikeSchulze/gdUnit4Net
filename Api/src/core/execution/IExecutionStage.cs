// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Execution;

using System.Threading.Tasks;

internal interface IExecutionStage
{
    Task Execute(ExecutionContext context);
}
