namespace GdUnit4;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Asserts;

/// <summary>
/// A collection of assertions and helpers to verify values
/// </summary>
public sealed class Assertions
{
    /// <summary>
    /// An Assertion to verify boolean values
    /// </summary>
    /// <param name="current">The current boolean value to verify</param>
    /// <returns>IBoolAssert</returns>
    public static IBoolAssert AssertBool(bool current) => new BoolAssert(current);

    /// <summary>
    /// An Assertion to verify string values
    /// </summary>
    /// <param name="current">The current string value to verify</param>
    /// <returns></returns>
    public static IStringAssert AssertString(string? current) => new StringAssert(current);

    /// <summary>
    /// An Assertion to verify integer values
    /// </summary>
    /// <param name="current">The current integer value to verify</param>
    /// <returns></returns>
    public static INumberAssert<int> AssertInt(int current) => new NumberAssert<int>(current);

    /// <summary>
    /// An Assertion to verify double values
    /// </summary>
    /// <param name="current">The current double value to verify</param>
    /// <returns></returns>
    public static INumberAssert<double> AssertFloat(double current) => new NumberAssert<double>(current);

    /// <summary>
    /// An Assertion to verify object values
    /// </summary>
    /// <param name="current">The current double value to verify</param>
    /// <returns></returns>
    public static IObjectAssert AssertObject(object? current) => new ObjectAssert(current);

    /// <summary>
    /// An Assertion to verify array values
    /// </summary>
    /// <param name="current">The current array value to verify</param>
    /// <returns></returns>
    public static IEnumerableAssert<TValue?> AssertArray<TValue>(IEnumerable<TValue?>? current)
        => new EnumerableAssert<TValue?>(current);

    public static IEnumerableAssert<object?> AssertArray(IEnumerable<object?>? current)
        => new EnumerableAssert<object?>(current);


    /// <summary>
    /// An assertion method for all Godot vector types.
    /// </summary>
    /// <typeparam name="TValue">The type of Godot vector.</typeparam>
    /// <param name="vector">The vector value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    public static IVectorAssert<TValue> AssertVector<TValue>(TValue vector) where TValue : IEquatable<TValue>
        => new VectorAssert<TValue>(vector);

    /// <summary>
    /// An Assertion to verify Godot.Vector2 values
    /// </summary>
    /// <param name="current">The current vector2 value to verify</param>
    /// <returns></returns>
    [Obsolete("AssertVec2 is deprecated, please use AssertVector instead.")]
    public static IVectorAssert<Godot.Vector2> AssertVec2(Godot.Vector2 current) => AssertVector(current);

    /// <summary>
    /// An Assertion to verify Godot.Vector3 values
    /// </summary>
    /// <param name="current">The current vector3 value to verify</param>
    /// <returns></returns>
    [Obsolete("AssertVec3 is deprecated, please use AssertVector instead.")]
    public static IVectorAssert<Godot.Vector3> AssertVec3(Godot.Vector3 current) => AssertVector(current);

    /// <summary>
    /// An Assertion used by test generation to notify the test is not yet implemented
    /// </summary>
    /// <returns></returns>
    public static bool AssertNotYetImplemented() => throw new Exceptions.TestFailedException("Test not yet implemented!");

    /// <summary>
    /// An Assertion to verify Godot signals
    /// </summary>
    /// <param name="node">The object where is emitting the signal</param>
    /// <returns></returns>
    public static ISignalAssert AssertSignal(Godot.GodotObject node) => new SignalAssert(node);

    public static IStringAssert AssertThat(string? current) => new StringAssert(current);

    public static IBoolAssert AssertThat(bool current) => new BoolAssert(current);

