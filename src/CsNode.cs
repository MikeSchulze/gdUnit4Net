using System.Collections.Generic;

namespace GdUnit3
{
    public class CsNode : Godot.Node
    {
        private readonly string _resourcePath;

        public string ResourcePath() => _resourcePath;

        // called from GdUnit3 GdScript to build test case nodes
        public Godot.Collections.Array<string> test_case_names() => TestCases.ToGodotArray<string>();

        public int LineNumber
        { get; private set; } = -1;

        public List<string> TestCases
        { get; private set; } = new List<string>();

        public bool IsCsTestSuite
        { get; private set; } = false;

        public CsNode(string name, string resourcePath)
        {
            Name = name;
            _resourcePath = resourcePath;
            IsCsTestSuite = true;
        }

        public CsNode(string name, string resourcePath, int lineNumber, List<string> testCases) : this(name, resourcePath)
        {
            LineNumber = lineNumber;
            TestCases = testCases;
        }

        public override string ToString()
        {
            if (TestCases.Count != 0)
                return $"{Name}:{LineNumber} {TestCases.Formated()}";
            return $"{Name}:{LineNumber}";
        }
    }
}
