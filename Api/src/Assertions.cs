// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

using Asserts;

using Core.Execution.Exceptions;
using Core.Execution.Monitoring;
using Core.Extensions;

using Extractors;

using Godot;
using Godot.Collections;

using Array = Godot.Collections.Array;
using ITuple = Asserts.ITuple;
using Tuple = Asserts.Tuple;

/// <summary>
///     A collection of assertions and helpers to verify values.
/// </summary>
public static class Assertions
{
    /// <summary>
    ///     An Assertion to verify boolean values.
    /// </summary>
    /// <param name="current">The current boolean value to verify.</param>
    /// <returns>An instance of IBoolAssert for further assertions.</returns>
    public static IBoolAssert AssertBool(bool current) => new BoolAssert(current);

    /// <summary>
    ///     An Assertion to verify string values.
    /// </summary>
    /// <param name="current">The current string value to verify.</param>
    /// <returns>An instance of IStringAssert for further assertions.</returns>
    public static IStringAssert AssertString(string? current) => new StringAssert(current);

    /// <summary>
    ///     An Assertion to verify integer values.
    /// </summary>
    /// <param name="current">The current integer value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<int> AssertInt(int current) => new NumberAssert<int>(current);

    /// <summary>
    ///     An Assertion to verify double values.
    /// </summary>
    /// <param name="current">The current double value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<double> AssertFloat(double current) => new NumberAssert<double>(current);

    /// <summary>
    ///     An Assertion to verify object values.
    /// </summary>
    /// <param name="current">The current double value to verify.</param>
    /// <returns>An instance of IObjectAssert for further assertions.</returns>
    public static IObjectAssert AssertObject(object? current) => new ObjectAssert(current);

    /// <summary>
    ///     An Assertion to verify array values.
    /// </summary>
    /// <typeparam name="TValue">The type of elements in the array.</typeparam>
    /// <param name="current">The current array value to verify.</param>
    /// <returns>An instance of IEnumerableAssert for further assertions.</returns>
    public static IEnumerableAssert<TValue?> AssertArray<TValue>(IEnumerable<TValue?>? current)
        => new EnumerableAssert<TValue?>(current);

    /// <summary>
    ///     An Assertion to verify array values.
    /// </summary>
    /// <param name="current">The current array value to verify.</param>
    /// <returns>An instance of IEnumerableAssert for further assertions.</returns>
    public static IEnumerableAssert<object?> AssertArray(IEnumerable<object?>? current)
        => new EnumerableAssert<object?>(current);

    /// <summary>
    ///     An assertion method for all Godot vector types.
    /// </summary>
    /// <typeparam name="TValue">The type of Godot vector.</typeparam>
    /// <param name="vector">The vector value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    public static IVectorAssert<TValue> AssertVector<TValue>(TValue vector)
        where TValue : IEquatable<TValue>
        => new VectorAssert<TValue>(vector);

    /// <summary>
    ///     An Assertion to verify Godot.Vector2 values.
    /// </summary>
    /// <param name="current">The current vector2 value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    [Obsolete("AssertVec2 is deprecated, please use AssertVector instead.")]
    public static IVectorAssert<Vector2> AssertVec2(Vector2 current) => AssertVector(current);

    /// <summary>
    ///     An Assertion to verify Godot.Vector3 values.
    /// </summary>
    /// <param name="current">The current vector3 value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    [Obsolete("AssertVec3 is deprecated, please use AssertVector instead.")]
    public static IVectorAssert<Vector3> AssertVec3(Vector3 current) => AssertVector(current);

    /// <summary>
    ///     An Assertion used by test generation to notify the test is not yet implemented.
    /// </summary>
    public static void AssertNotYetImplemented() => throw new TestFailedException("Test not yet implemented!");

    /// <summary>
    ///     An Assertion to verify Godot signals.
    /// </summary>
    /// <param name="node">The object where is emitting the signal.</param>
    /// <returns>An instance of ISignalAssert for further assertions.</returns>
    public static ISignalAssert AssertSignal(GodotObject node) => new SignalAssert(node);

    /// <summary>
    ///     An Assertion to verify string values.
    /// </summary>
    /// <param name="current">The current string value to verify.</param>
    /// <returns>An instance of IStringAssert for further assertions.</returns>
    public static IStringAssert AssertThat(string? current) => new StringAssert(current);

