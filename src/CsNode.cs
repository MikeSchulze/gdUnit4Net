using System.Collections.Generic;

namespace GdUnit4
{
    public partial class CsNode : Godot.Node
    {
        private readonly string _resourcePath;

        public string ResourcePath => _resourcePath;

        public int LineNumber
        { get; private set; } = -1;

        public Godot.Collections.Array<string> ParameterizedTests
        { get; private set; } = new Godot.Collections.Array<string>();

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
            ParameterizedTests = testCases.ToGodotArray<string>();
        }

        public override string ToString()
        {
            if (ParameterizedTests.Count != 0)
                return $"{Name}:{LineNumber} {ParameterizedTests.Formated()}";
            return $"{Name}:{LineNumber}";
        }
    }
}
