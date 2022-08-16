namespace GdUnit3
{
    public class CsNode : Godot.Node
    {
		private readonly string _resourcePath;

        public string ResourcePath() => _resourcePath;

        public int LineNumber
        { get; private set; }


        public bool IsCsTestSuite
        { get; private set; }

        public CsNode(string name, string resourcePath, int lineNumber = -1)
        {
            Name = name;
            _resourcePath = resourcePath;
            LineNumber = lineNumber;
            IsCsTestSuite = true;
        }
    }
}
