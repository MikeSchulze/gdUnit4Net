// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

using Constraints;

internal sealed class BoolAssert : AssertBase<bool, IBoolConstraint>, IBoolAssert
{
    internal BoolAssert(bool current)
        : base(current)
    {
    }

    public IBoolConstraint IsFalse()
    {
        if (true.Equals(Current))
            ThrowTestFailureReport(AssertFailures.IsFalse(), Current, false);
        return this;
    }

    public IBoolConstraint IsTrue()
    {
        if (!true.Equals(Current))
            ThrowTestFailureReport(AssertFailures.IsTrue(), Current, true);
        return this;
    }
}
