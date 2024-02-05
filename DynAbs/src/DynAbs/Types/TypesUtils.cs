using DynAbs.Summaries;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public static class TypesUtils
    {
        static IDictionary<ISlicerSymbol, IDictionary<ISlicerSymbol, bool>> complexTypesCache = new Dictionary<ISlicerSymbol, IDictionary<ISlicerSymbol, bool>>();
        static IDictionary<string, ISlicerSymbol> associatedSymbol = new Dictionary<string, ISlicerSymbol>();

        public static bool Compatibles(ISlicerSymbol a, ISlicerSymbol b)
        {
            var initialTime = System.DateTime.Now;

            IDictionary<ISlicerSymbol, bool> firstEntry;
            if (complexTypesCache.TryGetValue(a, out firstEntry))
            {
                bool secondEntry;
                if (firstEntry.TryGetValue(b, out secondEntry))
                {
                    Globals.TypesHitsCache++;
                    return secondEntry;
                }
            }
            Globals.TypesMissCache++;

            bool returnValue;
            if (a == b)
                returnValue = true;
            // A and B are distinct. To an anonymous or null you cannot assign another type.
            else if (a.IsNullSymbol || a.IsAnonymous)
                returnValue = false;
            else if (b.IsAnonymous && a.Symbol != null && a.Symbol.IsAnonymousType)
                returnValue = true;
            else if (a.IsObject)
                returnValue = !(b.IsNullSymbol);
            else if (b.Symbol == null)
                returnValue = false;
            else
                returnValue = Compatibles(a.Symbol, b.Symbol);

            if (firstEntry == null)
                complexTypesCache[a] = new Dictionary<ISlicerSymbol, bool>();
            complexTypesCache[a][b] = returnValue;

            Globals.TypesTimeCompatibles += System.DateTime.Now.Subtract(initialTime).TotalMilliseconds;

            return returnValue;
        }

        public static bool Compatibles(ITypeSymbol type1, ITypeSymbol type2)
        {
            bool returnValue = false;

            // If it does not have a base type is because it is an object, and if the left type is an object, everything is compatible
            if ((type1.BaseType == null && type1.TypeKind == TypeKind.Class) || 
                type1.TypeKind == TypeKind.TypeParameter ||
                // If right type is T, we assume compatibility (soundness)
                //(type1.TypeKind == TypeKind.Interface && type2.AllInterfaces.Contains(type1))) 
                (type1.TypeKind == TypeKind.Interface && type2.AllInterfaces.Any(x => x.ToString() == type1.ToString())))
                returnValue = true;
            else
            {
                ITypeSymbol baseType = type2;
                while (baseType != null)
                {
                    // If the class has the same name or rhs type implements the iterface of the right...
                    if (type1.Name == baseType.Name)
                    {
                        returnValue = true;
                        break;
                    }
                    baseType = baseType.BaseType;
                }

                // If they are lists... we look inside the container type
                if (returnValue && (type1 is INamedTypeSymbol && baseType is INamedTypeSymbol))
                {
                    var aTypeArguments = ((INamedTypeSymbol)type1).TypeArguments.ToList();
                    var bTypeArguments = ((INamedTypeSymbol)baseType).TypeArguments.ToList();

                    // Note: There was an exception with EventHandler<Error> = EventHandler...
                    // Left type has parameters, is more specific, then this is not compatible with the right type
                    if (aTypeArguments.Count > bTypeArguments.Count)
                        return false;

                    if (aTypeArguments.Count > 0) // && aTypeParameters.Count == bTypeParameters.Count)
                        for (var i = 0; i < aTypeArguments.Count; i++)
                            if (!Compatibles(aTypeArguments[i], bTypeArguments[i]))
                            {
                                returnValue = false;
                                break;
                            }
                }
            }

            return returnValue;
        }

        public static ISlicerSymbol GetFieldSymbol(ISlicerSymbol symbol, string name)
        {
            var initialTime = System.DateTime.Now;

            ISlicerSymbol returnValue = null;
            if (symbol.Symbol == null)
            {
                if (symbol.IsNullSymbol)
                    throw new SlicerException(Exceptions.ErrorMessages.Types_GetFieldOnNullSymbol);
                returnValue = ISlicerSymbol.CreateObjectSymbol();
            }
            if (returnValue == null)
            {
                var lookupType = symbol.Symbol;
                ISymbol memberSymbol = null;
                while (lookupType != null)
                {
                    var members = lookupType.OriginalDefinition.GetMembers(name);
                    if (members.Count() > 0)
                    {
                        memberSymbol = members.First();
                        break;
                    }
                    else
                        lookupType = lookupType.BaseType;
                }
                if (memberSymbol == null)
                    // Note: This field does not belong to the type (weird)
                    returnValue = ISlicerSymbol.CreateObjectSymbol();
                else
                {
                    if (memberSymbol.Kind == SymbolKind.Property)
                        returnValue = ISlicerSymbol.Create(((IPropertySymbol)memberSymbol).Type);
                    else if (memberSymbol.Kind == SymbolKind.Field)
                        returnValue = ISlicerSymbol.Create(((IFieldSymbol)memberSymbol).Type);

                    else //if (memberSymbol.Kind == SymbolKind.Method) (or what..?)
                        returnValue = ISlicerSymbol.CreateObjectSymbol();
                }
            }

            Globals.TypesTimeGetFieldSymbol += System.DateTime.Now.Subtract(initialTime).TotalMilliseconds;
            return returnValue;
        }

        public static ISlicerSymbol GetMin(ISlicerSymbol a, ISlicerSymbol b)
        {
            var initialTime = System.DateTime.Now;
            ISlicerSymbol returnValue = null;

            if (a == b)
                returnValue = a;
            // Null, IsAnonymous y Query are the most specific, we have an own representation of them.
            else if (a.IsNullSymbol || a.IsAnonymous)
                returnValue = a;
            else if (b.IsNullSymbol || b.IsAnonymous)
                returnValue = b;
            else if (a.IsObject)
                returnValue = b;
            else if (b.IsObject)
                returnValue = a;
            // If the base type is null, this is an object
            else if (a.Symbol.BaseType == null)
                returnValue = b;
            else if (b.Symbol.BaseType == null)
                returnValue = a;
            else if (a.Symbol.TypeKind == TypeKind.Interface && b.Symbol.TypeKind == TypeKind.Class)
                returnValue = b;
            else if (b.Symbol.TypeKind == TypeKind.Interface && a.Symbol.TypeKind == TypeKind.Class)
                returnValue = a;
            // The most general is not the minor. If you can assign one type to another is because is more general.
            else if (!Compatibles(a, b))
                return a;
            else if (!Compatibles(b, a))
                return b;
            else
            {
                // a and b are the same...
                returnValue = b;
            }

            Globals.TypesTimeGetMin += System.DateTime.Now.Subtract(initialTime).TotalMilliseconds;
            return returnValue;
        }

        public static ISlicerSymbol GetTypeByETType(ETType type, IDictionary<ETType, ISlicerSymbol> mapping)
        {
            if (type.Kind == ETTypeKind.Default)
                return GetNamedTypeByName(type.Name);
            return mapping.Where(x => x.Key.Equals(type)).Single().Value;
        }
        
        public static ISlicerSymbol GetNamedTypeByName(string name)
        {
            if (associatedSymbol.ContainsKey(name))
                return associatedSymbol[name];

            var returnedSymbol = ISlicerSymbol.CreateNullTypeSymbol();
            var symbols = new HashSet<INamedTypeSymbol>();
            foreach (var project in Globals.UserSolution.Projects)
            {
                var results = Microsoft.CodeAnalysis.FindSymbols.SymbolFinder.FindDeclarationsAsync(project, name.Split('.').Last(), true).Result;
                symbols.UnionWith(results.OfType<INamedTypeSymbol>());
            }

            if (symbols.Count > 1)
            {
                if (name.Contains('.'))
                    symbols.RemoveWhere(x => x.ContainingNamespace.ToString() != name.Substring(0, name.LastIndexOf('.')));
                
                if (symbols.Count != 1)
                {
                    // Ambiguous type name in annotation... (Weird...)
                    symbols = new HashSet<INamedTypeSymbol>() { symbols.First() };
                }
            }
            
            if (symbols.Count > 0)
                returnedSymbol = ISlicerSymbol.Create(symbols.Single());
            associatedSymbol[name] = returnedSymbol;
            return returnedSymbol;
        }
    }
}
