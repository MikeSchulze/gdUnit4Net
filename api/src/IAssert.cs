namespace GdUnit4.Asserts;

/// <summary> Main interface of all GdUnit asserts </summary>
public interface IAssert
{
    /// <summary> Overrides the default failure message by given custom message.</summary>
    IAssert OverrideFailureMessage(string message);
}

/// <summary> Base interface of all GdUnit asserts </summary>
public interface IAssertBase<TValue> : IAssert
{
    /// <summary>Verifies that the current value is null.</summary>
    IAssertBase<TValue> IsNull();

    /// <summary> Verifies that the current value is not null.</summary>
    IAssertBase<TValue> IsNotNull();

    /// <summary> Verifies that the current value is equal to expected one.
    IAssertBase<TValue> IsEqual(TValue expected);

    /// <summary> Verifies that the current value is not equal to expected one.</summary>
    IAssertBase<TValue> IsNotEqual(TValue expected);
}
