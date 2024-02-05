using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    public interface IControlManagement
    {
        uint GetCurrentControlVtx();
        void EnterCondition(Stmt stmt);
        void ExitCondition();
        void EnterMethod(Stmt enterMethodStatement, uint? baseControlDependency = null);
        void ExitMethod(Stmt exitMethodStatement);
        bool Break();
        void Continue();
        void StmtToDgVtx(Stmt stmt, uint vtx);
        void PrintCallGraph(string path);
    }
}
