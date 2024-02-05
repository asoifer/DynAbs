using System;
using System.Collections.Generic;

namespace DynAbs
{
    public interface IAliasingSolver
    {
        ISet<uint> LastDef_Get(Term term);

        void LastDef_Set(Term term, uint lastDef);

        void Alloc(Term term, bool @override = true, string kind = null);

        void Assign(Term term1, Term term2);

        void RedefineType(Term term);

        void AssignRV(Term term);

        void EnterMethodAndBind(string methodSymbol, List<Term> actualParams, List<Term> formalParams, Term receiver, Term @this);

        ISet<uint> ExitMethodAndUnbind(Term term, Term returnExceptionTerm, Term exceptionTerm);

        void Havoc(Term receiver, List<Term> arguments, Term returnValue, Func<Stmt, ISet<uint>, uint> GetDGNode, AnnotationWithData ad, Stmt invocationPoint);

        bool EnterStaticMode(bool EnterByCallbacks = false);

        void ExitStaticMode();
        
        void EnterLoop();

        void NextLoopIteration();

        void ExitLoop();

        bool Converged();

        void CleanTemporaryEntries();

        bool StaticMode { get; }

        void DumpPTG(string path, string label = null, bool globalScope = false, string key = null);

        void SaveResults(string graphEvolutionFile, string internalProfileFile);
    }
}