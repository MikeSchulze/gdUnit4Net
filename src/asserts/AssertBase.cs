namespace GdUnit4.Asserts
{
    using Exceptions;

    internal abstract class AssertBase<V> : IAssertBase<V>
    {
        protected V? Current { get; private set; }

        protected string? CustomFailureMessage { get; set; } = null;

        protected string CurrentFailureMessage { get; set; } = "";

        protected AssertBase(V? current)
        {
            Current = current;
        }

        public IAssertBase<V> IsEqual(V expected)
        {
            var result = Comparable.IsEqual(Current, expected);
            if (!result.Valid)
                ThrowTestFailureReport(AssertFailures.IsEqual(Current, expected), Current, expected);
            return this;
        }

        public IAssertBase<V> IsNotEqual(V expected)
        {
            var result = Comparable.IsEqual(Current, expected);
            if (result.Valid)
                ThrowTestFailureReport(AssertFailures.IsNotEqual(Current, expected), Current, expected);
            return this;
        }
        public IAssertBase<V> IsNull()
        {
            if (Current != null)
                ThrowTestFailureReport(AssertFailures.IsNull(Current), Current, null);
            return this;
        }

        public IAssertBase<V> IsNotNull()
        {
            if (Current == null)
                ThrowTestFailureReport(AssertFailures.IsNotNull(Current), Current, null);
            return this;
        }

        public IAssert OverrideFailureMessage(string message)
        {
            CustomFailureMessage = message;
            return this;
        }

        protected void ThrowTestFailureReport(string message, object? current, object? expected, int stackFrameOffset = 0, int lineNumber = -1)
        {
            var failureMessage = (CustomFailureMessage ?? message).UnixFormat();
            CurrentFailureMessage = failureMessage;
            throw new TestFailedException(failureMessage, stackFrameOffset, lineNumber);
        }
    }
}
