// ReSharper disable once CheckNamespace

namespace GdUnit4;

using System;

/// <summary>
///     Indicates that a test method requires the Godot runtime environment.
///     This attribute is used to mark test methods that use Godot functionality and therefore need a running Godot engine.
///     It extends the basic TestCaseAttribute with Godot-specific test execution capabilities.
/// </summary>
/// <remarks>
///     Use this attribute when your test method:
///     <list type="bullet">
///         <item>Uses Godot types (like Node, Scene)</item>
///         <item>Interacts with Godot's core systems</item>
///         <item>Creates or manipulates Godot objects</item>
///     </list>
/// </remarks>
/// <example>
///     <code>
/// [GodotTestCase]
/// public void TestWithGodotTypes()
/// {
///     var node = new Node2D();
///     AssertThat(node).IsNotNull();
/// }
///
/// [GodotTestCase(1, 2, "test")]  // With parameters
/// public void TestWithParameters(int a, int b, string name)
/// {
///     var node = new Node2D();
///     node.Name = name;
///     AssertThat(node.Position.X + a + b).IsEqual(3);
/// }
/// </code>
/// </example>
/// <param name="args">Optional test arguments that will be passed to the test method</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class GodotTestCaseAttribute : TestCaseAttribute
{
    public GodotTestCaseAttribute(params object?[] args) : base("", -1)
        => Arguments = args;
}
