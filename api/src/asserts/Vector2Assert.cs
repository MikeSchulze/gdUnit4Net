using Godot;

namespace GdUnit4.Asserts
{
    internal sealed class Vector2Assert : AssertBase<Godot.Vector2>, IVector2Assert
    {

        public Vector2Assert(Godot.Vector2 current) : base(current)
        { }

        public IVector2Assert IsBetween(Vector2 from, Vector2 to)
        {
            if (Current < from || Current > to)
                ThrowTestFailureReport(AssertFailures.IsBetween(Current, from, to), Current, from);
            return this;
        }

        public new IVector2Assert IsEqual(Vector2 expected) => (IVector2Assert)base.IsEqual(expected);

        public IVector2Assert IsEqualApprox(Vector2 expected, Vector2 approx) => IsBetween(expected - approx, expected + approx);

        public IVector2Assert IsGreater(Vector2 expected)
        {
            if (Current <= expected)
                ThrowTestFailureReport(AssertFailures.IsGreater(Current, expected), Current, expected);
            return this;
        }

        public IVector2Assert IsGreaterEqual(Vector2 expected)
        {
            if (Current < expected)
                ThrowTestFailureReport(AssertFailures.IsGreaterEqual(Current, expected), Current, expected);
            return this;
        }

        public IVector2Assert IsLess(Vector2 expected)
        {
            if (Current >= expected)
                ThrowTestFailureReport(AssertFailures.IsLess(Current, expected), Current, expected);
            return this;
        }

        public IVector2Assert IsLessEqual(Vector2 expected)
        {
            if (Current > expected)
                ThrowTestFailureReport(AssertFailures.IsLessEqual(Current, expected), Current, expected);
            return this;
        }

        public IVector2Assert IsNotBetween(Vector2 from, Vector2 to)
        {
            if (Current >= from && Current <= to)
                ThrowTestFailureReport(AssertFailures.IsNotBetween(Current, from, to), Current, from);
            return this;
        }

        public new IVector2Assert IsNotEqual(Vector2 expected) => (IVector2Assert)base.IsNotEqual(expected);

        public new IVector2Assert OverrideFailureMessage(string message) => (IVector2Assert)base.OverrideFailureMessage(message);

    }
}
