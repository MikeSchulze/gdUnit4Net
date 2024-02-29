namespace GdUnit4;

using System.Collections.Generic;

public partial class CsNode : Godot.Node
{
    private readonly string resourcePath;

    public string ResourcePath() => resourcePath;

    // wraper method to GdScript
    public Godot.Collections.Array<string> TestCaseNames() => ParameterizedTests.ToGodotArray<string>();

    public int LineNumber
    { get; private set; } = -1;

    public List<string> ParameterizedTests
    { get; private set; } = new List<string>();

    public bool IsCsTestSuite
    { get; private set; }

    public CsNode(string name, string resourcePath)
    {
        Name = name;
        this.resourcePath = resourcePath;
        IsCsTestSuite = true;
    }

    public CsNode(string name, string resourcePath, int lineNumber, List<string> testCases) : this(name, resourcePath)
    {
        LineNumber = lineNumber;
        ParameterizedTests = testCases;
    }

    public override string ToString()
    {
        if (ParameterizedTests.Count != 0)
            return $"{Name}:{LineNumber} {ParameterizedTests.Formatted()}";
        return $"{Name}:{LineNumber}";
    }
}
