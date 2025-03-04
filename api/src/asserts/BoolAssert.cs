namespace GdUnit4.Asserts;

public sealed class BoolAssert : AssertBase<bool>, IBoolAssert
{
    public BoolAssert(bool current) : base(current)
    {
    }

    public IBoolAssert IsFalse()
    {
        if (true.Equals(Current))
            ThrowTestFailureReport(AssertFailures.IsFalse(), Current, false);
        return this;
    }

    public IBoolAssert IsTrue()
    {
        if (!true.Equals(Current))
            ThrowTestFailureReport(AssertFailures.IsTrue(), Current, true);
        return this;
    }

    public new IBoolAssert OverrideFailureMessage(string message)
        => (IBoolAssert)base.OverrideFailureMessage(message);
}