    /// <summary>
    ///     An Assertion to verify boolean values.
    /// </summary>
    /// <param name="current">The current boolean value to verify.</param>
    /// <returns>An instance of IBoolAssert for further assertions.</returns>
    public static IBoolAssert AssertThat(bool current) => new BoolAssert(current);

    /// <summary>
    ///     An Assertion to verify sbyte values.
    /// </summary>
    /// <param name="current">The current sbyte value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<sbyte> AssertThat(sbyte current) => new NumberAssert<sbyte>(current);

    /// <summary>
    ///     An Assertion to verify byte values.
    /// </summary>
    /// <param name="current">The current byte value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<byte> AssertThat(byte current) => new NumberAssert<byte>(current);

    /// <summary>
    ///     An Assertion to verify short values.
    /// </summary>
    /// <param name="current">The current short value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<short> AssertThat(short current) => new NumberAssert<short>(current);

    /// <summary>
    ///     An Assertion to verify ushort values.
    /// </summary>
    /// <param name="current">The current ushort value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<ushort> AssertThat(ushort current) => new NumberAssert<ushort>(current);

    /// <summary>
    ///     An Assertion to verify int values.
    /// </summary>
    /// <param name="current">The current int value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<int> AssertThat(int current) => new NumberAssert<int>(current);

    /// <summary>
    ///     An Assertion to verify uint values.
    /// </summary>
    /// <param name="current">The current uint value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<uint> AssertThat(uint current) => new NumberAssert<uint>(current);

    /// <summary>
    ///     An Assertion to verify long values.
    /// </summary>
    /// <param name="current">The current long value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<long> AssertThat(long current) => new NumberAssert<long>(current);

    /// <summary>
    ///     An Assertion to verify ulong values.
    /// </summary>
    /// <param name="current">The current ulong value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<ulong> AssertThat(ulong current) => new NumberAssert<ulong>(current);

    /// <summary>
    ///     An Assertion to verify float values.
    /// </summary>
    /// <param name="current">The current float value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<float> AssertThat(float current) => new NumberAssert<float>(current);

    /// <summary>
    ///     An Assertion to verify double values.
    /// </summary>
    /// <param name="current">The current double value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<double> AssertThat(double current) => new NumberAssert<double>(current);

    /// <summary>
    ///     An Assertion to verify decimal values.
    /// </summary>
    /// <param name="current">The current decimal value to verify.</param>
    /// <returns>An instance of INumberAssert for further assertions.</returns>
    public static INumberAssert<decimal> AssertThat(decimal current) => new NumberAssert<decimal>(current);

    /// <summary>
    ///     An Assertion to verify dictionary values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="current">The current dictionary value to verify.</param>
    /// <returns>An instance of IDictionaryAssert for further assertions.</returns>
    public static IDictionaryAssert<TKey, TValue> AssertThat<TKey, TValue>(IDictionary<TKey, TValue>? current)
        where TKey : notnull
        where TValue : notnull
        => new DictionaryAssert<TKey, TValue>(current);

    /// <summary>
    ///     An Assertion to verify dictionary values.
    /// </summary>
    /// <param name="current">The current dictionary value to verify.</param>
    /// <returns>An instance of IDictionaryAssert for further assertions.</returns>
    public static IDictionaryAssert<Variant, Variant> AssertThat(Dictionary? current)
        => new DictionaryAssert<Variant, Variant>(current);

    /// <summary>
    ///     An Assertion to verify dictionary values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="current">The current dictionary value to verify.</param>
    /// <returns>An instance of IDictionaryAssert for further assertions.</returns>
    public static IDictionaryAssert<TKey, TValue> AssertThat<[MustBeVariant] TKey, [MustBeVariant] TValue>(Dictionary<TKey, TValue>? current)
        where TKey : notnull
        where TValue : notnull
        => new DictionaryAssert<TKey, TValue>(current);

    /// <summary>
    ///     The dynamic assertions for all enumerable types.
    /// </summary>
    /// <param name="current">The enumerable value to verify.</param>
    /// <returns>An instance of IEnumerableAssert for further assertions.</returns>
    public static IEnumerableAssert<object?> AssertThat(IEnumerable? current)
        => new EnumerableAssert<object?>(current);

