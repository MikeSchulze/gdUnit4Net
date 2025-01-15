// ReSharper disable once CheckNamespace

namespace GdUnit4;

using System;

/// <summary>
///     Indicates that a test class requires the Godot runtime environment.
///     This attribute is used to mark test classes that contain test hooks ([Before], [After], [BeforeTest], [AfterTest])
///     which use Godot functionality and therefore need a running Godot engine.
/// </summary>
/// <remarks>
///     Use this attribute when your test class:
///     <list type="bullet">
///         <item>Uses Godot types or functionality in test hooks</item>
///         <item>Requires the Godot engine to be initialized</item>
///         <item>Interacts with Godot's core systems in setup or teardown methods</item>
///     </list>
/// </remarks>
/// <example>
///     <code>
/// [RequireGodotRuntime]
/// public class SceneRunnerTest
/// {
///     [Before]
///     public void Setup() =>
///         Engine.PhysicsTicksPerSecond = 60;
///
///     [TestCase]
///     public void TestMethod() =>
///         AssertThat(true).IsTrue();
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class RequireGodotRuntimeAttribute : Attribute
{
}
