using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class ISlicerSymbol
    {
        public ITypeSymbol Symbol { get; internal set; }
        public bool IsNullSymbol { get; internal set; }
        public bool IsAnonymous { get; internal set; }
        public bool IsObject { get; internal set; }
        public bool IsMethod { get; internal set; }
        public bool IsScalar { get { return (!IsObject) && (!IsAnonymous) && (Symbol == null || !Symbol.IsNotScalar()); } }
        public bool IsStruct { get { return Symbol != null && Symbol.CustomIsStruct(); } }
        
        static ISlicerSymbol NullSlicerSymbol { get; set; }
        static ISlicerSymbol AnonymousSlicerSymbol { get; set; }
        static ISlicerSymbol LambdaSlicerSymbol { get; set; }
        static ISlicerSymbol ObjectSlicerSymbol { get; set; }

        public static ISlicerSymbol Create(ITypeSymbol symbol, IDictionary<ITypeSymbol, ITypeSymbol> typesDictionary = null)
        {
            if (typesDictionary != null && symbol != null && (symbol.Kind == SymbolKind.TypeParameter))
            { 
                var tempSymbol = typesDictionary.Where(x => x.Key.Name == symbol.Name).FirstOrDefault().Value;
                if (tempSymbol != null)
                    symbol = tempSymbol;
            }
            // TODO: XXX: IsMethod depends on which symbol we are talking about
            return new ISlicerSymbol() { Symbol = symbol, IsNullSymbol = false, IsAnonymous = false, IsObject = false, IsMethod = false };
        }
        public static ISlicerSymbol CreateNullTypeSymbol()
        {
            if (NullSlicerSymbol == null)
                NullSlicerSymbol = new ISlicerSymbol() { Symbol = null, IsNullSymbol = true, IsAnonymous = false, IsObject = false, IsMethod = false };

            return NullSlicerSymbol;
        }
        public static ISlicerSymbol CreateAnonymousSymbol()
        {
            if (AnonymousSlicerSymbol == null)
                AnonymousSlicerSymbol = new ISlicerSymbol() { Symbol = null, IsNullSymbol = false, IsAnonymous = true, IsObject = false, IsMethod = false };
            
            return AnonymousSlicerSymbol;
        }
        public static ISlicerSymbol CreateLambdaSlicerSymbol()
        {
            if (LambdaSlicerSymbol == null)
                LambdaSlicerSymbol = new ISlicerSymbol() { Symbol = null, IsNullSymbol = false, IsAnonymous = false, IsObject = false, IsMethod = true };

            return LambdaSlicerSymbol;
        }
        public static ISlicerSymbol CreateObjectSymbol()
        {
            if (ObjectSlicerSymbol == null)
                ObjectSlicerSymbol = new ISlicerSymbol() { Symbol = null, IsNullSymbol = false, IsAnonymous = false, IsObject = true, IsMethod = false };

            return ObjectSlicerSymbol;
        }
        
        public static bool operator ==(ISlicerSymbol a, ISlicerSymbol b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (System.Object.ReferenceEquals(a, null) && System.Object.ReferenceEquals(b, null))
                return true;
            if (System.Object.ReferenceEquals(a, null) || System.Object.ReferenceEquals(b, null))
                return false;

            // Los 2 casos null no puede pasar porque la referencia tiene que ser la misma (utilizamos singletones, y me refiero al 1er IF)
            if (a.Symbol == null || b.Symbol == null)
                return false;
            
            // Si las referencias no son iguales es porque son typeSymbol
            if (SymbolEqualityComparer.Default.Equals(a.Symbol, b.Symbol))
                return true;

            // En muchos casos la comparación de tipos da false cuando no lo es
            // En esos casos la comparación de strings es la posta porque te encadena los tipos bien sino es un lío... (lamentablemente)
            return a.Symbol.ToString() == b.Symbol.ToString();
        }

        public static bool operator !=(ISlicerSymbol a, ISlicerSymbol b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return (ISlicerSymbol)obj == this;
        }

        public override int GetHashCode()
        {
            if (this.IsNullSymbol)
                return 1;
            if (this.IsAnonymous)
                return 2;
            if (this.IsObject)
                return 3;
            if (this.IsMethod)
                return 4;

            return Symbol.GetHashCode();
        }

        public override string ToString()
        {
            if (IsNullSymbol)
                return "NULL";
            if (IsObject)
                return "OBJECT";
            if (IsAnonymous)
                return "ANONYMOUS";
            return string.IsNullOrWhiteSpace(Symbol.Name) ? Symbol.ToString() : Symbol.Name.Split('.').Last();
        }
    }
}
