using System;

namespace GdUnit3
{
    public class TestStageAttribute : Attribute
    {

        protected TestStageAttribute(string name, int line)
        {
            Name = name;
            Line = line;
        }

        /// <summary>
        /// Describes the intention of the test, will be shown as a tool tip on the inspector node.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Sets the timeout in ms to interrupt the test if the test execution takes longer as the given value.
        /// </summary>
        public long Timeout { get; set; } = -1;

        /// <summary>
        /// The test name
        /// </summary>
        internal string Name { get; private set; } = "";

        /// <summary>
        /// The line of the annotated method
        /// </summary>
        internal int Line { get; private set; } = -1;
    }
}
