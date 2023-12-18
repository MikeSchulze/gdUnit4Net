using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace GdUnit4
{
    using System.Diagnostics.CodeAnalysis;
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
        public static IEnumerableAssert AssertArray(IEnumerable? current) => new EnumerableAssert(current);

        /// <summary>
        /// An Assertion to verify Godot.Vector2 values
        /// </summary>
        /// <param name="current">The current vector2 value to verify</param>
        /// <returns></returns>
        public static IVector2Assert AssertVec2(Godot.Vector2 current) => new Vector2Assert(current);

        /// <summary>
        /// An Assertion to verify Godot.Vector3 values
        /// </summary>
        /// <param name="current">The current vector3 value to verify</param>
        /// <returns></returns>
        public static IVector3Assert AssertVec3(Godot.Vector3 current) => new Vector3Assert(current);

        /// <summary>
        /// An Assertion used by test generation to notify the test is not yet implemented
        /// </summary>
        /// <returns></returns>
        public static bool AssertNotYetImplemented() => throw new Exceptions.TestFailedException("Test not yet implemented!", -1);

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

        public static IDictionaryAssert<K, V> AssertThat<K, V>(IDictionary<K, V>? current) where K : notnull
            => new DictionaryAssert<K, V>(current?.ToDictionary(e => e.Key, e => e.Value));

        public static IDictionaryAssert<Godot.Variant, Godot.Variant> AssertThat(Godot.Collections.Dictionary? current)
           => new DictionaryAssert<Godot.Variant, Godot.Variant>(current);

        public static IDictionaryAssert<TKey, TValue> AssertThat<[Godot.MustBeVariant] TKey, [Godot.MustBeVariant] TValue>(Godot.Collections.Dictionary<TKey, TValue>? current) where TKey : notnull
           => new DictionaryAssert<TKey, TValue>(current);

        public static IVector2Assert AssertThat(Godot.Vector2 current) => new Vector2Assert(current);
        public static IVector3Assert AssertThat(Godot.Vector3 current) => new Vector3Assert(current);


        /// <summary>
        /// A dynamic assertion for <see cref="Godot.Variant"/> based on the input type.
        /// </summary>
        /// <param name="current">The input value to be asserted.</param>
        /// <returns>A dynamic assert object that provides assertion methods based on the input type.</returns>
        public static dynamic AssertThat(Godot.Variant current) => AssertThat(current.UnboxVariant());


        /// <summary>
        /// A dynamic assertion based on the input type.
        /// </summary>
        /// <typeparam name="T">The type of the input.</typeparam>
        /// <param name="current">The input value to be asserted.</param>
        /// <returns>A dynamic assert object that provides assertion methods based on the input type.</returns>
        public static dynamic AssertThat<T>(T? current)
        {
            var type = typeof(T);
            if (type == typeof(IDictionary) && current == null)
            {
                var assertType = typeof(DictionaryAssert<,>).MakeGenericType(typeof(object), typeof(object));
                return Activator.CreateInstance(assertType, current)!;
            }
            if (current is IDictionary dv)
            {
                if (type.IsGenericType)
                {
                    var dictionaryTypeArgs = type.GetGenericArguments();
                    var keyType = dictionaryTypeArgs[0];
                    var valueType = dictionaryTypeArgs[1];
                    var assertType = typeof(DictionaryAssert<,>).MakeGenericType(keyType, valueType);
                    return Activator.CreateInstance(assertType, dv)!;
                }
                return new DictionaryAssert<object, object?>(dv
                    .Cast<DictionaryEntry>()
                    .ToDictionary(entry => (object)entry.Key, entry => (object?)entry.Value));
            }

            if (current is IEnumerable ev)
                return new EnumerableAssert(ev);
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
        public static async Task<IExceptionAssert?> AssertThrown<T>(Task<T> task)
        {
            try
            {
                await task;
                return default;
            }
            catch (Exception e)
            {
                return new ExceptionAssert<T>(e);
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
                return new ExceptionAssert<object>(e);
            }
        }

        /// ----------- Helpers -------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Adds the Node on actual SceneTree to be processed during test execution.
        /// The node is auto freed and can be disabled by set autoFree = false.
        /// </summary>
        public static T AddNode<T>(T node, bool autoFree = true) where T : Godot.Node
        {
            if (autoFree)
                AutoFree(node);
            Godot.SceneTree? tree = Godot.Engine.GetMainLoop() as Godot.SceneTree;
            tree!.Root.AddChild(node);
            return node;
        }

        ///<summary>
        /// A litle helper to auto freeing your created objects after test execution
        /// </summary>
        public static T? AutoFree<T>(T? obj) where T : Godot.GodotObject => Executions.Monitors.MemoryPool.RegisterForAutoFree(obj);

        /// <summary>
        /// Buils a tuple by given values
        /// </summary>
        public static ITuple Tuple(params object?[] args) => new GdUnit4.Asserts.Tuple(args);

        /// <summary>
        ///  Builds an extractor by given method name and optional arguments
        /// </summary>
        public static IValueExtractor Extr(string methodName, params object[] args) => new ValueExtractor(methodName, args);
    }
}
