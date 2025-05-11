// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

using System.Threading.Tasks;

using Godot;

/// <summary>
///     An Assertion Tool to verify Godot signals.
/// </summary>
public interface ISignalAssert : IAssertBase<GodotObject>
{
    /// <summary>
    ///     Starts the monitoring of emitted signals during the test runtime.
    ///     It should be called first if you want to collect all emitted signals after the emitter has been created.
    /// </summary>
    /// <returns>ISignalAssert.</returns>
    public ISignalAssert StartMonitoring();

    /// <summary>
    ///     Verifies that given signal is emitted until waiting time.
    /// </summary>
    /// <param name="signal">The signal name.</param>
    /// <param name="args">Optional signal arguments.</param>
    /// Example: it waits a maximum of 2000ms before is failing if the signal is not emitted
    /// await AssertSignal(node).IsEmitted("draw").WithTimeout(2000);
    /// <returns>Task.</returns>
    public Task<ISignalAssert> IsEmitted(string signal, params Variant[] args);

    /// <summary>
    ///     Verifies that given signal is NOT emitted until waiting time.
    /// </summary>
    /// <param name="signal">The signal name.</param>
    /// <param name="args">Optional signal arguments.</param>
    /// Example: it waits until 2000ms and is failing if the signal is emitted in this time
    /// await AssertSignal(node).IsNotEmitted("draw").WithTimeout(2000);
    /// <returns>Task.</returns>
    public Task<ISignalAssert> IsNotEmitted(string signal, params Variant[] args);

    /// <summary>
    ///     Verifies if the signal exists on the emitter.
    /// </summary>
    /// <param name="signal">The signal name.</param>
    /// <returns>ISignalAssert.</returns>
    public ISignalAssert IsSignalExists(string signal);
}
