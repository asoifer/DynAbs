using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs
{
    class TermInitializerSolver : IAliasingSolver
    {
        public IBroker Broker { get; set; }
        IAliasingSolver AliasingSolver;

        public TermInitializerSolver(IAliasingSolver aliasingSolver)
        {
            AliasingSolver = aliasingSolver;
        }

        public bool StaticMode => AliasingSolver.StaticMode;

        public void Alloc(Term term, bool @override = true, string kind = null)
        {
            try
            {
                AliasingSolver.Alloc(term, @override, kind);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                AliasingSolver.Alloc(term, @override, kind);
            }
        }

        public void Assign(Term term1, Term term2)
        {
            try
            {
                AliasingSolver.Assign(term1, term2);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                AliasingSolver.Assign(term1, term2);
            }
        }

        public void AssignRV(Term term)
        {
            AliasingSolver.AssignRV(term);
        }

        public void CleanTemporaryEntries()
        {
            AliasingSolver.CleanTemporaryEntries();
        }

        public bool Converged()
        {
            return AliasingSolver.Converged();
        }

        public void DumpPTG(string path, string label = null, bool globalScope = false, string key = null)
        {
            AliasingSolver.DumpPTG(path, label, globalScope, key);
        }

        public void EnterLoop()
        {
            AliasingSolver.EnterLoop();
        }

        public void EnterMethodAndBind(string methodSymbol, List<Term> actualParams, List<Term> formalParams, Term receiver, Term @this)
        {
            try
            {
                AliasingSolver.EnterMethodAndBind(methodSymbol, actualParams, formalParams, receiver, @this);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                AliasingSolver.EnterMethodAndBind(methodSymbol, actualParams, formalParams, receiver, @this);
            }
        }

        public bool EnterStaticMode(bool EnterByCallbacks = false)
        {
            return AliasingSolver.EnterStaticMode(EnterByCallbacks);
        }

        public void ExitLoop()
        {
            AliasingSolver.ExitLoop();
        }

        public ISet<uint> ExitMethodAndUnbind(Term term, Term returnExceptionTerm, Term exceptionTerm)
        {
            try
            {
                return AliasingSolver.ExitMethodAndUnbind(term, returnExceptionTerm, exceptionTerm);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                return AliasingSolver.ExitMethodAndUnbind(term, returnExceptionTerm, exceptionTerm);
            }
        }

        public void ExitStaticMode()
        {
            AliasingSolver.ExitStaticMode();
        }

        public void Havoc(Term receiver, List<Term> arguments, Term returnValue, Func<Stmt, ISet<uint>, uint> GetDGNode, AnnotationWithData ad, Stmt invocationPoint)
        {
            try
            {
                AliasingSolver.Havoc(receiver, arguments, returnValue, GetDGNode, ad, invocationPoint);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                AliasingSolver.Havoc(receiver, arguments, returnValue, GetDGNode, ad, invocationPoint);
            }
        }

        public ISet<uint> LastDef_Get(Term term)
        {
            try
            {
                return AliasingSolver.LastDef_Get(term);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                return AliasingSolver.LastDef_Get(term);
            }
        }

        public void LastDef_Set(Term term, uint lastDef)
        {
            try
            {
                AliasingSolver.LastDef_Set(term, lastDef);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                AliasingSolver.LastDef_Set(term, lastDef);
            }
        }

        public void NextLoopIteration()
        {
            AliasingSolver.NextLoopIteration();
        }

        public void RedefineType(Term term)
        {
            AliasingSolver.RedefineType(term);
        }

        public void SaveResults(string graphEvolutionFile, string internalProfileFile)
        {
            AliasingSolver.SaveResults(graphEvolutionFile, internalProfileFile);
        }

        public void InitializeTerm(Term term)
        {
            if (!term.IsGlobal)
            {
                if (term.Count > 1)
                {
                    Broker.DefUseOperation(term, new Term[] { term.DiscardLast() });
                    return;
                }
                throw new SlicerException("Término no global");
            }

            //#if DEBUG
            //Utils.Print("InitializeTerm: " + term.ToString());
            //#endif

            if (!term.IsScalar)
                Alloc(term);
            Broker.DefExternalOperation(term);
        }
    }
}
