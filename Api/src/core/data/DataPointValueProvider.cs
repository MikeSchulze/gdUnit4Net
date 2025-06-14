// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Data;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Execution;

using Extensions;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
///     Provides functionality to retrieve and process test data from data point sources for data-driven tests.
///     This class handles both synchronous and asynchronous data sources, supporting various data formats
///     including arrays, enumerable, and single values.
/// </summary>
/// <remarks>
///     The DataPointValueProvider supports the following data source patterns:
///     <list type="bullet">
///         <item>
///             <term>Sync Data Sources</term>
///             <description>
///                 - Static properties or methods returning IEnumerable{object?[]}
///                 - Static properties or methods returning IEnumerable{T} where T is converted to object?[]
///                 - Parameterized methods for dynamic data generation
///             </description>
///         </item>
///         <item>
///             <term>Async Data Sources</term>
///             <description>
///                 - IAsyncEnumerable{object?[]} for direct array data
///                 - IAsyncEnumerable{T} where T is wrapped in a single-element array
///             </description>
///         </item>
///     </list>
///     Example usages:
///     <code>
/// // Synchronous array data
/// public static IEnumerable{object[]} TestData => new[]
/// {
///     new object[] { 1, 2, 3 },
///     new object[] { 4, 5, 9 }
/// };
///
/// // Asynchronous array data
/// public static async IAsyncEnumerable{object?[]} AsyncData()
/// {
///     yield return new object?[] { 1, 2, 3 };
///     await Task.Delay(10);
///     yield return new object?[] { 4, 5, 9 };
/// }
///
/// // Single value async data
/// public static async IAsyncEnumerable{int} AsyncInts()
/// {
///     yield return 1;
///     await Task.Delay(10);
///     yield return 2;
/// }
/// </code>
/// </remarks>
internal static class DataPointValueProvider
{
    internal static IEnumerable<object?[]> GetData(TestCase testCase)
    {
        var dataPoint = testCase.DataPoint ??
                        throw new ArgumentNullException($"No data point specified at {testCase.DataPoint}.");
        var declaringType = dataPoint.DataPointDeclaringType ?? testCase.MethodInfo.DeclaringType ??
            throw new ArgumentNullException($"No declaring type found for {dataPoint.DataPointDeclaringType}");

        // get data from property
        var dataSource = GetDataPointSource(declaringType, dataPoint, out _) ??
                         throw new ArgumentNullException($"Value returned by property or method {dataPoint.DataPointSource} shouldn't be null.");

        if (!TryGetData(dataSource, out var data))
            throw new ArgumentException($"Data source '{dataPoint.DataPointSource}' in {declaringType.FullName} must return IEnumerable<object?[]> or IEnumerable<object?>");

        return data;
    }

    internal static async IAsyncEnumerable<object?[]> GetDataAsync(TestCase testCase, TimeSpan timeout)
    {
        var dataPoint = testCase.DataPoint ??
                        throw new ArgumentNullException($"No data point specified at {testCase.DataPoint}.");

        var declaringType = dataPoint.DataPointDeclaringType ?? testCase.MethodInfo.DeclaringType ??
            throw new ArgumentNullException($"No declaring type found for {dataPoint.DataPointDeclaringType}");

        var dataSource = GetDataPointSource(declaringType, dataPoint, out var returnType) ??
                         throw new ArgumentNullException($"Value returned by async property or method {dataPoint.DataPointSource} shouldn't be null.");

        if (!IsAsyncEnumerableType(returnType!))
            throw new ArgumentException($"Data source '{dataPoint.DataPointSource}' in {declaringType.FullName} must return IAsyncEnumerable<T>");

        await foreach (var item in StreamAsyncData(dataSource, returnType!, timeout).ConfigureAwait(false))
            yield return item;
    }

