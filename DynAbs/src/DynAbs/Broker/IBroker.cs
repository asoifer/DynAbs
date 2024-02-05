using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    public interface IBroker
    {
        // For debugging
        IAliasingSolver Solver { get; }
        IDependencyGraph DependencyGraph { get; }

        bool SliceCriteriaReached { get; set; }

        void Break();

        void Continue();

        void EnterCondition(Stmt stmt);

        void ExitCondition(Stmt stmt);

        void EnterMethod(string methodSymbol, List<Term> argumentTermList, List<Term> parameterTermList, Term receiver, Term @this, Stmt invocationPoint, Stmt enterMethodStatement);

        void ExitMethod(Stmt exitMethodStatement, Term returnValue, Term returnExceptionTerm, Term exceptionTerm);

        void Alloc(Term term);

        void DefExternalOperation(Term defTerm);

        void DefUseOperation(ISet<Term> defTerms, ISet<Term> useTerms);

        void DefUseOperation(Term defTerm, Term[] useTerms);

        void DefUseOperation(Term defTerm);

        void UseOperation(Stmt stmt, List<Term> useTerms);

        void Assign(Term defTerm, Term useTerm);

        void Assign(Term defTerm, Term useTerm, List<Term> anotherUses);

        void RedefineType(Term term);

        void AssignRV(Term returnValue);

        void HandleNonInstrumentedMethod(List<Term> argumentTermList, Term @this, List<Term> returnedValues, Term returnValue, ISymbol symbol, string methodName = null);

        void HandleArrayInitialization(List<Term> argumentTermList, List<Term> returnedValues, Term returnValue);

        void CreateNonInstrumentedRegion(List<Term> involvedTerms, Term returnValue);

        void CatchReturnedValueIntoRegion(Term region, Term returnedValue);

        void CustomEvent(List<Term> argumentTermList, Term @this, List<Term> returnedValues, Term returnValue, string EventName);

        bool EnterStaticMode(bool EnterByCallbacks = false);

        void ExitStaticMode();

        bool StaticMode { get; }

        void Slice(ResultSummaryData Data);

        List<ISet<Stmt>> GetSlices();

        void EnterLoop();
        void NextLoopIteration();
        void ExitLoop();

        List<ResultSummaryData> SlicesSummaryData { get; }

        void PrintCallGraph(string path);
        void LogCallback(ISymbol callback, ISymbol symbol, string methodName = null);
        void SaveCallsInformation(string methodsFile, string callbacksFile);
    }
}
