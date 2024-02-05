using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class PerformanceTestBroker: IBroker
    {
        IBroker _broker;
        IDictionary<string, int> _methodCounter; 

        public PerformanceTestBroker(IBroker broker)
        {
            _methodCounter = new Dictionary<string, int>();
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
            GlobalPerformanceValues.BrokerValues.Counters.Break++;
            var start = DateTime.Now;
            _broker.Break();
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.Break = GlobalPerformanceValues.BrokerValues.Times.Break.Add(diff);
        }

        public void Continue()
        {
            GlobalPerformanceValues.BrokerValues.Counters.Continue++;
            var start = DateTime.Now;
            _broker.Continue();
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.Continue = GlobalPerformanceValues.BrokerValues.Times.Continue.Add(diff);
        }

        public void EnterCondition(Stmt stmt)
        {
            GlobalPerformanceValues.BrokerValues.Counters.EnterCondition++;
            var start = DateTime.Now;
            _broker.EnterCondition(stmt);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.EnterCondition = GlobalPerformanceValues.BrokerValues.Times.EnterCondition.Add(diff);
        }

        public void ExitCondition(Stmt stmt)
        {
            GlobalPerformanceValues.BrokerValues.Counters.ExitCondition++;
            var start = DateTime.Now;
            _broker.ExitCondition(stmt);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.ExitCondition = GlobalPerformanceValues.BrokerValues.Times.ExitCondition.Add(diff);
        }

        public void EnterMethod(string methodSymbol, List<Term> argumentTermList, List<Term> parameterTermList, Term receiver, Term @this, Stmt invocationPoint, Stmt enterMethodStatement)
        {
            GlobalPerformanceValues.BrokerValues.Counters.EnterMethod++;
            var start = DateTime.Now;
            _broker.EnterMethod(methodSymbol, argumentTermList,parameterTermList,receiver,@this,invocationPoint,enterMethodStatement);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.EnterMethod = GlobalPerformanceValues.BrokerValues.Times.EnterMethod.Add(diff);
        }

        public void ExitMethod(Stmt exitMethodStatement, Term returnValue, Term returnExceptionTerm, Term exceptionTerm)
        {
            GlobalPerformanceValues.BrokerValues.Counters.ExitMethod++;
            var start = DateTime.Now;
            _broker.ExitMethod(exitMethodStatement, returnValue, returnExceptionTerm, exceptionTerm);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.ExitMethod = GlobalPerformanceValues.BrokerValues.Times.ExitMethod.Add(diff);
        }

        public void Alloc(Term term)
        {
            GlobalPerformanceValues.BrokerValues.Counters.Alloc++;
            var start = DateTime.Now;
            _broker.Alloc(term);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.Alloc = GlobalPerformanceValues.BrokerValues.Times.Alloc.Add(diff);
        }

        public void DefExternalOperation(Term defTerm)
        {
            throw new SlicerException("No puede llamarse desde afuera");
        }

        public void DefUseOperation(ISet<Term> defTerms, ISet<Term> useTerms)
        {
            GlobalPerformanceValues.BrokerValues.Counters.DefUseOperation++;
            var start = DateTime.Now;
            _broker.DefUseOperation(defTerms, useTerms);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.DefUseOperation = GlobalPerformanceValues.BrokerValues.Times.DefUseOperation.Add(diff);
        }

        public void DefUseOperation(Term defTerm, Term[] useTerms)
        {
            GlobalPerformanceValues.BrokerValues.Counters.DefUseOperation++;
            var start = DateTime.Now;
            _broker.DefUseOperation(defTerm, useTerms);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.DefUseOperation = GlobalPerformanceValues.BrokerValues.Times.DefUseOperation.Add(diff);
        }

        public void DefUseOperation(Term defTerm)
        {
            GlobalPerformanceValues.BrokerValues.Counters.DefUseOperation++;
            var start = DateTime.Now;
            _broker.DefUseOperation(defTerm);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.DefUseOperation = GlobalPerformanceValues.BrokerValues.Times.DefUseOperation.Add(diff);
        }

        public void UseOperation(Stmt stmt, List<Term> useTerms)
        {
            GlobalPerformanceValues.BrokerValues.Counters.DefUseOperation++;
            var start = DateTime.Now;
            _broker.UseOperation(stmt, useTerms);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.DefUseOperation = GlobalPerformanceValues.BrokerValues.Times.DefUseOperation.Add(diff);
        }

        public void Assign(Term defTerm, Term useTerm)
        {
            GlobalPerformanceValues.BrokerValues.Counters.Assign++;
            var start = DateTime.Now;
            _broker.Assign(defTerm, useTerm);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.Assign = GlobalPerformanceValues.BrokerValues.Times.Assign.Add(diff);
        }

        public void Assign(Term defTerm, Term useTerm, List<Term> anotherUses)
        {
            GlobalPerformanceValues.BrokerValues.Counters.Assign++;
            var start = DateTime.Now;
            _broker.Assign(defTerm, useTerm, anotherUses);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.Assign = GlobalPerformanceValues.BrokerValues.Times.Assign.Add(diff);
        }

        public void RedefineType(Term term)
        {
            GlobalPerformanceValues.BrokerValues.Counters.RedefineType++;
            var start = DateTime.Now;
            _broker.RedefineType(term);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.RedefineType = GlobalPerformanceValues.BrokerValues.Times.RedefineType.Add(diff);
        }

        public void AssignRV(Term returnValue)
        {
            GlobalPerformanceValues.BrokerValues.Counters.AssignRV++;
            var start = DateTime.Now;
            _broker.AssignRV(returnValue);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.AssignRV = GlobalPerformanceValues.BrokerValues.Times.AssignRV.Add(diff);
        }

        public void HandleNonInstrumentedMethod(List<Term> argumentTermList, Term @this, List<Term> returnedValues, Term returnValue, ISymbol symbol, string methodName = null)
        {
            if (symbol != null && symbol is IMethodSymbol)
            {
                string key = Utils.GetNamespaceName((IMethodSymbol)symbol) + "." + Utils.GetClassName((IMethodSymbol)symbol)
                    + "." + Utils.GetMethodName((IMethodSymbol)symbol);
                if (_methodCounter.ContainsKey(key))
                    _methodCounter[key] = _methodCounter[key] + 1;
                else
                    _methodCounter.Add(key, 1);
            }
            GlobalPerformanceValues.BrokerValues.Counters.HandleNonInstrumentedMethod++;
            var start = DateTime.Now;
            _broker.HandleNonInstrumentedMethod(argumentTermList, @this, returnedValues, returnValue, symbol, methodName);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.HandleNonInstrumentedMethod = GlobalPerformanceValues.BrokerValues.Times.HandleNonInstrumentedMethod.Add(diff);
        }

        public void HandleArrayInitialization(List<Term> argumentTermList, List<Term> returnedValues, Term returnValue)
        {
            GlobalPerformanceValues.BrokerValues.Counters.HandleArrayInitialization++;
            var start = DateTime.Now;
            _broker.HandleArrayInitialization(argumentTermList, returnedValues, returnValue);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.HandleArrayInitialization = 
                GlobalPerformanceValues.BrokerValues.Times.HandleArrayInitialization.Add(diff);
        }

        public void CreateNonInstrumentedRegion(List<Term> involvedTerms, Term returnValue)
        {
            GlobalPerformanceValues.BrokerValues.Counters.CreateNonInstrumentedRegion++;
            var start = DateTime.Now;
            _broker.CreateNonInstrumentedRegion(involvedTerms, returnValue);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.CreateNonInstrumentedRegion =
                GlobalPerformanceValues.BrokerValues.Times.CreateNonInstrumentedRegion.Add(diff);
        }

        public void CatchReturnedValueIntoRegion(Term region, Term returnedValue)
        {
            GlobalPerformanceValues.BrokerValues.Counters.CatchReturnedValueIntoRegion++;
            var start = DateTime.Now;
            _broker.CatchReturnedValueIntoRegion(region, returnedValue);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.CatchReturnedValueIntoRegion =
                GlobalPerformanceValues.BrokerValues.Times.CatchReturnedValueIntoRegion.Add(diff);
        }

        public void CustomEvent(List<Term> argumentTermList, Term @this, List<Term> returnedValues, Term returnValue, string EventName)
        {
            GlobalPerformanceValues.BrokerValues.Counters.HandleNonInstrumentedMethod++;
            var start = DateTime.Now;
            _broker.CustomEvent(argumentTermList, @this, returnedValues, returnValue, EventName);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.HandleNonInstrumentedMethod = GlobalPerformanceValues.BrokerValues.Times.HandleNonInstrumentedMethod.Add(diff);
        }

        public bool EnterStaticMode(bool EnterByCallbacks = false)
        {
            GlobalPerformanceValues.BrokerValues.Counters.EnterStaticMode++;
            var start = DateTime.Now;
            var temp = _broker.EnterStaticMode(EnterByCallbacks);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.EnterStaticMode =
                GlobalPerformanceValues.BrokerValues.Times.EnterStaticMode.Add(diff);
            return temp;
        }

        public void ExitStaticMode()
        {
            GlobalPerformanceValues.BrokerValues.Counters.ExitStaticMode++;
            var start = DateTime.Now;
            _broker.ExitStaticMode();
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.BrokerValues.Times.ExitStaticMode =
                GlobalPerformanceValues.BrokerValues.Times.ExitStaticMode.Add(diff);
        }

        public bool StaticMode { get { return _broker.StaticMode; } }

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

        public void Slice(ResultSummaryData Data)
        {
            _broker.Slice(Data);
        }

        public List<ISet<Stmt>> GetSlices()
        {
            return _broker.GetSlices();
        }

        public void SaveResults(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                return;

            var sb = new StringBuilder();
            foreach (var elem in _methodCounter.OrderByDescending(x => x.Value))
                sb.AppendLine(elem.Key + ": " + elem.Value);
            System.IO.File.WriteAllText(file, sb.ToString());
        }

        public void PrintResults()
        {
            var newDiccOrd = _methodCounter.OrderByDescending(x => x.Value).Take(10);
            foreach (var elem in newDiccOrd)
                Console.WriteLine(elem.Key + ": " + elem.Value);
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