    /// <summary>
    ///     The dynamic assertions for all enumerable types.
    /// </summary>
    /// <param name="current">The enumerable value to verify.</param>
    /// <returns>An instance of IEnumerableAssert for further assertions.</returns>
    public static IEnumerableAssert<bool> AssertThat(BitArray? current)
        => new EnumerableAssert<bool>(current);

    /// <summary>
    ///     The dynamic assertions for all enumerable types.
    /// </summary>
    /// <typeparam name="TValue">The type of elements in the enumerable.</typeparam>
    /// <param name="current">The enumerable value to verify.</param>
    /// <returns>An instance of IEnumerableAssert for further assertions.</returns>
    public static IEnumerableAssert<TValue?> AssertThat<TValue>(IEnumerable<TValue?>? current)
        => new EnumerableAssert<TValue?>(current);

    /// <summary>
    ///     The dynamic assertions for all enumerable types.
    /// </summary>
    /// <typeparam name="TValue">The type of elements in the array.</typeparam>
    /// <param name="current">The array values to verify.</param>
    /// <returns>An instance of IEnumerableAssert for further assertions.</returns>
    [SuppressMessage(
        "StyleCop.CSharp.SpacingRules",
        "SA1011:ClosingSquareBracketsMustBeSpacedCorrectly",
        Justification = "A closing square bracket within a C# statement is not spaced correctly.")]
    public static IEnumerableAssert<TValue?> AssertThat<TValue>(params TValue?[]? current)
        => new EnumerableAssert<TValue?>(current);

    /// <summary>
    ///     The dynamic assertions for all enumerable types.
    /// </summary>
    /// <param name="current">The enumerable value to verify.</param>
    /// <returns>An instance of IEnumerableAssert for further assertions.</returns>
    public static IEnumerableAssert<Variant> AssertThat(Array? current)
        => new EnumerableAssert<Variant>(current);

    /// <summary>
    ///     The dynamic assertions for all enumerable types.
    /// </summary>
    /// <typeparam name="TValue">The type of elements in the array.</typeparam>
    /// <param name="current">The enumerable value to verify.</param>
    /// <returns>An instance of IEnumerableAssert for further assertions.</returns>
    public static IEnumerableAssert<TValue?> AssertThat<[MustBeVariant] TValue>(Array<TValue>? current)
        => new EnumerableAssert<TValue?>(current);

    /// <summary>
    ///     The dynamic assertions for all Godot vector types.
    /// </summary>
    /// <param name="current">The vector value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    public static IVectorAssert<Vector2> AssertThat(Vector2 current) => new VectorAssert<Vector2>(current);

    /// <summary>
    ///     The dynamic assertions for all Godot vector types.
    /// </summary>
    /// <param name="current">The vector value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    public static IVectorAssert<Vector2I> AssertThat(Vector2I current) => new VectorAssert<Vector2I>(current);

    /// <summary>
    ///     The dynamic assertions for all Godot vector types.
    /// </summary>
    /// <param name="current">The vector value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    public static IVectorAssert<Vector3> AssertThat(Vector3 current) => new VectorAssert<Vector3>(current);

    /// <summary>
    ///     The dynamic assertions for all Godot vector types.
    /// </summary>
    /// <param name="current">The vector value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    public static IVectorAssert<Vector3I> AssertThat(Vector3I current) => new VectorAssert<Vector3I>(current);

    /// <summary>
    ///     The dynamic assertions for all Godot vector types.
    /// </summary>
    /// <param name="current">The vector value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    public static IVectorAssert<Vector4> AssertThat(Vector4 current) => new VectorAssert<Vector4>(current);

    /// <summary>
    ///     The dynamic assertions for all Godot vector types.
    /// </summary>
    /// <param name="current">The vector value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    public static IVectorAssert<Vector4I> AssertThat(Vector4I current) => new VectorAssert<Vector4I>(current);

    /// <summary>
    ///     The dynamic assertions for all Godot vector types.
    /// </summary>
    /// <param name="current">The vector value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    public static IVectorAssert<System.Numerics.Vector2> AssertThat(System.Numerics.Vector2 current)
        => new VectorAssert<System.Numerics.Vector2>(current);

