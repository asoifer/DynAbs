using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class MultiSolver : IAliasingSolver
    {
        ISet<IAliasingSolver> solvers;
        IAliasingSolver mainSolver;

        public MultiSolver(UserSliceConfiguration userSliceConfiguration)
        {
            mainSolver = new BasicSolver(userSliceConfiguration);
            solvers = new HashSet<IAliasingSolver>();
            solvers.Add(mainSolver);
            solvers.Add(new Aliasing.CS.ClustersSolver(userSliceConfiguration));
        }

        public void Alloc(Term term, bool @override = true, string kind = null)
        {
            foreach (var ptg in solvers)
                ptg.Alloc(term, @override, kind);
        }

        public void Assign(Term term1, Term term2)
        {
            List<ISet<uint>> results = new List<ISet<uint>>();
            foreach (var ptg in solvers.Except(new IAliasingSolver[] { mainSolver }))
                results.Add(ptg.LastDef_Get(term2));
            results.Add(mainSolver.LastDef_Get(term2));

            if (!results.First().SetEquals(results.Last()))
                ;

            foreach (var ptg in solvers)
                ptg.Assign(term1, term2);

            List<ISet<uint>> results2 = new List<ISet<uint>>();
            foreach (var ptg in solvers.Except(new IAliasingSolver[] { mainSolver }))
                results2.Add(ptg.LastDef_Get(term1));
            results2.Add(mainSolver.LastDef_Get(term1));

            if (!results2.First().SetEquals(results2.Last()))
                ;
        }

        public void AssignRV(Term term)
        {
            foreach (var ptg in solvers)
                ptg.AssignRV(term);
        }

        public void CleanTemporaryEntries()
        {
            foreach (var ptg in solvers)
                ptg.CleanTemporaryEntries();
        }

        public void DumpPTG(string path, string label = null, bool globalScope = false, string key = null)
        {
            foreach (var ptg in solvers)
                ptg.DumpPTG(path, label, globalScope, key);
        }

        public void EnterMethodAndBind(string methodSymbol, List<Term> actualParams, List<Term> formalParams, Term receiver, Term @this)
        {
            foreach (var ptg in solvers)
                ptg.EnterMethodAndBind(methodSymbol, actualParams, formalParams, receiver, @this);
        }

        public bool EnterStaticMode(bool EnterByCallbacks = false)
        {
            bool returnValue = false;
            foreach (var ptg in solvers)
                returnValue = ptg.EnterStaticMode(EnterByCallbacks);
            return returnValue;
        }

        public ISet<uint> ExitMethodAndUnbind(Term term, Term returnExceptionTerm, Term exceptionTerm)
        {
            foreach (var ptg in solvers.Except(new IAliasingSolver[] { mainSolver }))
                ptg.ExitMethodAndUnbind(term, returnExceptionTerm, exceptionTerm);
            return mainSolver.ExitMethodAndUnbind(term, returnExceptionTerm, exceptionTerm);
        }

        public void ExitStaticMode()
        {
            foreach (var ptg in solvers)
                ptg.ExitStaticMode();
        }

        public bool StaticMode { get { return mainSolver.StaticMode; } }

        public void Havoc(Term receiver, List<Term> arguments, Term returnValue, Func<Stmt, ISet<uint>, uint> getDGNode, AnnotationWithData ad, Stmt invocationPoint)
        {
            foreach (var ptg in solvers)
                ptg.Havoc(receiver, arguments, returnValue, getDGNode, ad, invocationPoint);
        }

        public ISet<uint> LastDef_Get(Term term)
        {
            List<ISet<uint>> results = new List<ISet<uint>>();
            foreach (var ptg in solvers.Except(new IAliasingSolver[] { mainSolver }))
                results.Add(ptg.LastDef_Get(term));
            results.Add(mainSolver.LastDef_Get(term));

            if (!results.First().SetEquals(results.Last()))
                ;

            return results.Last();
        }

        public void LastDef_Set(Term term, uint lastDef)
        {
            List<ISet<uint>> results = new List<ISet<uint>>();
            var hasException = false;
            try
            {
                foreach (var ptg in solvers.Except(new IAliasingSolver[] { mainSolver }))
                    results.Add(ptg.LastDef_Get(term));
                results.Add(mainSolver.LastDef_Get(term));
            }
            catch (Exception ex) { hasException = true; }

            if (!hasException)
                if (!results.First().SetEquals(results.Last()))
                    ;

            foreach (var ptg in solvers)
                ptg.LastDef_Set(term, lastDef);

            List<ISet<uint>> results2 = new List<ISet<uint>>();
            foreach (var ptg in solvers.Except(new IAliasingSolver[] { mainSolver }))
                results2.Add(ptg.LastDef_Get(term));
            results2.Add(mainSolver.LastDef_Get(term));

            if (!results2.First().SetEquals(results2.Last()))
                ;
        }

        public void EnterLoop()
        {
            foreach (var ptg in solvers)
                ptg.EnterLoop();
        }

        public void NextLoopIteration()
        {
            foreach (var ptg in solvers)
                ptg.NextLoopIteration();
        }

        public void ExitLoop()
        {
            foreach (var ptg in solvers)
                ptg.ExitLoop();
        }

        public bool Converged()
        {
            foreach (var ptg in solvers)
                ptg.Converged();
            return mainSolver.Converged();
        }

        public void RedefineType(Term term)
        {
            foreach (var ptg in solvers)
                ptg.RedefineType(term);
        }

        public void SaveResults(string graphEvolutionFile, string internalProfileFile)
        {
            foreach (var ptg in solvers)
                ptg.SaveResults(graphEvolutionFile, internalProfileFile);
        }
    }
}
