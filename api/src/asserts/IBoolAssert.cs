namespace GdUnit4.Asserts;

/// <summary>
///     An Assertion Tool to verify boolean values
/// </summary>
public interface IBoolAssert : IAssertBase<bool>
{
    /// <summary>
    ///     Verifies that the current value is true.
    /// </summary>
    /// <returns>IBoolAssert</returns>
    public IBoolAssert IsTrue();

    /// <summary>
    ///     Verifies that the current value is false.
    /// </summary>
    /// <returns>IBoolAssert</returns>
    public IBoolAssert IsFalse();

    /// <summary>
    ///     Overrides the default failure message by given custom message.
    /// </summary>
    /// <param name="message">A custom failure message</param>
    /// <returns>IBoolAssert</returns>
    public new IBoolAssert OverrideFailureMessage(string message);
}
