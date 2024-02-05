using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using QuikGraph;
using StaticModeKey = System.String;

namespace DynAbs
{
    public class SingleObjectSolver : IAliasingSolver
    {
        UserSliceConfiguration Configuration;
        PtgVertex SingleVertex;
        
        public ScopeContainer GlobalScope = new ScopeContainer(true);
        public Stack<ScopeContainer> ScopeStack = new Stack<ScopeContainer>();
        public static readonly Field SIGMA_FIELD = Field.SigmaField();

        public bool StaticMode => false;

        public SingleObjectSolver(UserSliceConfiguration userConfiguration)
        {
            Configuration = userConfiguration;

            SingleVertex = new PtgVertex(Configuration, "Single", false, ISlicerSymbol.CreateObjectSymbol(), false, VertexType.Hub, PtgVertex.DefaultKind, false, 0, "");
            SingleVertex.AddVertex(EdgeType.Sigma, SingleVertex);
            SingleVertex.FieldsLastDef[Field.SIGMA_FIELD] = new HashSet<uint>() { };
        }

        public void EnterMethodAndBind(string methodSymbol, List<Term> actualParams, List<Term> formalParams, Term receiver, Term @this)
        {
            ScopeContainer previousScope = null;
            if (ScopeStack.Count > 0)
                previousScope = ScopeStack.Peek();

            var currentScope = new ScopeContainer();
            ScopeStack.Push(currentScope);
            currentScope.Arguments = actualParams;
            currentScope.Parameters = formalParams;

            for (var i = 0; i < formalParams.Count; i++)
                if (actualParams.Count > i && (actualParams[i] != null) && (!actualParams[i].IsScalar) && (!formalParams[i].IsScalar))
                {
                    var vertices = aPt(actualParams[i].IsGlobal ? GlobalScope : previousScope, actualParams[i]);
                    // LAFHIS TODO
                    //vertices ??= SingleVertex;
                    Add(formalParams[i], vertices);
                }

            if (receiver != null)
            {
                var thisNodes = aPt(receiver.IsGlobal ? GlobalScope : previousScope, receiver);
                if (thisNodes == null)
                    ;
                Add(@this, thisNodes);
            }
        }

        public ISet<uint> ExitMethodAndUnbind(Term term, Term returnExceptionTerm, Term exceptionTerm)
        {
            var lastScope = ScopeStack.Pop();
            lastScope.RemoveEntryPointsToYou();
            lastScope.Alive = false;

            for (var i = 0; i < lastScope.Arguments.Count; i++)
            {
                var parameter = lastScope.Parameters[i];
                var argument = lastScope.Arguments[i].ReferencedTerm; // Usamos el original
                if (parameter.IsOutOrRef)
                {
                    Assign(argument, ScopeForTerm(argument), parameter, lastScope);
                    LastDef_Set(argument, LastDef_Get(parameter, lastScope));
                }
            }

            ISet<uint> returnValue = null;
            if (term != null && lastScope.ReturnValue != null)
            {
                var returnValueScope = lastScope.ReturnValue.IsGlobal ? GlobalScope : lastScope;
                if (!term.IsScalar && !lastScope.ReturnValue.IsScalar)
                    Assign(term, ScopeForTerm(term), lastScope.ReturnValue, returnValueScope);
                returnValue = LastDef_Get(lastScope.ReturnValue, returnValueScope);
            }

            // TODO: exception term

            return returnValue;
        }

        public void Assign(Term lhsTerm, Term rhsTerm)
        {
            var leftScope = ScopeForTerm(lhsTerm);
            var rightScope = ScopeForTerm(rhsTerm);
            Assign(lhsTerm, leftScope, rhsTerm, rightScope);
        }

        public void Assign(Term lhsTerm, ScopeContainer lhsScope, Term rhsTerm, ScopeContainer rhsScope)
        {
            var currentVertex = aPt(rhsScope, rhsTerm);
            Assign(lhsTerm, lhsScope, currentVertex);
        }

        public void Assign(Term lhsTerm, ScopeContainer lhsScope, PtgVertex rhsVertex)
        {
            if (rhsVertex == null)
                return;

            if (lhsTerm.IsVar)
            {
                var set = SolverUtils.CreateReferenceComparedPTGHashSet();
                set.Add(rhsVertex);
                lhsScope.OverrideEntryPointValues(lhsTerm.First.ToString(), set, lhsTerm.Last.Symbol);
                return;
            }

            var lhsVertex = aPt(lhsScope, lhsTerm.DiscardLast());
            lhsVertex.AddVertex(EdgeType.Sigma, rhsVertex);
        }

        public void AssignRV(Term term)
        {
            ScopeStack.Peek().ReturnValue = term;
        }

        public ISet<uint> LastDef_Get(Term term)
        {
            return LastDef_Get(term, ScopeForTerm(term));
        }

