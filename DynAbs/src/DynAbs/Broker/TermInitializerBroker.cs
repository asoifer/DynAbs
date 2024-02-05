using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class TermInitializerBroker : IBroker
    {
        IBroker _broker;

        public TermInitializerBroker(IBroker broker)
        {
            _broker = broker;
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
            try
            {
                _broker.EnterMethod(methodSymbol, argumentTermList, parameterTermList, receiver, @this, invocationPoint, enterMethodStatement);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                EnterMethod(methodSymbol, argumentTermList, parameterTermList, receiver, @this, invocationPoint, enterMethodStatement);
            }
        }

        public void ExitMethod(Stmt exitMethodStatement, Term returnValue, Term returnExceptionTerm, Term exceptionTerm)
        {
            _broker.ExitMethod(exitMethodStatement, returnValue, returnExceptionTerm, exceptionTerm);
        }

        public void Alloc(Term term)
        {
            try
            {
                _broker.Alloc(term);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                _broker.Alloc(term);
            }
        }

        public void DefExternalOperation(Term defTerm)
        {
            throw new SlicerException("No se puede llamar desde otro lado");
        }

        public void DefUseOperation(ISet<Term> defTerms, ISet<Term> useTerms)
        {
            try
            { 
                _broker.DefUseOperation(defTerms, useTerms);
            }
            catch(UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                DefUseOperation(defTerms, useTerms);
            }
        }

        public void DefUseOperation(Term defTerm, Term[] useTerms)
        {
            try
            {
                _broker.DefUseOperation(defTerm, useTerms);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                DefUseOperation(defTerm, useTerms);
            }
        }

        public void DefUseOperation(Term defTerm)
        {
            _broker.DefUseOperation(defTerm);
        }

        public void UseOperation(Stmt stmt, List<Term> useTerms)
        {
            try
            {
                _broker.UseOperation(stmt, useTerms);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                UseOperation(stmt, useTerms);
            }
        }

        public void Assign(Term defTerm, Term useTerm)
        {
            try
            {
                _broker.Assign(defTerm, useTerm);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                Assign(defTerm, useTerm);
            }
        }

        public void Assign(Term defTerm, Term useTerm, List<Term> anotherUses)
        {
            try
            {
                _broker.Assign(defTerm, useTerm, anotherUses);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                Assign(defTerm, useTerm, anotherUses);
            }
        }

        public void RedefineType(Term term)
        {
            _broker.RedefineType(term);
        }

        public void AssignRV(Term returnValue)
        {
            _broker.AssignRV(returnValue);
        }

        public void HandleNonInstrumentedMethod(List<Term> argumentTermList, Term @this, List<Term> returnedValues, Term returnValue, ISymbol symbol, string methodName = null)
        {
            try
            {
                _broker.HandleNonInstrumentedMethod(argumentTermList, @this, returnedValues, returnValue, symbol, methodName);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                HandleNonInstrumentedMethod(argumentTermList, @this, returnedValues, returnValue, symbol, methodName);
            }
        }

        public void HandleArrayInitialization(List<Term> argumentTermList, List<Term> returnedValues, Term returnValue)
        {
            try
            {
                _broker.HandleArrayInitialization(argumentTermList, returnedValues, returnValue);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                HandleArrayInitialization(argumentTermList, returnedValues, returnValue);
            }
        }

        public void CreateNonInstrumentedRegion(List<Term> involvedTerms, Term returnValue)
        {
            try
            {
                _broker.CreateNonInstrumentedRegion(involvedTerms, returnValue);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                CreateNonInstrumentedRegion(involvedTerms, returnValue);
            }
        }

        public void CatchReturnedValueIntoRegion(Term region, Term returnedValue)
        {
            try
            {
                _broker.CatchReturnedValueIntoRegion(region, returnedValue);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                CatchReturnedValueIntoRegion(region, returnedValue);
            }
        }

        public void CustomEvent(List<Term> argumentTermList, Term @this, List<Term> returnedValues, Term returnValue, string EventName)
        {
            try
            {
                _broker.CustomEvent(argumentTermList, @this, returnedValues, returnValue, EventName);
            }
            catch (UninitializedTerm uninitializedTerm)
            {
                InitializeTerm(uninitializedTerm.Term);
                CustomEvent(argumentTermList, @this, returnedValues, returnValue, EventName);
            }
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

        ISet<string> termsToInit = new HashSet<string>();
        public void InitializeTerm(Term term)
        {
            var added = termsToInit.Add(term.ToString());
            if (!added)
                throw new NonGlobalUninitializedTerm(term);

            if (!term.IsGlobal)
            {
                if (term.Count > 1)
                {
                    _broker.DefUseOperation(term, new Term[] { term.DiscardLast() });
                    return;
                }
                throw new NonGlobalUninitializedTerm(term);
            }

            if (!term.IsScalar)
                Alloc(term);
            _broker.DefExternalOperation(term);
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
