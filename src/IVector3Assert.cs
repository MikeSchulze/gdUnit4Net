namespace GdUnit4.Asserts
{
    /// <summary> An Assertion Tool to verify Godot.Vector3 values </summary>
    public interface IVector3Assert : IAssertBase<Godot.Vector3>
    {
        /// <summary>
        /// Verifies that the current value is equal to expected one.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <returns>IVector3Assert</returns>
        public new IVector3Assert IsEqual(Godot.Vector3 expected);

        /// <summary>
        /// Verifies that the current value is not equal to expected one.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <returns>IVector3Assert</returns>
        public new IVector3Assert IsNotEqual(Godot.Vector3 expected);

        /// <summary>
        /// Verifies that the current and expected value are approximately equal.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <param name="approx">The approximade</param>
        /// <returns>IVector3Assert</returns>
        public IVector3Assert IsEqualApprox(Godot.Vector3 expected, Godot.Vector3 approx);

        /// <summary>
        /// Verifies that the current value is less than the given one.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <returns>IVector3Assert</returns>
        public IVector3Assert IsLess(Godot.Vector3 expected);

        /// <summary>
        /// Verifies that the current value is less than or equal the given one.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <returns>IVector3Assert</returns>
        public IVector3Assert IsLessEqual(Godot.Vector3 expected);

        /// <summary>
        /// Verifies that the current value is greater than the given one.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <returns>IVector3Assert</returns>
        public IVector3Assert IsGreater(Godot.Vector3 expected);

        /// <summary>
        /// Verifies that the current value is greater than or equal the given one.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <returns>IVector3Assert</returns>
        public IVector3Assert IsGreaterEqual(Godot.Vector3 expected);

        /// <summary>
        /// Verifies that the current value is between the given boundaries (inclusive).
        /// </summary>
        /// <param name="from">The value starting from</param>
        /// <param name="to">The value ending to</param>
        /// <returns>IVector3Assert</returns>
        public IVector3Assert IsBetween(Godot.Vector3 from, Godot.Vector3 to);

        /// <summary>
        /// Verifies that the current value is not between the given boundaries (inclusive).
        /// </summary>
        /// <param name="from">The value starting from</param>
        /// <param name="to">The value ending to</param>
        /// <returns>IVector3Assert</returns>
        public IVector3Assert IsNotBetween(Godot.Vector3 from, Godot.Vector3 to);

        /// <summary>
        /// Overrides the default failure message by given custom message.
        /// </summary>
        /// <param name="message">The message to replace the default message</param>
        /// <returns>IVector3Assert</returns>
        new IVector3Assert OverrideFailureMessage(string message);
    }
}
