using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Intrinsics
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var table = new ConsoleTable("FullName", "Supported");

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            GetSupportedIntrinsics(assemblies)
                .ForEach(tuple => table.AddRow(tuple.Item1, tuple.Item2));

            table.Write();
            Console.WriteLine();
        }

        private static IEnumerable<Tuple<string, string>> GetSupportedIntrinsics(IEnumerable<Assembly> allAssemblies)
        {
            return allAssemblies
                .SelectMany(GetSupportedIntrinsics);
        }

        private static IEnumerable<Tuple<string, string>> GetSupportedIntrinsics(Assembly assembly)
        {
            return assembly.DefinedTypes
                .SelectMany(GetSupportedIntrinsics);
        }

        private static IEnumerable<Tuple<string, string>> GetSupportedIntrinsics(TypeInfo definedType)
        {
            if (definedType.FullName.StartsWith("System.Runtime.Intrinsics", StringComparison.Ordinal))
            {
                var isSupportedPropertyInfo = definedType.GetProperty("IsSupported");
                if (isSupportedPropertyInfo != null)
                {
                    var supported = (bool)isSupportedPropertyInfo.GetValue(null);
                    return new[] { Tuple.Create(definedType.FullName, supported ? "Yes" : "No") };
                }
            }

            return Enumerable.Empty<Tuple<string, string>>();
        }
    }
}