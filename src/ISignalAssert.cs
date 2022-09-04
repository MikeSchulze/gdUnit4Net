using System.Threading.Tasks;

namespace GdUnit3.Asserts
{
    /// <summary> An Assertion Tool to verify Godot signals</summary>
    public interface ISignalAssert : IAssertBase<Godot.Object>
    {
        /// <summary>
        /// Verifies that given signal is emitted until waiting time
        /// </summary>
        /// <param name="signal">The signal name</param>
        /// <param name="args">Optional signal arguments</param>
        /// Example: it waits a maximum of 2000ms before is failing if the signal is not emitted
        /// await AssertSignal(node).IsEmitted("draw").WithTimeout(2000);
        /// <returns></returns>
        public Task<ISignalAssert> IsEmitted(string signal, params object[] args);

        /// <summary>
        /// Verifies that given signal is NOT emitted until waiting time
        /// </summary>
        /// <param name="signal">The signal name</param>
        /// <param name="args">Optional signal arguments</param>
        /// Example: it waits until 2000ms and is failing if the signal is emitted in this time
        /// await AssertSignal(node).IsNotEmitted("draw").WithTimeout(2000);
        /// <returns></returns>
        public Task<ISignalAssert> IsNotEmitted(string signal, params object[] args);

        /// <summary>
        /// Verifies if the signal exists on the emitter.
        /// </summary>
        /// <param name="signal">The signal name</param>
        /// <returns></returns>
        public ISignalAssert IsSignalExists(string signal);

    }
}
