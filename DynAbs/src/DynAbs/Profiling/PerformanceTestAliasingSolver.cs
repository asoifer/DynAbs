using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class PerformanceTestAliasingSolver : IAliasingSolver
    {
        public IAliasingSolver solver;

        public PerformanceTestAliasingSolver(IAliasingSolver aliasingSolver)
        {
            solver = aliasingSolver;
        }

        public ISet<uint> LastDef_Get(Term term)
        {
            GlobalPerformanceValues.AliasingSolverValues.Counters.LastDefGet++;
            var start = DateTime.Now;
            var temp = solver.LastDef_Get(term);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.AliasingSolverValues.Times.LastDefGet =
                GlobalPerformanceValues.AliasingSolverValues.Times.LastDefGet.Add(diff);
            if (diff > GlobalPerformanceValues.AliasingSolverValues.Times.LastDefGet_Max)
                GlobalPerformanceValues.AliasingSolverValues.Times.LastDefGet_Max = diff;
            return temp;
        }

        public void LastDef_Set(Term term, uint lastDef)
        {
            GlobalPerformanceValues.AliasingSolverValues.Counters.LastDefSet++;
            var start = DateTime.Now;
            solver.LastDef_Set(term, lastDef);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.AliasingSolverValues.Times.LastDefSet =
                GlobalPerformanceValues.AliasingSolverValues.Times.LastDefSet.Add(diff);
            if (diff > GlobalPerformanceValues.AliasingSolverValues.Times.LastDefSet_Max) 
                GlobalPerformanceValues.AliasingSolverValues.Times.LastDefSet_Max = diff;
        }

        public void Alloc(Term term, bool @override = true, string kind = null)
        {
            GlobalPerformanceValues.AliasingSolverValues.Counters.Alloc++;
            var start = DateTime.Now;
            solver.Alloc(term, @override);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.AliasingSolverValues.Times.Alloc = 
                GlobalPerformanceValues.AliasingSolverValues.Times.Alloc.Add(diff);
        }

        public void Assign(Term term1, Term term2)
        {
            GlobalPerformanceValues.AliasingSolverValues.Counters.Assign++;
            var start = DateTime.Now;
            solver.Assign(term1, term2);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.AliasingSolverValues.Times.Assign =
                GlobalPerformanceValues.AliasingSolverValues.Times.Assign.Add(diff);
        }

        public void RedefineType(Term term)
        {
            GlobalPerformanceValues.AliasingSolverValues.Counters.RedefineType++;
            var start = DateTime.Now;
            solver.RedefineType(term);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.AliasingSolverValues.Times.RedefineType =
                GlobalPerformanceValues.AliasingSolverValues.Times.RedefineType.Add(diff);
        }

        public void AssignRV(Term term)
        {
            GlobalPerformanceValues.AliasingSolverValues.Counters.AssignRV++;
            var start = DateTime.Now;
            solver.AssignRV(term);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.AliasingSolverValues.Times.AssignRV = 
                GlobalPerformanceValues.AliasingSolverValues.Times.AssignRV.Add(diff);
        }

        public void EnterMethodAndBind(string methodSymbol, List<Term> actualParams, List<Term> formalParams, Term receiver, Term @this)
        {
            GlobalPerformanceValues.AliasingSolverValues.Counters.EnterMethodAndBind++;
            var start = DateTime.Now;
            solver.EnterMethodAndBind(methodSymbol, actualParams, formalParams, receiver, @this);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.AliasingSolverValues.Times.EnterMethodAndBind =
                GlobalPerformanceValues.AliasingSolverValues.Times.EnterMethodAndBind.Add(diff);
        }

        public ISet<uint> ExitMethodAndUnbind(Term term, Term returnExceptionTerm, Term exceptionTerm)
        {
            GlobalPerformanceValues.AliasingSolverValues.Counters.ExitMethodAndUnbind++;
            var start = DateTime.Now;
            var temp = solver.ExitMethodAndUnbind(term, returnExceptionTerm, exceptionTerm);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.AliasingSolverValues.Times.ExitMethodAndUnbind =
                GlobalPerformanceValues.AliasingSolverValues.Times.ExitMethodAndUnbind.Add(diff);
            return temp;
        }

        public void Havoc(Term receiver, List<Term> arguments, Term returnValue, Func<Stmt, ISet<uint>, uint> GetDGNode, AnnotationWithData ad, Stmt invocationPoint)
        {
            GlobalPerformanceValues.AliasingSolverValues.Counters.Havoc++;
            var start = DateTime.Now;
            solver.Havoc(receiver, arguments, returnValue, GetDGNode, ad, invocationPoint);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.AliasingSolverValues.Times.Havoc =
                GlobalPerformanceValues.AliasingSolverValues.Times.Havoc.Add(diff);
        }

        public bool EnterStaticMode(bool EnterByCallbacks = false)
        {
            GlobalPerformanceValues.AliasingSolverValues.Counters.EnterStaticMode++;
            var start = DateTime.Now;
            var temp = solver.EnterStaticMode(EnterByCallbacks);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.AliasingSolverValues.Times.EnterStaticMode =
                GlobalPerformanceValues.AliasingSolverValues.Times.EnterStaticMode.Add(diff);
            return temp;
        }

        public void ExitStaticMode()
        {
            GlobalPerformanceValues.AliasingSolverValues.Counters.ExitStaticMode++;
            var start = DateTime.Now;
            solver.ExitStaticMode();
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.AliasingSolverValues.Times.ExitStaticMode =
                GlobalPerformanceValues.AliasingSolverValues.Times.ExitStaticMode.Add(diff);
        }

        public void EnterLoop()
        {
            solver.EnterLoop();
        }

        public void NextLoopIteration()
        {
            solver.NextLoopIteration();
        }

        public void ExitLoop()
        {
            solver.ExitLoop();
        }

        public bool Converged()
        {
            return solver.Converged();
        }

        public void CleanTemporaryEntries()
        {
            GlobalPerformanceValues.AliasingSolverValues.Counters.CleanTemporaryEntries++;
            var start = DateTime.Now;
            solver.CleanTemporaryEntries();
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.AliasingSolverValues.Times.CleanTemporaryEntries =
                GlobalPerformanceValues.AliasingSolverValues.Times.CleanTemporaryEntries.Add(diff);
        }

        public bool StaticMode { get { return solver.StaticMode; } }

        public void DumpPTG(string path, string label = null, bool globalScope = false, string key = null)
        {
            solver.DumpPTG(path, label, globalScope, key);
        }

        public void SaveResults(string graphEvolutionFile, string internalProfileFile)
        {
            solver.SaveResults(graphEvolutionFile, internalProfileFile);
        }
    }
}