    /// <summary>
    /// numeric asserts
    /// </summary>
    /// <param name="current"></param>
    /// <returns></returns>
    public static INumberAssert<sbyte> AssertThat(sbyte current) => new NumberAssert<sbyte>(current);
    public static INumberAssert<byte> AssertThat(byte current) => new NumberAssert<byte>(current);
    public static INumberAssert<short> AssertThat(short current) => new NumberAssert<short>(current);
    public static INumberAssert<ushort> AssertThat(ushort current) => new NumberAssert<ushort>(current);
    public static INumberAssert<int> AssertThat(int current) => new NumberAssert<int>(current);
    public static INumberAssert<uint> AssertThat(uint current) => new NumberAssert<uint>(current);
    public static INumberAssert<long> AssertThat(long current) => new NumberAssert<long>(current);
    public static INumberAssert<ulong> AssertThat(ulong current) => new NumberAssert<ulong>(current);
    public static INumberAssert<float> AssertThat(float current) => new NumberAssert<float>(current);
    public static INumberAssert<double> AssertThat(double current) => new NumberAssert<double>(current);
    public static INumberAssert<decimal> AssertThat(decimal current) => new NumberAssert<decimal>(current);

    public static IDictionaryAssert<TKey, TValue> AssertThat<TKey, TValue>(IDictionary<TKey, TValue>? current)
        where TKey : notnull
        where TValue : notnull
        => new DictionaryAssert<TKey, TValue>(current);

    public static IDictionaryAssert<Godot.Variant, Godot.Variant> AssertThat(Godot.Collections.Dictionary? current)
       => new DictionaryAssert<Godot.Variant, Godot.Variant>(current);

    public static IDictionaryAssert<TKey, TValue> AssertThat<[Godot.MustBeVariant] TKey, [Godot.MustBeVariant] TValue>(Godot.Collections.Dictionary<TKey, TValue>? current)
        where TKey : notnull
        where TValue : notnull
       => new DictionaryAssert<TKey, TValue>(current);


    /// <summary>
    /// The dynamic assertions for all enumerable types.
    /// </summary>
    /// <param name="current">The enumerable value to verify.</param>
    /// <returns>An instance of IEnumerableAssert for further assertions.</returns>
    public static IEnumerableAssert<object?> AssertThat(IEnumerable? current)
        => new EnumerableAssert<object?>(current);

    public static IEnumerableAssert<bool> AssertThat(BitArray? current)
        => new EnumerableAssert<bool>(current);

    public static IEnumerableAssert<TValue?> AssertThat<TValue>(IEnumerable<TValue?>? current)
        => new EnumerableAssert<TValue?>(current);

    public static IEnumerableAssert<TValue?> AssertThat<TValue>(params TValue?[]? current)
        => new EnumerableAssert<TValue?>(current);

    public static IEnumerableAssert<Godot.Variant> AssertThat(Godot.Collections.Array? current)
        => new EnumerableAssert<Godot.Variant>(current);

    public static IEnumerableAssert<TValue?> AssertThat<[Godot.MustBeVariant] TValue>(Godot.Collections.Array<TValue>? current)
        => new EnumerableAssert<TValue?>(current);


    /// <summary>
    /// The dynamic assertions for all Godot vector types.
    /// </summary>
    /// <param name="current">The vector value to verify.</param>
    /// <returns>An instance of IVectorAssert for further assertions.</returns>
    public static IVectorAssert<Godot.Vector2> AssertThat(Godot.Vector2 current) => new VectorAssert<Godot.Vector2>(current);
    public static IVectorAssert<Godot.Vector2I> AssertThat(Godot.Vector2I current) => new VectorAssert<Godot.Vector2I>(current);
    public static IVectorAssert<Godot.Vector3> AssertThat(Godot.Vector3 current) => new VectorAssert<Godot.Vector3>(current);
    public static IVectorAssert<Godot.Vector3I> AssertThat(Godot.Vector3I current) => new VectorAssert<Godot.Vector3I>(current);
    public static IVectorAssert<Godot.Vector4> AssertThat(Godot.Vector4 current) => new VectorAssert<Godot.Vector4>(current);
    public static IVectorAssert<Godot.Vector4I> AssertThat(Godot.Vector4I current) => new VectorAssert<Godot.Vector4I>(current);


    /// <summary>
    /// A dynamic assertion for <see cref="Godot.Variant"/> based on the input type.
    /// </summary>
    /// <param name="current">The input value to be asserted.</param>
    /// <returns>A dynamic assert object that provides assertion methods based on the input type.</returns>
    public static dynamic AssertThat(Godot.Variant current) => AssertThat(current.UnboxVariant());