    internal static bool IsAsyncDataPoint(TestCase testCase)
    {
        var dataPoint = testCase.DataPoint;
        if (dataPoint == null)
            return false;

        var declaringType = dataPoint.DataPointDeclaringType ?? testCase.MethodInfo.DeclaringType;
        if (declaringType == null)
            return false;

        // Check property
        var property = declaringType.GetProperty(
            dataPoint.DataPointSource,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

        if (property != null)
        {
            var propertyType = property.PropertyType;
            return IsAsyncEnumerableType(propertyType);
        }

        // Check method
        var method = declaringType.GetMethod(
            dataPoint.DataPointSource,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

        if (method != null)
        {
            var returnType = method.ReturnType;
            return IsAsyncEnumerableType(returnType);
        }

        return false;
    }

    private static async IAsyncEnumerable<object?[]> StreamAsyncData(object asyncSource, Type returnType, TimeSpan timeout)
    {
        var elementType = returnType.GetGenericArguments()[0];
        var isArrayValue = elementType == typeof(object?[]) || elementType == typeof(object[]);
        var enumerableInterface = typeof(IAsyncEnumerable<>).MakeGenericType(elementType);
        var enumeratorInterface = typeof(IAsyncEnumerator<>).MakeGenericType(elementType);

        using var cancellationToken = new CancellationTokenSource(timeout);

        // Get the GetAsyncEnumerator method
        var getEnumeratorMethod = enumerableInterface.GetMethod("GetAsyncEnumerator")
                                  ?? throw new InvalidOperationException($"Could not find GetAsyncEnumerator method on {enumerableInterface.FullName}");
        var enumerator = getEnumeratorMethod.Invoke(asyncSource, new object[] { cancellationToken.Token })
                         ?? throw new InvalidOperationException("GetAsyncEnumerator returned null");
        var moveNextMethod = enumeratorInterface.GetMethod("MoveNextAsync")
                             ?? throw new InvalidOperationException("Could not find MoveNextAsync method");
        var currentProperty = enumeratorInterface.GetProperty("Current")
                              ?? throw new InvalidOperationException("Could not find Current property");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var moveNextTask = (ValueTask<bool>)moveNextMethod.Invoke(enumerator, null)!;
                    var hasNext = await moveNextTask
                        .AsTask()
                        .WaitAsync(cancellationToken.Token)
                        .ConfigureAwait(false);

                    if (!hasNext)
                        break;
                }
                catch (OperationCanceledException)
                {
                    throw new AsyncDataPointCanceledException(
                        $"The execution has timed out after {timeout.Humanize()}.",
                        BuildStackTrace(asyncSource));
                }

                var current = currentProperty.GetValue(enumerator);
                if (isArrayValue)
                {
                    if (current == null)
                        continue;
                    yield return (object?[])current;
                }
                else
                    yield return new[] { current };
            }
        }
        finally
        {
            try
            {
                if (enumerator is IAsyncDisposable disposable)
                    await disposable.DisposeAsync().ConfigureAwait(true);
            }
            catch (Exception e) when (e is NotImplementedException or NotSupportedException)
            {
                // ignore disposal exceptions
            }
        }
    }

    private static string? LookupClassPath(Type classType) => new DirectoryInfo(Directory.GetCurrentDirectory())
        .EnumerateFiles($"*{classType.Name}.cs", SearchOption.AllDirectories)
        .Where(file => FileContainsClassName(file.FullName, classType.Name))
        .Select(file => file.FullName)
        .FirstOrDefault();

    // Check if the class name exists in the file using a quick text check
    private static bool FileContainsClassName(string filePath, string className)
    {
        // Read file line-by-line instead of loading the whole file into memory
        using var reader = new StreamReader(filePath);
        while (reader.ReadLine() is { } line)
        {
            // If the line contains the class keyword and the class name, it's a likely candidate
            if (line.Contains($"class {className}", StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static string BuildStackTrace(object asyncSource)
    {
        try
        {
            var sourceType = asyncSource.GetType();
            if (!sourceType.Name.Contains("d__", StringComparison.Ordinal))
                return "at unknown location";

            var originalType = sourceType.DeclaringType;

            if (originalType == null)
                return "at unknown location";

            var methodName = sourceType.Name.Split('<', '>')[1];
            var sourceFile = LookupClassPath(originalType);
            if (sourceFile == null)
                return $"at {originalType.FullName}.{methodName}()";

            // If we found the source file, parse it to find the method line
            var sourceText = File.ReadAllText(sourceFile);
            var tree = CSharpSyntaxTree.ParseText(sourceText);
            var root = tree.GetRoot();

            var methodDeclaration = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.ValueText == methodName);

            if (methodDeclaration != null)
            {
                var lineSpan = tree.GetLineSpan(methodDeclaration.Span);
                var lineNumber = lineSpan.StartLinePosition.Line + 1;
                return $"at {originalType.FullName}.{methodName}() in {sourceFile}:line {lineNumber}";
            }

            // For now, return basic information while we examine the debug output
            return $"at {originalType.FullName}.{methodName}()";
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            Console.Error.WriteLine($"Error getting source location: {e}");
            return "at unknown location";
        }
    }

    private static object? GetDataPointSource(Type declaringType, DataPointAttribute dataPoint, out Type? returnType)
    {
        // First try to get as property
        var property = declaringType.GetProperty(
            dataPoint.DataPointSource,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

        if (property != null)
        {
            if (!property.CanRead)
                throw new ArgumentException($"Property '{dataPoint.DataPointSource}' in {declaringType.FullName} must have a getter");

            returnType = property.PropertyType;

            return property.GetValue(null, null);
        }

        // Then try as method
        var method = declaringType.GetMethod(
            dataPoint.DataPointSource,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance) ?? throw new ArgumentException(
            $"No property or method named '{dataPoint.DataPointSource}' found in {declaringType.FullName}");

        if (dataPoint.DataPointParameters != null)
            ValidateMethodParameters(method, dataPoint.DataPointParameters);

        returnType = method.ReturnType;
        return method.Invoke(null, dataPoint.DataPointParameters);
    }

    private static void ValidateMethodParameters(MethodInfo method, object?[] providedParameters)
    {
        var methodParameters = method.GetParameters();

        if (methodParameters.Length != providedParameters.Length)
            throw new ArgumentException($"Method '{method.Name}' expects {methodParameters.Length} parameters but {providedParameters.Length} were provided");

        for (var i = 0; i < methodParameters.Length; i++)
        {
            var parameterType = methodParameters[i].ParameterType;
            var providedValue = providedParameters[i];

            if (providedValue != null && !parameterType.IsInstanceOfType(providedValue))
                throw new ArgumentException($"Parameter {i} of method '{method.Name}' expects type {parameterType.Name} but got {providedValue.GetType().Name}");
        }
    }

    private static bool IsAsyncEnumerableType(Type type)
    {
        if (!type.IsGenericType)
            return false;
        return type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>);
    }

    private static bool TryGetData(object? dataSource, [NotNullWhen(true)] out IEnumerable<object?[]>? data)
    {
        data = null;

        if (dataSource == null)
            return false;

        // Handle IEnumerable<object?[]> directly
        if (dataSource is IEnumerable<object?[]> arrayData)
        {
            data = arrayData;
            return true;
        }

        // Handle IEnumerable<object?> by converting single objects to arrays
        if (dataSource is IEnumerable<object?> singleData)
        {
            data = singleData.Select(item => new[] { item });
            return true;
        }

        // Handle general IEnumerable
        if (dataSource is IEnumerable enumerable)
        {
            var resultList = new List<object?[]>();

            foreach (var item in enumerable)
            {
                if (item is object?[] array)
                    resultList.Add(array);
                else
                    resultList.Add(new[] { item });
            }

            if (resultList.Count == 0)
                return false;
            data = resultList;
            return true;
        }

        return false;
    }
}
