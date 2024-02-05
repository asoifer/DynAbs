using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class TermVerificationBroker : IBroker
    {
        IBroker _broker;

        public TermVerificationBroker(IBroker broker)
        {
            _broker = new PerformanceTestBroker(broker);
        }

        public IAliasingSolver Solver
        {
            get { return _broker.Solver; }
        }

        public IDependencyGraph DependencyGraph
        {
            get { return _broker.DependencyGraph; }
        }

        public bool SliceCriteriaReached
        {
            get
            {
                return _broker.SliceCriteriaReached;
            }
            set
            {
                _broker.SliceCriteriaReached = value;
            }
        }

        public void Break()
        {
            _broker.Break();
        }

        public void Continue()
        {
            _broker.Continue();
        }

        public void EnterCondition(Stmt stmt)
        {
            _broker.EnterCondition(stmt);
        }

        public void ExitCondition(Stmt stmt)
        {
            _broker.ExitCondition(stmt);
        }

        public void EnterMethod(string methodSymbol, List<Term> argumentTermList, List<Term> parameterTermList, Term receiver, Term @this, Stmt invocationPoint, Stmt enterMethodStatement)
        {
            argumentTermList.ToList().ForEach(x => CheckTerm(x));
            parameterTermList.ToList().ForEach(x => CheckTerm(x));
            CheckTerm(@this);
            CheckTerm(receiver);
            _broker.EnterMethod(methodSymbol, argumentTermList, parameterTermList, receiver, @this, invocationPoint, enterMethodStatement);
        }

        public void ExitMethod(Stmt exitMethodStatement, Term returnValue, Term returnExceptionTerm, Term exceptionTerm)
        {
            CheckTerm(returnValue);
            _broker.ExitMethod(exitMethodStatement, returnValue, returnExceptionTerm, exceptionTerm);
        }

        public void Alloc(Term term)
        {
            CheckTerm(term);
            _broker.Alloc(term);
        }

        public void DefExternalOperation(Term defTerm)
        {
            throw new SlicerException("No puede llamarse desde afuera");
        }

        public void DefUseOperation(ISet<Term> defTerms, ISet<Term> useTerms)
        {
            defTerms.ToList().ForEach(x => CheckTerm(x));
            useTerms.ToList().ForEach(x => CheckTerm(x));
            _broker.DefUseOperation(defTerms, useTerms);
        }

        public void DefUseOperation(Term defTerm, Term[] useTerms)
        {
            CheckTerm(defTerm);
            useTerms.ToList().ForEach(x => CheckTerm(x));
            _broker.DefUseOperation(defTerm, useTerms);
        }

        public void DefUseOperation(Term defTerm)
        {
            CheckTerm(defTerm);
            _broker.DefUseOperation(defTerm);
        }

        public void UseOperation(Stmt stmt, List<Term> useTerms)
        {
            useTerms.ToList().ForEach(x => CheckTerm(x));
            _broker.UseOperation(stmt, useTerms);
        }

        public void Assign(Term defTerm, Term useTerm)
        {
            CheckTerm(defTerm);
            CheckTerm(useTerm);
            _broker.Assign(defTerm, useTerm);
        }

        public void Assign(Term defTerm, Term useTerm, List<Term> anotherUses)
        {
            CheckTerm(defTerm);
            CheckTerm(useTerm);
            if (anotherUses != null)
                foreach (var t in anotherUses)
                    CheckTerm(t);
            _broker.Assign(defTerm, useTerm, anotherUses);
        }

        public void RedefineType(Term term)
        {
            CheckTerm(term);
            _broker.RedefineType(term);
        }

        public void AssignRV(Term returnValue)
        {
            CheckTerm(returnValue);
            _broker.AssignRV(returnValue);
        }

        public void HandleNonInstrumentedMethod(List<Term> argumentTermList, Term @this, List<Term> returnedValues, Term returnValue, ISymbol symbol, string methodName = null)
        {
            argumentTermList.ForEach(x => CheckTerm(x));
            returnedValues.ForEach(x => CheckTerm(x));
            CheckTerm(@this);
            CheckTerm(returnValue);
            _broker.HandleNonInstrumentedMethod(argumentTermList, @this, returnedValues, returnValue, symbol, methodName);
        }

        public void CatchReturnedValueIntoRegion(Term region, Term returnedValue)
        {
            _broker.CatchReturnedValueIntoRegion(region, returnedValue);
        }

        public void HandleArrayInitialization(List<Term> argumentTermList, List<Term> returnedValues, Term returnValue)
        {
            argumentTermList.ForEach(x => CheckTerm(x));
            CheckTerm(returnValue);
            _broker.HandleArrayInitialization(argumentTermList, returnedValues, returnValue);
        }

        public void CreateNonInstrumentedRegion(List<Term> involvedTerms, Term returnValue)
        {
            involvedTerms.ForEach(x => CheckTerm(x));
            CheckTerm(returnValue);
            _broker.CreateNonInstrumentedRegion(involvedTerms, returnValue);
        }

        public void CustomEvent(List<Term> argumentTermList, Term @this, List<Term> returnedValues, Term returnValue, string EventName)
        {
            _broker.CustomEvent(argumentTermList, @this, returnedValues, returnValue, EventName);
        }

        public bool EnterStaticMode(bool EnterByCallbacks = false)
        {
            return _broker.EnterStaticMode(EnterByCallbacks);
        }

        public void ExitStaticMode()
        {
            _broker.ExitStaticMode();
        }

        public bool StaticMode { get { return _broker.StaticMode; } }

        public void Slice(ResultSummaryData data)
        {
            _broker.Slice(data);
        }

        public List<ISet<Stmt>> GetSlices()
        {
            return _broker.GetSlices();
        }

        public void EnterLoop()
        {
            _broker.EnterLoop();
        }

        public void NextLoopIteration()
        {
            _broker.NextLoopIteration();
        }

        public void ExitLoop()
        {
            _broker.ExitLoop();
        }

        void CheckTerm(Term term)
        {
            if (term != null && term.Parts.Any(x => x.Symbol == null || (!x.Symbol.IsAnonymous && !x.Symbol.IsNullSymbol && !x.Symbol.IsObject && x.Symbol.Symbol == null)))
                throw new SlicerException("Todos los fields tienen que tener símbolo");
        }

        public void PrintCallGraph(string path)
        {
            _broker.PrintCallGraph(path);
        }

        public void LogCallback(ISymbol callback, ISymbol symbol, string methodName = null)
        {
            _broker.LogCallback(callback, symbol, methodName);
        }

        public void SaveCallsInformation(string methodsFile, string callbacksFile)
        {
            _broker.SaveCallsInformation(methodsFile, callbacksFile);
        }

        public List<ResultSummaryData> SlicesSummaryData { get { return _broker.SlicesSummaryData; } }
    }
}