    /// <summary>
    /// A dynamic assertion based on the input type.
    /// </summary>
    /// <typeparam name="TValue">The type of the input.</typeparam>
    /// <param name="current">The input value to be asserted.</param>
    /// <returns>A dynamic assert object that provides assertion methods based on the input type.</returns>
    public static dynamic AssertThat<TValue>(TValue? current)
    {
        var valueType = typeof(TValue);

        if (typeof(IDictionary).IsAssignableFrom(valueType))
        {
            if (valueType.IsGenericType)
            {
                var assertType = typeof(DictionaryAssert<,>).MakeGenericType(valueType.GenericTypeArguments);
                return Activator.CreateInstance(assertType, current)!;
            }
            return DictionaryAssert<object, object?>.From(current as IDictionary);
        }
        if (typeof(IEnumerable).IsAssignableFrom(valueType))
        {
            if (valueType.IsGenericType)
            {
                var assertType = typeof(EnumerableAssert<>).MakeGenericType(valueType.GenericTypeArguments[0]);
                return Activator.CreateInstance(assertType, current)!;
            }
            return AssertThat(current as IEnumerable);
        }
        return new ObjectAssert(current);
    }

    /// <summary>
    /// Asserts that an exception of type <see cref="Exception"/> is thrown when executing the supplied action.
    /// </summary>
    /// <param name="action">An action that may throw an exception.</param>
    /// <returns>An instance of <see cref="IExceptionAssert"/> for further assertions on the thrown exception.</returns>
    public static IExceptionAssert AssertThrown(Action action) =>
        new ExceptionAssert<Exception>(action);


    /// <summary>
    /// An Assertion to verify for expecting exceptions when performing a task.
    /// <example>
    /// <code>
    ///     await AssertThrown(task.WithTimeout(500))
    ///        .ContinueWith(result => result.Result.HasMessage("timed out after 500ms."));
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="task">A task where throw possible exceptions</param>
    /// <returns>a task of <c>IExceptionAssert</c> to await</returns>
    public static async Task<IExceptionAssert?> AssertThrown<TAction>(Task<TAction> task)
    {
        try
        {
            await task;
            return default;
        }
        catch (Exception e)
        {
            return new ExceptionAssert<Exception>(e);
        }
    }

    public static async Task<IExceptionAssert?> AssertThrown(Task task)
    {
        try
        {
            await task;
            return default;
        }
        catch (Exception e)
        {
            return new ExceptionAssert<Exception>(e);
        }
    }

    /// ----------- Helpers -------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Adds the Node on actual SceneTree to be processed during test execution.
    /// The node is auto freed and can be disabled by set autoFree = false.
    /// </summary>
    public static TNode AddNode<TNode>(TNode node, bool autoFree = true) where TNode : Godot.Node
    {
        if (autoFree)
            AutoFree(node);
        var tree = Godot.Engine.GetMainLoop() as Godot.SceneTree;
        tree!.Root.AddChild(node);
        return node;
    }

    ///<summary>
    /// A little helper to auto freeing your created objects after test execution
    /// </summary>
    public static TValue? AutoFree<TValue>(TValue? obj) where TValue : Godot.GodotObject
        => Executions.Monitors.MemoryPool.RegisterForAutoFree(obj);

    /// <summary>
    /// Builds a tuple by given values
    /// </summary>
    public static ITuple Tuple(params object?[] args) => new Asserts.Tuple(args);

    /// <summary>
    ///  Builds an extractor by given method name and optional arguments
    /// </summary>
    public static IValueExtractor Extr(string methodName, params object[] args) => new ValueExtractor(methodName, args);


    /// <summary>
    /// Provides the expected line number via compile state.
    /// Is primary designed to use on internal test coverage to validate the reported error line is correct.
    /// </summary>
    /// <param name="lineNumber"></param>
    /// <returns></returns>
    internal static int ExpectedLineNumber([System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0) => lineNumber - 1;
}
