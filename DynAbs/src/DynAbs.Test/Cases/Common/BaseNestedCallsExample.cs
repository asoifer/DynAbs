using System;
using System.Collections.Generic;

namespace DynAbs.Test.Cases.Common
{
    class BaseNestedCallsExample
    {
        static void Main()
        {
            var a = TypeResolutionState.UsingSystem;
            Console.WriteLine(a.namespaces);
            return;
        }

        internal class TypeResolutionState
        {
            internal static readonly string[] systemNamespace = { "System" };
            internal static readonly TypeResolutionState UsingSystem = new TypeResolutionState();
            internal readonly string[] namespaces;
            private readonly HashSet<string> _typesDefined;

            private TypeResolutionState() : this(systemNamespace)
            {

            }

            internal TypeResolutionState(string[] namespaces)
            {
                this.namespaces = namespaces ?? systemNamespace;
                _typesDefined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
