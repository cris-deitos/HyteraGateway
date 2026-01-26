using System.Reflection;

namespace DllAnalyzer;

/// <summary>
/// Simple tool to analyze DLL assemblies for reverse engineering
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== DLL Analyzer for HyteraGateway ===");
        Console.WriteLine();

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: DllAnalyzer <path-to-dll>");
            Console.WriteLine();
            Console.WriteLine("This tool analyzes .NET assemblies to help understand the Hytera protocol.");
            Console.WriteLine("It will list all types, methods, properties, and fields in the assembly.");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  DllAnalyzer.exe HyteraProtocol.dll");
            return;
        }

        string dllPath = args[0];

        if (!File.Exists(dllPath))
        {
            Console.WriteLine($"Error: File not found: {dllPath}");
            return;
        }

        try
        {
            Console.WriteLine($"Analyzing: {dllPath}");
            Console.WriteLine();

            Assembly assembly = Assembly.LoadFrom(dllPath);

            Console.WriteLine($"Assembly: {assembly.FullName}");
            Console.WriteLine();

            Type[] types = assembly.GetTypes();
            Console.WriteLine($"Found {types.Length} types:");
            Console.WriteLine();

            foreach (Type type in types.OrderBy(t => t.FullName))
            {
                Console.WriteLine($"Type: {type.FullName}");
                Console.WriteLine($"  Base Type: {type.BaseType?.FullName ?? "None"}");

                // List interfaces
                Type[] interfaces = type.GetInterfaces();
                if (interfaces.Length > 0)
                {
                    Console.WriteLine("  Interfaces:");
                    foreach (Type i in interfaces)
                    {
                        Console.WriteLine($"    - {i.FullName}");
                    }
                }

                // List methods
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (methods.Length > 0)
                {
                    Console.WriteLine("  Methods:");
                    foreach (MethodInfo method in methods)
                    {
                        string parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                        Console.WriteLine($"    - {method.ReturnType.Name} {method.Name}({parameters})");
                    }
                }

                // List properties
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (properties.Length > 0)
                {
                    Console.WriteLine("  Properties:");
                    foreach (PropertyInfo property in properties)
                    {
                        Console.WriteLine($"    - {property.PropertyType.Name} {property.Name}");
                    }
                }

                // List fields
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (fields.Length > 0)
                {
                    Console.WriteLine("  Fields:");
                    foreach (FieldInfo field in fields)
                    {
                        Console.WriteLine($"    - {field.FieldType.Name} {field.Name}");
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine($"Analysis complete. Found {types.Length} types.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing assembly: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
