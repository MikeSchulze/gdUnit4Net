namespace NUnit.Extension.GdUnit4;

using System.Reflection;

public class DebuggerUtils
{
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
