using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Intrinsics
{
    internal class Program
    {
        private static IEnumerable<(string FullName, bool IsSupported, IEnumerable<string> Methods)> GetSupportedIntrinsics(IEnumerable<Assembly> allAssemblies)
        {
            return allAssemblies
                .SelectMany(GetSupportedIntrinsics);
        }

        private static IEnumerable<(string FullName, bool IsSupported, IEnumerable<string> Methods)> GetSupportedIntrinsics(Assembly assembly)
        {
            return assembly.DefinedTypes
                .SelectMany(GetSupportedIntrinsics);
        }

        private static IEnumerable<(string FullName, bool IsSupported, IEnumerable<string> Methods)> GetSupportedIntrinsics(TypeInfo definedType)
        {
            if (definedType.FullName.StartsWith("System.Runtime.Intrinsics", StringComparison.Ordinal))
            {
                var isSupportedPropertyInfo = definedType.GetProperty("IsSupported");
                if (isSupportedPropertyInfo != null)
                {
                    var supported = (bool)isSupportedPropertyInfo.GetValue(null);
                    var methods = definedType
                        .GetMethods()
                        .Select(x => x.Name);
                    return new[] { (definedType.FullName, supported, methods) };
                }
            }

            return Enumerable.Empty<(string FullName, bool IsSupported, IEnumerable<string> Methods)>();
        }

        private static void Main(string[] args)
        {
            var table = new ConsoleTable("FullName", "Supported", "Methods");

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var supportedIntrinsics = GetSupportedIntrinsics(assemblies).ToArray();

            foreach (var supportedIntrinsic in supportedIntrinsics)
            {
                var supported = supportedIntrinsic.IsSupported ? "Supported" : "Not Supported";
                Console.WriteLine($"{supportedIntrinsic.FullName} - {supported }");
                foreach (var method in supportedIntrinsic.Methods)
                {
                    Console.WriteLine($"\t{method}");
                }
            }
        }
    }
}