    /// <summary>
    ///     The dynamic assertions for all Godot vector types.
    /// </summary>
    /// <param name="current">The vector value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    public static IVectorAssert<System.Numerics.Vector3> AssertThat(System.Numerics.Vector3 current)
        => new VectorAssert<System.Numerics.Vector3>(current);

    /// <summary>
    ///     The dynamic assertions for all Godot vector types.
    /// </summary>
    /// <param name="current">The vector value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    public static IVectorAssert<System.Numerics.Vector4> AssertThat(System.Numerics.Vector4 current)
        => new VectorAssert<System.Numerics.Vector4>(current);

    /// <summary>
    ///     A dynamic assertion for <see cref="Variant" /> based on the input type.
    /// </summary>
    /// <param name="current">The input value to be asserted.</param>
    /// <returns>A dynamic assertion that provides assertion methods based on the input type.</returns>
    public static dynamic AssertThat(Variant current) => AssertThat(current.UnboxVariant());

    /// <summary>
    ///     A dynamic assertion based on the input type.
    /// </summary>
    /// <typeparam name="TValue">The type of the input.</typeparam>
    /// <param name="current">The input value to be asserted.</param>
    /// <returns>A dynamic assertion that provides assertion methods based on the input type.</returns>
    public static dynamic AssertThat<TValue>(TValue? current)
    {
        var valueType = typeof(TValue);

        if (typeof(IDictionary).IsAssignableFrom(valueType))
        {
            if (!valueType.IsGenericType)
                return DictionaryAssert<object, object?>.From(current as IDictionary);

            var assertType = typeof(DictionaryAssert<,>).MakeGenericType(valueType.GenericTypeArguments);
            var constructorArgType = typeof(IDictionary<,>).MakeGenericType(valueType.GenericTypeArguments);
            var constructor = (assertType.GetConstructor(
                                   BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                                   null,
                                   [constructorArgType],
                                   null)
                               ?? assertType.GetConstructor(
                                   BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                                   null,
                                   [valueType],
                                   null))
                              ?? throw new InvalidOperationException($"No suitable constructor found for {assertType.Name}");

            var instance = constructor.Invoke([current!]);
            return instance;
        }

        if (typeof(IEnumerable).IsAssignableFrom(valueType))
        {
            if (!valueType.IsGenericType)
                return AssertThat(current as IEnumerable);

            var assertType = typeof(EnumerableAssert<>).MakeGenericType(valueType.GenericTypeArguments[0]);
            var constructorArgType = typeof(IEnumerable<>).MakeGenericType(valueType.GenericTypeArguments[0]);
            var constructor = assertType.GetConstructor(
                                  BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                                  null,
                                  [constructorArgType],
                                  null)
                              ?? assertType.GetConstructor(
                                  BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                                  null,
                                  [valueType],
                                  null)
                              ?? throw new InvalidOperationException($"No suitable constructor found for {assertType.Name}");

            var instance = constructor.Invoke([current!]);
            return instance;
        }

        return new ObjectAssert(current);
    }

    /// <summary>
    ///     Asserts that an exception of type <see cref="Exception" /> is thrown when executing the supplied action.
    /// </summary>
    /// <param name="action">An action that may throw an exception.</param>
    /// <returns>An instance of <see cref="IExceptionAssert" /> for further assertions on the thrown exception.</returns>
    public static IExceptionAssert AssertThrown(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return new ExceptionAssert<Exception>(action);
    }

