namespace GdUnit4.Asserts
{
    /// <summary> An Assertion Tool to verify Godot.Vector2 values </summary>
    public interface IVector2Assert : IAssertBase<Godot.Vector2>
    {
        /// <summary>
        /// Verifies that the current value is equal to expected one.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <returns>IVector2Assert</returns>
        public new IVector2Assert IsEqual(Godot.Vector2 expected);

        /// <summary>
        /// Verifies that the current value is not equal to expected one.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <returns>IVector2Assert</returns>
        public new IVector2Assert IsNotEqual(Godot.Vector2 expected);

        /// <summary>
        /// Verifies that the current and expected value are approximately equal.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <param name="approx">The approximade</param>
        /// <returns>IVector2Assert</returns>
        public IVector2Assert IsEqualApprox(Godot.Vector2 expected, Godot.Vector2 approx);

        /// <summary>
        /// Verifies that the current value is less than the given one.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <returns>IVector2Assert</returns>
        public IVector2Assert IsLess(Godot.Vector2 expected);

        /// <summary>
        /// Verifies that the current value is less than or equal the given one.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <returns>IVector2Assert</returns>
        public IVector2Assert IsLessEqual(Godot.Vector2 expected);

        /// <summary>
        /// Verifies that the current value is greater than the given one.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <returns>IVector2Assert</returns>
        public IVector2Assert IsGreater(Godot.Vector2 expected);

        /// <summary>
        /// Verifies that the current value is greater than or equal the given one.
        /// </summary>
        /// <param name="expected">The expencted value</param>
        /// <returns>IVector2Assert</returns>
        public IVector2Assert IsGreaterEqual(Godot.Vector2 expected);

        /// <summary>
        /// Verifies that the current value is between the given boundaries (inclusive).
        /// </summary>
        /// <param name="from">The value starting from</param>
        /// <param name="to">The value ending to</param>
        /// <returns>IVector2Assert</returns>
        public IVector2Assert IsBetween(Godot.Vector2 from, Godot.Vector2 to);

        /// <summary>
        /// Verifies that the current value is not between the given boundaries (inclusive).
        /// </summary>
        /// <param name="from">The value starting from</param>
        /// <param name="to">The value ending to</param>
        /// <returns>IVector2Assert</returns>
        public IVector2Assert IsNotBetween(Godot.Vector2 from, Godot.Vector2 to);

        /// <summary>
        /// Overrides the default failure message by given custom message.
        /// </summary>
        /// <param name="message">The message to replace the default message</param>
        /// <returns>IVector2Assert</returns>
        new IVector2Assert OverrideFailureMessage(string message);
    }
}
