namespace NUnit.Extension.GdUnit4;

using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

public class DebuggerUtils
{
    public static int? GetDebugPort()
    {
        // Look for TestRunner port in command line
        var cmdLine = Environment.CommandLine;
        var match = Regex.Match(cmdLine, @"--port\s+(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var port))
        {
            Console.WriteLine($"Found TestRunner port from command line: {port}");
            return port;
        }

        Console.WriteLine("Command line: " + cmdLine);
        Console.WriteLine("No debug port found");
        return null;
    }

    public static void ListDebuggerTypes()
    {
        // Look for Rider's test runner assemblies
        var riderAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.Contains("JetBrains.ReSharper.TestRunner") == true);

        foreach (var assembly in riderAssemblies)
        {
            Console.WriteLine($"\nChecking assembly: {assembly.GetName().Name}");

            // Look for debugger-related types
            var debuggerTypes = assembly.GetTypes()
                .Where(t => t.Name.Contains("Debug") ||
                            t.Name.Contains("Server") ||
                            t.Name.Contains("Protocol"));

            foreach (var type in debuggerTypes)
            {
                Console.WriteLine($"\nType: {type.FullName}");

                // Look for methods related to starting a debugger server
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Static | BindingFlags.Instance)
                    .Where(m => m.Name.Contains("Debug") ||
                                m.Name.Contains("Server") ||
                                m.Name.Contains("Start"));

                foreach (var method in methods)
                {
                    Console.WriteLine($"  Method: {method.Name}");
                    foreach (var param in method.GetParameters()) Console.WriteLine($"    Parameter: {param.Name} ({param.ParameterType})");
                }
            }
        }
    }


    public static IFrameworkHandle GetVsTestFrameworkHandle()
    {
        try
        {
            // Try to get the TestEngine instance
            var engineType = Type.GetType("Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.TestEngine, Microsoft.VisualStudio.TestPlatform.CrossPlatEngine");
            if (engineType != null)
            {
                Console.WriteLine("Found TestEngine type");
                var engineInstance = engineType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (engineInstance != null) Console.WriteLine("Found TestEngine instance");
            }

            // Try to get the current test session
            var sessionType = Type.GetType("Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.TestSession, Microsoft.VisualStudio.TestPlatform.CrossPlatEngine");
            if (sessionType != null)
            {
                Console.WriteLine("Found TestSession type");
                var currentSession = sessionType.GetProperty("Current", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (currentSession != null) Console.WriteLine("Found current session");
            }

            // Look through loaded assemblies for the framework handle
            var vsAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name?.Contains("VisualStudio") == true ||
                            a.GetName().Name?.Contains("TestPlatform") == true);

            foreach (var assembly in vsAssemblies)
            {
                Console.WriteLine($"\nChecking assembly: {assembly.GetName().Name}");

                var handleTypes = assembly.GetTypes()
                    .Where(t => typeof(IFrameworkHandle).IsAssignableFrom(t) ||
                                t.Name.Contains("FrameworkHandle") ||
                                t.Name.Contains("TestContext"));

                foreach (var type in handleTypes)
                {
                    Console.WriteLine($"Found type: {type.FullName}");

                    // Look for current/active instance
                    var instanceProps = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic |
                                                           BindingFlags.Static | BindingFlags.Instance)
                        .Where(p => p.Name.Contains("Current") ||
                                    p.Name.Contains("Instance") ||
                                    p.Name.Contains("Active"));

                    foreach (var prop in instanceProps)
                        try
                        {
                            var instance = prop.GetValue(null);
                            if (instance != null) Console.WriteLine($"Found instance via {prop.Name}");
                        }
                        catch
                        {
                            // Ignore property access errors
                        }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accessing framework handle: {ex.Message}");
        }

        return null;
    }


    public static void AttachDebuggerToProcess_(int processId)
    {
        try
        {
            var mergedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "JetBrains.ReSharper.TestRunner.Merged");

            if (mergedAssembly != null)
            {
                // Find the generic Bootstrapper type
                var bootstrapperType = mergedAssembly.GetType("JetBrains.ReSharper.TestRunner.Bootstrapper`1");
                if (bootstrapperType != null)
                {
                    Console.WriteLine("Found Bootstrapper type");

                    // Try to get the TryAttachDebugger method
                    var attachMethod = bootstrapperType.GetMethod("TryAttachDebugger",
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                    if (attachMethod != null)
                    {
                        Console.WriteLine("Found TryAttachDebugger method");
                        Console.WriteLine("\nMethod details:");
                        Console.WriteLine($"Is Static: {attachMethod.IsStatic}");
                        Console.WriteLine($"Return Type: {attachMethod.ReturnType}");
                        Console.WriteLine("Parameters:");
                        foreach (var param in attachMethod.GetParameters()) Console.WriteLine($"  {param.Name}: {param.ParameterType}");

                        // Try to construct generic type if needed
                        if (bootstrapperType.IsGenericTypeDefinition)
                        {
                            var appType = mergedAssembly.GetType("JetBrains.ReSharper.TestRunner.IApplication");
                            if (appType != null)
                            {
                                var concreteType = bootstrapperType.MakeGenericType(appType);
                                var instance = Activator.CreateInstance(concreteType);
                                if (instance != null)
                                {
                                    Console.WriteLine("\nCreated Bootstrapper instance");
                                    try
                                    {
                                        var result = attachMethod.Invoke(instance, null);
                                        Console.WriteLine($"Attach result: {result}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Failed to invoke method: {ex.Message}");
                                    }
                                }
                            }
                        }
                        else if (attachMethod.IsStatic)
                        {
                            var result = attachMethod.Invoke(null, null);
                            Console.WriteLine($"Attach result: {result}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to use bootstrapper: {ex}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    public static void AttachDebuggerToProcess(int processId)
    {
        try
        {
            var mergedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "JetBrains.ReSharper.TestRunner.Merged");

            if (mergedAssembly != null)
            {
                // First find all types that implement IApplication
                var iAppType = mergedAssembly.GetType("JetBrains.ReSharper.TestRunner.IApplication");
                if (iAppType != null)
                {
                    var appTypes = mergedAssembly.GetTypes()
                        .Where(t => !t.IsInterface && !t.IsAbstract && iAppType.IsAssignableFrom(t))
                        .ToList();

                    Console.WriteLine("Found Application implementations:");
                    foreach (var appType in appTypes) Console.WriteLine($"  {appType.FullName}");

                    if (appTypes.Any())
                    {
                        var concreteAppType = appTypes.First();
                        Console.WriteLine($"\nUsing: {concreteAppType.FullName}");

                        // Get the bootstrapper type
                        var bootstrapperType = mergedAssembly.GetType("JetBrains.ReSharper.TestRunner.Bootstrapper`1");
                        if (bootstrapperType != null)
                        {
                            Console.WriteLine("Found Bootstrapper type");

                            // Create concrete bootstrapper type
                            var concreteBootstrapperType = bootstrapperType.MakeGenericType(concreteAppType);
                            Console.WriteLine($"Created concrete type: {concreteBootstrapperType.FullName}");

                            // Get the TryAttachDebugger method
                            var attachMethod = concreteBootstrapperType.GetMethod("TryAttachDebugger",
                                BindingFlags.NonPublic | BindingFlags.Instance);

                            if (attachMethod != null)
                            {
                                Console.WriteLine("Found TryAttachDebugger method");

                                try
                                {
                                    // Look for constructor parameters
                                    var ctors = concreteBootstrapperType.GetConstructors();
                                    Console.WriteLine("\nAvailable constructors:");
                                    foreach (var ctor in ctors)
                                    {
                                        Console.WriteLine("Constructor parameters:");
                                        foreach (var param in ctor.GetParameters()) Console.WriteLine($"  {param.Name}: {param.ParameterType}");
                                    }

                                    // Try to create instance
                                    var instance = Activator.CreateInstance(concreteBootstrapperType);
                                    if (instance != null)
                                    {
                                        Console.WriteLine("\nCreated bootstrapper instance");
                                        attachMethod.Invoke(instance, null);
                                        Console.WriteLine("Invoked TryAttachDebugger");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error creating/using bootstrapper: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to use bootstrapper: {ex}");
        }
    }
}
