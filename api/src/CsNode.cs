namespace GdUnit4;

using System.Collections.Generic;

using Godot;
using Godot.Collections;

/// <summary>
///     This class it used on Godot/GdUnit4 execution framework to wrap test cases and test suites
/// </summary>
// ReSharper disable all MemberCanBePrivate.Global
public partial class CsNode : Node
{
    private readonly string resourcePath;

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

    public int LineNumber
    {
        get;
        private set;
    } = -1;

    public List<string> ParameterizedTests { get; } = new();

    public bool IsCsTestSuite
    {
        get;
        private set;
    }

    public string ResourcePath() => resourcePath;
    public Array<string> TestCaseNames() => ParameterizedTests.ToGodotArray<string>();
    public override string ToString()
    {
        if (ParameterizedTests.Count != 0)
            return $"{Name}:{LineNumber} {ParameterizedTests.Formatted()}";
        return $"{Name}:{LineNumber}";
    }
#pragma warning disable CA1707, IDE1006, IDE0060 // Naming Styles, Remove unused parameter
    /// <summary>
    ///     This method is called from GDScript and must be match the function name.
    /// </summary>
    /// <param name="index"></param>
    public void set_test_parameter_index(int index)
        => GD.PushWarning("Running a single parameterized test not supported!");
#pragma warning restore CA1707, IDE1006, IDE0060 // Naming Styles
}
