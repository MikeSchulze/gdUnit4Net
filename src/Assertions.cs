using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GdUnit3
{
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

        public static IStringAssert AssertThat(string current) => new StringAssert(current);
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


        public static IObjectAssert AssertThat(object? current) => new ObjectAssert(current);
        public static IEnumerableAssert AssertThat(IEnumerable? current) => new EnumerableAssert(current);
        public static IVector2Assert AssertThat(Godot.Vector2 current) => new Vector2Assert(current);
        public static IVector3Assert AssertThat(Godot.Vector3 current) => new Vector3Assert(current);


        /// <summary>
        /// An Assertion to verify for expecting exceptions
        /// </summary>
        /// <param name="supplier">A function callback where throw possible exceptions</param>
        /// <returns>IExceptionAssert</returns>
        public static IExceptionAssert AssertThrown<T>(Func<T> supplier) => new ExceptionAssert<T>(supplier);

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
        public async static Task<IExceptionAssert?> AssertThrown<T>(Task<T> task)
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

        public async static Task<IExceptionAssert?> AssertThrown(Task task)
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

        ///<summary>
        /// A litle helper to auto freeing your created objects after test execution
        /// </summary>
        public static T AutoFree<T>(T obj) where T : Godot.Object => Executions.Monitors.MemoryPool.RegisterForAutoFree(obj);

        /// <summary>
        /// Buils a tuple by given values
        /// </summary>
        public static ITuple Tuple(params object?[] args) => new GdUnit3.Asserts.Tuple(args);

        /// <summary>
        ///  Builds an extractor by given method name and optional arguments
        /// </summary>
        public static IValueExtractor Extr(string methodName, params object[] args) => new ValueExtractor(methodName, args);

        /// <summary>
        ///  A helper to return given enumerable as string representation
        /// </summary>
        public static string AaString(IEnumerable values)
        {
            var items = new List<string>();
            foreach (var value in values)
            {
                items.Add(value != null ? value.ToString() : "Null");
            }
            return string.Join(", ", items);
        }
    }
}
