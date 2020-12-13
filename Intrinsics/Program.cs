using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Intrinsics
{
    internal class Program
    {
        private static string GetParameter(ParameterInfo parameterInfo)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(parameterInfo.ParameterType.Name.Split('`')[0]);

            if (parameterInfo.ParameterType.IsGenericType)
            {
                stringBuilder.Append("<");

                stringBuilder.Append(GetParameterName(parameterInfo.ParameterType.GenericTypeArguments));

                stringBuilder.Append(">");
            }
            return stringBuilder.ToString();

            static string GetParameterName(Type[] genericTypeArguments)
            {
                var names = genericTypeArguments
                    .Select(x => x.Name)
                    .ToArray();

                return string.Join(", ", names);
            }
        }

        private static IEnumerable<(string FullName, bool IsSupported, IEnumerable<(string Name, IEnumerable<string> Parameters, string ReturnParameter)> Methods)> GetSupportedIntrinsics(IEnumerable<Assembly> allAssemblies)
        {
            return allAssemblies
                .SelectMany(GetSupportedIntrinsics);
        }

        private static IEnumerable<(string FullName, bool IsSupported, IEnumerable<(string Name, IEnumerable<string> Parameters, string ReturnParameter)> Methods)> GetSupportedIntrinsics(Assembly assembly)
        {
            return assembly.DefinedTypes
                .SelectMany(GetSupportedIntrinsics);
        }

        private static IEnumerable<(string FullName, bool IsSupported, IEnumerable<(string Name, IEnumerable<string> Parameters, string ReturnParameter)> Methods)> GetSupportedIntrinsics(TypeInfo definedType)
        {
            if (definedType.FullName.StartsWith("System.Runtime.Intrinsics", StringComparison.Ordinal))
            {
                var isSupportedPropertyInfo = definedType.GetProperty("IsSupported");
                if (isSupportedPropertyInfo != null)
                {
                    var supported = (bool)isSupportedPropertyInfo.GetValue(null);
                    var methods = definedType
                        .GetMethods()
                        .Where(x => !x.IsSpecialName)
                        .Where(x => x.IsStatic)
                        .Select(x => (x.Name, x.GetParameters().Select(GetParameter), GetParameter(x.ReturnParameter)));

                    return new[] { (definedType.FullName, supported, methods) };
                }
            }

            return Enumerable.Empty<(string FullName, bool IsSupported, IEnumerable<(string Name, IEnumerable<string> Parameters, string ReturnParameter)> Methods)>();
        }

        private static void Main(string[] args)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var supportedIntrinsic in GetSupportedIntrinsics(assemblies))
            {
                var supported = supportedIntrinsic.IsSupported ? "Supported" : "Not Supported";
                Console.WriteLine($"{supportedIntrinsic.FullName} - {supported }");
                foreach (var method in supportedIntrinsic.Methods)
                {
                    var parameters = string.Join(", ", method.Parameters);

                    Console.WriteLine($"\t{method.Name} ({parameters}) {method.ReturnParameter}");
                }
            }
        }
    }
}