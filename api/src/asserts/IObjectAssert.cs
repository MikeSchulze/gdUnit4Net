namespace GdUnit4.Asserts;

/// <summary>
///     An Assertion Tool to verify object values
/// </summary>
public interface IObjectAssert : IAssertBase<object>
{
    /// <summary>
    ///     Verifies that the current value is the same as the given one.
    /// </summary>
    /// <param name="expected">The object to be the same</param>
    /// <returns>IObjectAssert</returns>
    public IObjectAssert IsSame(object expected);

    /// <summary>
    ///     Verifies that the current value is not the same as the given one.
    /// </summary>
    /// <param name="expected">The object to be NOT the same</param>
    /// <returns>IObjectAssert</returns>
    public IObjectAssert IsNotSame(object expected);

    /// <summary>
    ///     Verifies that the current value is an instance of the given type.
    /// </summary>
    /// <typeparam name="TExpectedType">The type of instance to be expected</typeparam>
    /// <returns>IObjectAssert</returns>
    public IObjectAssert IsInstanceOf<TExpectedType>();

    /// <summary>
    ///     Verifies that the current value is not an instance of the given type.
    /// </summary>
    /// <typeparam name="TExpectedType">The type of instance to be NOT expected</typeparam>
    /// <returns>IObjectAssert</returns>
    public IObjectAssert IsNotInstanceOf<TExpectedType>();

    /// <summary>
    ///     Overrides the default failure message by given custom message.
    /// </summary>
    /// <param name="message">A custom failure message</param>
    /// <returns>IObjectAssert</returns>
    public new IObjectAssert OverrideFailureMessage(string message);
}
