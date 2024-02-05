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
    class InvocationInstrumenterWalker : CSharpSyntaxWalker
    {
        SemanticModel Model { get; set; }
        public List<string> RuntimeNames = new List<string>();

        public InvocationInstrumenterWalker(SemanticModel model)
        {
            Model = model;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            //COBERTURA: comentando este if, el test case NotSupportedLambda no compila
            //EXPLICACIONES: no se instrumentan los identificadores que ocurren
            //en una expresion lambda, ya que la variable usada en la expresion
            //no está en scope por fuera de la expresión, entonces no compila
            //OBS: como esto es un walker, no hace falta ni siquiera ir a la base
            //OBS2: en realidad alcanzaría con no instrumentar unicamente el
            //      identificador que representa la variable lambda
            //OBS3: por ahora habría que seguir agregando a mano posibles metodos
            //      de Linq. Habría que ver si hay alguna manera de saberlo genérica
            if (node.Expression is MemberAccessExpressionSyntax)
            {
                if (((MemberAccessExpressionSyntax)node.Expression).Name.ToString() == "Where"
                    || ((MemberAccessExpressionSyntax)node.Expression).Name.ToString() == "OrderBy"
                    || ((MemberAccessExpressionSyntax)node.Expression).Name.ToString() == "ThenBy"
                    || ((MemberAccessExpressionSyntax)node.Expression).Name.ToString() == "FirstOrDefault")
                    return;
            }

            base.VisitInvocationExpression(node);

            string obj = null;
            if (node.Expression is MemberAccessExpressionSyntax)
            {
                MemberAccessExpressionSyntax member = (MemberAccessExpressionSyntax)node.Expression;
                obj = member.Expression.ToString();
            }
            else if (node.Expression is IdentifierNameSyntax)
            //no se puede castear de Generic a Identifier
            //de todas maneras la variable asignada despues no se usa!
            //else if (node.Expression is IdentifierNameSyntax
            //    || node.Expression is GenericNameSyntax)
            {
                IdentifierNameSyntax identifier = (IdentifierNameSyntax)node.Expression;
                obj = "this";
            }
            else if (node.Expression is GenericNameSyntax)
            {
                obj = "this";
            }

            string runtimeName = null;
            SymbolInfo symbolInfo = Model.GetSymbolInfo(node.Expression);
            bool symbolIsInSource = symbolInfo.Symbol.Locations[0].IsInSource;
            if (symbolInfo.Symbol.IsStatic)
            {
                if (symbolIsInSource)
                {
                    runtimeName = "\"static-source\"";
                }
                else
                {
                    runtimeName = "\"static-metadata\"";
                }
            }
            else
            {
                if (obj == null) throw new InvalidOperationException("obj value must exist");

                runtimeName = obj + ".GetType().FullName";
            }
            RuntimeNames.Add(runtimeName);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            base.VisitObjectCreationExpression(node);

            string obj = node.Type.ToString();
            string runtimeName = "typeof(" + obj + ").FullName";
            RuntimeNames.Add(runtimeName);
        }
    }
}
