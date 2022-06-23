using Godot;

namespace GdUnit3.Asserts
{
    internal sealed class Vector3Assert : AssertBase<Godot.Vector3>, IVector3Assert
    {

        public Vector3Assert(Godot.Vector3 current) : base(current)
        { }

        public IVector3Assert IsBetween(Vector3 from, Vector3 to)
        {
            if (Current < from || Current > to)
                ThrowTestFailureReport(AssertFailures.IsBetween(Current, from, to), Current, from);
            return this;
        }

        public new IVector3Assert IsEqual(Vector3 expected) => (IVector3Assert)base.IsEqual(expected);

        public IVector3Assert IsEqualApprox(Vector3 expected, Vector3 approx) => IsBetween(expected - approx, expected + approx);

        public IVector3Assert IsGreater(Vector3 expected)
        {
            if (Current <= expected)
                ThrowTestFailureReport(AssertFailures.IsGreater(Current, expected), Current, expected);
            return this;
        }

        public IVector3Assert IsGreaterEqual(Vector3 expected)
        {
            if (Current < expected)
                ThrowTestFailureReport(AssertFailures.IsGreaterEqual(Current, expected), Current, expected);
            return this;
        }

        public IVector3Assert IsLess(Vector3 expected)
        {
            if (Current >= expected)
                ThrowTestFailureReport(AssertFailures.IsLess(Current, expected), Current, expected);
            return this;
        }

        public IVector3Assert IsLessEqual(Vector3 expected)
        {
            if (Current > expected)
                ThrowTestFailureReport(AssertFailures.IsLessEqual(Current, expected), Current, expected);
            return this;
        }

        public IVector3Assert IsNotBetween(Vector3 from, Vector3 to)
        {
            if (Current >= from && Current <= to)
                ThrowTestFailureReport(AssertFailures.IsNotBetween(Current, from, to), Current, from);
            return this;
        }

        public new IVector3Assert IsNotEqual(Vector3 expected) => (IVector3Assert)base.IsNotEqual(expected);

        public new IVector3Assert OverrideFailureMessage(string message) => (IVector3Assert)base.OverrideFailureMessage(message);

    }
}