        public ISet<uint> LastDef_Get(Term term, ScopeContainer scope)
        {
            if (term.IsGlobal && term.IsVar && !scope.LastDefDict.ContainsKey(term.First))
                throw new UninitializedTerm(term);

            if (term.IsVar)
            {
                if (!scope.LastDefDict.ContainsKey(term.First) &&
                    !term.IsTemporal &&
                    term.Stmt.CSharpSyntaxNode.GetContainer() is 
                    Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax)
                    return LastDef_Get(term, ScopeStack.ToList()[ScopeStack.ToList().IndexOf(scope) + 1]);

                if (!scope.LastDefDict.ContainsKey(term.First))
                    ;

                return new HashSet<uint>(scope.LastDefDict[term.First].IntSet);
            }

            var vertex = aPt(scope, term.DiscardLast());

            if (vertex == null && term.IsGlobal)
                throw new UninitializedTerm(term.DiscardLast());

            if (vertex == null)
            {
                // TODO LAFHIS
                return SingleVertex.FieldsLastDef[Field.SIGMA_FIELD];
            }

            return vertex.FieldsLastDef[SIGMA_FIELD];
        }

        public void LastDef_Set(Term term, uint lastDef)
        {
            LastDef_Set(term, new HashSet<uint>() { lastDef });
        }

        public void LastDef_Set(Term term, ISet<uint> lastDef, bool weak = false)
        {
            var scope = ScopeForTerm(term);
            if (term.IsVar)
            {
                if (scope.LastDefDict.ContainsKey(term.First))
                {
                    if (weak)
                        lastDef.UnionWith(scope.LastDefDict[term.First].IntSet);
                    scope.LastDefDict[term.First].IntSet = lastDef;
                }
                else
                    scope.LastDefDict[term.First] = new IntSetWithData(lastDef, 0, term.IsTemporal);
                return;
            }
            
            var vertex = aPt(scope, term.DiscardLast());
            if (vertex == null && term.IsGlobal)
                throw new UninitializedTerm(term.DiscardLast());

            if (vertex == null)
                ;

            vertex.FieldsLastDef[SIGMA_FIELD].UnionWith(lastDef);
        }

        public void Havoc(Term receiver, List<Term> arguments, Term returnValue, Func<Stmt, ISet<uint>, uint> GetDGNode, AnnotationWithData ad, Stmt invocationPoint)
        {
            var allArgs = new List<Term>();
            if (receiver != null)
                allArgs.Add(receiver);
            allArgs.AddRange(arguments);

            var uses = new HashSet<uint>();
            foreach (var p in allArgs)
                uses.UnionWith(LastDef_Get(p));

            PtgVertex node = null;
            foreach (var a in allArgs)
            {
                node = aPt(ScopeForTerm(a), a);
                if (node != null)
                    break;
            }

            if (node != null)
                uses.UnionWith(node.FieldsLastDef[SIGMA_FIELD]);

            var lastDef = GetDGNode(invocationPoint, uses);

            foreach (var p in allArgs.Where(x => x.ReferencedTerm != null && x.ReferencedTerm.IsOutOrRef))
            { 
                LastDef_Set(p.ReferencedTerm, lastDef);
                if ((!p.IsScalar) || (!p.ReferencedTerm.IsScalar))
                    Assign(p.ReferencedTerm, ScopeForTerm(p.ReferencedTerm), SingleVertex);
            }

            if (node != null)
                node.FieldsLastDef[SIGMA_FIELD] = new HashSet<uint>() { lastDef };

            if (returnValue != null)
            {
                LastDef_Set(returnValue, lastDef);
                if (!returnValue.IsScalar)
                    Assign(returnValue, ScopeForTerm(returnValue), SingleVertex);
            }
        }

        public void Alloc(Term term, bool @override = true, string kind = null) 
        {
            var scope = ScopeForTerm(term);
            if (term.IsVar)
            {
                if (@override || (!scope.EntryPointVerticesDict.ContainsKey(term.Last.ToString())))
                    scope.OverrideEntryPointValue(term.Last.ToString(), SingleVertex, term.Last.Symbol);
                return;
            }

            var lhsVertex = aPt(scope, term.DiscardLast());

            if (lhsVertex == null && term.IsGlobal)
                throw new UninitializedTerm(term.DiscardLast());
        }

        public void CleanTemporaryEntries() { }

        public bool Converged() => false;

        public void DumpPTG(string path, string label = null, bool globalScope = false, string key = null) { }

        public void EnterLoop() { }

        public bool EnterStaticMode(bool EnterByCallbacks = false) => false;

        public void ExitLoop() { }

        public void ExitStaticMode() { }

        public void RedefineType(Term term) { }

        public void NextLoopIteration() { }

        public void SaveResults(string graphEvolutionFile, string internalProfileFile) { }

        public PtgVertex aPt(ScopeContainer scope, Term term)
        {
            if (term.IsGlobal && term.IsVar && !scope.LastDefDict.ContainsKey(term.First))
                throw new UninitializedTerm(term);
            
            if (!scope.EntryPointVerticesDict.ContainsKey(term.First.ToString()))
                return null;

            return scope.EntryPointVerticesDict[term.First.ToString()].VertexSet.SingleOrDefault();
        }

        public void Add(Term term, PtgVertex destVertex)
        {
            var scope = ScopeForTerm(term);
            if (destVertex == null && term.IsGlobal)
                throw new UninitializedTerm(term);

            var currentSet = SolverUtils.CreateReferenceComparedPTGHashSet();
            if (destVertex != null)
                currentSet.Add(destVertex);

            if (term.IsVar)
            {
                scope.OverrideEntryPointValues(term.First.ToString(), currentSet, term.Last.Symbol);
                return;
            }

            throw new SlicerException("No se debería llegar hasta acá");
        }

        public ScopeContainer ScopeForTerm(Term term)
        {
            return term.IsGlobal ? GlobalScope : ScopeStack.Peek();
        }
    }
}