    /// <summary>
    ///     An Assertion to verify for expecting exceptions when performing a task.
    /// </summary>
    /// <typeparam name="TAction">The type of the task result.</typeparam>
    /// <param name="task">A task that may throw an exception during execution.</param>
    /// <returns>
    ///     A task that resolves to an <see cref="IExceptionAssert" /> if an exception was thrown,
    ///     or null if the task completed successfully.
    /// </returns>
    /// <remarks>
    ///     This overload accepts a generic Task and captures any exception thrown during execution.
    /// </remarks>
    /// <example>
    ///     <code>
    ///     await AssertThrown(task.WithTimeout(500))
    ///        .ContinueWith(result => result.Result.HasMessage("timed out after 500ms."));
    ///     </code>
    /// </example>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Method purpose is to capture and assert on any exception")]
    public static async Task<IExceptionAssert?> AssertThrown<TAction>(Task<TAction> task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            _ = await task.ConfigureAwait(true);
            return default;
        }
        catch (Exception e)
        {
            return new ExceptionAssert<Exception>(e);
        }
    }

    /// <summary>
    ///     An Assertion to verify for expecting exceptions when performing a task.
    /// </summary>
    /// <param name="task">A task that may throw an exception during execution.</param>
    /// <returns>
    ///     A task that resolves to an <see cref="IExceptionAssert" /> if an exception was thrown,
    ///     or null if the task completed successfully.
    /// </returns>
    /// <remarks>
    ///     This overload accepts a non-generic Task and captures any exception thrown during execution.
    /// </remarks>
    /// <example>
    ///     <code>
    ///     await AssertThrown(asyncOperation.WithTimeout(500))
    ///         .ContinueWith(result => result.Result.HasMessage("Operation timed out"));
    ///     </code>
    /// </example>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Method purpose is to capture and assert on any exception")]
    public static async Task<IExceptionAssert?> AssertThrown(Task task)
    {
        ArgumentNullException.ThrowIfNull(task);
        try
        {
            await task.ConfigureAwait(true);
            return default;
        }
        catch (Exception e)
        {
            return new ExceptionAssert<Exception>(e);
        }
    }

    /// ----------- Helpers -------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Adds the Node on the actual SceneTree to be processed during test execution.
    ///     The node is auto-freed and can be disabled by set autoFree = false.
    /// </summary>
    /// <typeparam name="TNode">The type of node to add.</typeparam>
    /// <param name="node">The node to add to the scene tree.</param>
    /// <param name="autoFree">Whether to automatically free the node after test execution.</param>
    /// <returns>The added node for method chaining.</returns>
    public static TNode AddNode<TNode>(TNode node, bool autoFree = true)
        where TNode : Node
    {
        if (autoFree)
            _ = AutoFree(node);
        var tree = Engine.GetMainLoop() as SceneTree;
        tree!.Root.AddChild(node);
        return node;
    }

    /// <summary>
    ///     A little helper to auto-freeing your created objects after test execution.
    /// </summary>
    /// <typeparam name="TValue">The type of Godot object to register.</typeparam>
    /// <param name="obj">The Godot object to register for automatic cleanup.</param>
    /// <returns>The registered object for method chaining.</returns>
    public static TValue? AutoFree<TValue>(TValue? obj)
        where TValue : GodotObject
        => MemoryPool.RegisterForAutoFree(obj);

    /// <summary>
    ///     Builds a tuple by given values.
    /// </summary>
    /// <param name="args">The values to include in the tuple.</param>
    /// <returns>A new tuple containing the specified values.</returns>
    public static ITuple Tuple(params object?[] args) => new Tuple(args);

    /// <summary>
    ///     Builds an extractor by given method name and optional arguments.
    /// </summary>
    /// <param name="methodName">The name of the method to extract.</param>
    /// <param name="args">Optional arguments to pass to the method.</param>
    /// <returns>A value extractor that can be used with assertion methods.</returns>
    public static IValueExtractor Extr(string methodName, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(methodName);
        return new ValueExtractor(methodName, args);
    }

    /// <summary>
    ///     Waits for the specified signal to be emitted by a particular source node.
    ///     If the signal is not emitted within the given timeout, the operation fails.
    /// </summary>
    /// <example>
    ///     <code>
    ///      // Waits for the signal "mySignal" is emitted by myNode.
    ///      await runner.AwaitSignalOn(myNode, "mySignal");
    ///   </code>
    /// </example>
    /// <param name="source">The object from which the signal is emitted.</param>
    /// <param name="signal">The name of the signal to wait.</param>
    /// <param name="args">An optional set of signal arguments.</param>
    /// <returns>Task to wait.</returns>
    public static async Task<ISignalAssert> AwaitSignalOn(GodotObject source, string signal, params Variant[] args) =>
        await ISceneRunner.AwaitSignalOn(source, signal, args).ConfigureAwait(true);

    /// <summary>
    ///     Provides the expected line number via compiler state.
    ///     Is primarily designed to use on internal test coverage to validate the reported error line is correct.
    /// </summary>
    /// <param name="lineNumber">The calling line number, automatically provided by the compiler.</param>
    /// <returns>The adjusted line number for error reporting.</returns>
    internal static int ExpectedLineNumber([CallerLineNumber] int lineNumber = 0) => lineNumber - 1;
}
