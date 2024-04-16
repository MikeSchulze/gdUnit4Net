namespace GdUnit4.Asserts;

public sealed class ObjectAssert : AssertBase<object>, IObjectAssert
{
    public ObjectAssert(object? current) : base(current)
    {
        var type = current?.GetType();
        if (type != null && type.IsPrimitive)
            ThrowTestFailureReport($"ObjectAssert initial error: current is primitive <{type}>", Current, null);
    }

    public IObjectAssert IsNotInstanceOf<TExpectedType>()
    {
        if (Current is TExpectedType)
            ThrowTestFailureReport(AssertFailures.NotInstanceOf(typeof(TExpectedType)), Current, typeof(TExpectedType));
        return this;
    }

    public IObjectAssert IsNotSame(object expected)
    {
        if (Current == expected)
            ThrowTestFailureReport(AssertFailures.IsNotSame(expected), Current, expected);
        return this;
    }

    public IObjectAssert IsSame(object expected)
    {
        if (Current != expected)
            ThrowTestFailureReport(AssertFailures.IsSame(Current, expected), Current, expected);
        return this;
    }

    public IObjectAssert IsInstanceOf<TExpectedType>()
    {
        if (Current is not TExpectedType)
            ThrowTestFailureReport(AssertFailures.IsInstanceOf(Current?.GetType(), typeof(TExpectedType)), Current, typeof(TExpectedType));
        return this;
    }

    public new IObjectAssert OverrideFailureMessage(string message)
    {
        base.OverrideFailureMessage(message);
        return this;
    }
}
