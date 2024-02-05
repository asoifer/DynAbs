using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    public class ControlManagement : IControlManagement
    {
        Stack<CallInfo> mainControlFlowStack = new Stack<CallInfo>();
        CallGraph callGraph = new CallGraph();
        
        public void StmtToDgVtx(Stmt stmt, uint vtx)
        {
            // Se necesitan guardar todos los nodos porque pueden ser posibles callbacks y por lo tanto puede haber dependencia de control
            if (!(stmt.CSharpSyntaxNode is IfStatementSyntax || IsLoopOrSwitchStatement(stmt) || (stmt.CSharpSyntaxNode.Parent is ConditionalExpressionSyntax && ((ConditionalExpressionSyntax)stmt.CSharpSyntaxNode.Parent).Condition == stmt.CSharpSyntaxNode)))
                return;

            var stmtToNodesDict = mainControlFlowStack.Peek().DGInfo;
            if (!stmtToNodesDict.ContainsKey(stmt))
                stmtToNodesDict.Add(stmt, vtx);
            else
                stmtToNodesDict[stmt] = vtx;
        }

        public void EnterMethod(Stmt enterMethodStatement, uint? baseControlDependency = null)
        {
            mainControlFlowStack.Push(new CallInfo(baseControlDependency, enterMethodStatement, new Stack<Stmt>(), new Dictionary<Stmt, uint>()));
            callGraph.EnterMethod(enterMethodStatement);
        }

        public void ExitMethod(Stmt exitMethodStatement)
        {
            mainControlFlowStack.Pop();
            callGraph.ExitMethod(exitMethodStatement);
        }

        public uint GetCurrentControlVtx()
        {
            var currentControlFlowStack = mainControlFlowStack.Peek();
            var stmtToNodesDict = currentControlFlowStack.DGInfo;
            if (currentControlFlowStack.ControlDependencies.Count > 0 && stmtToNodesDict.ContainsKey(currentControlFlowStack.ControlDependencies.Peek()))
                return stmtToNodesDict[currentControlFlowStack.ControlDependencies.Peek()];
            return currentControlFlowStack.DGVertex ?? (uint)0;
        }
        
        public void EnterCondition(Stmt stmt)
        {
            mainControlFlowStack.Peek().ControlDependencies.Push(stmt);
        }

        public void ExitCondition()
        {
            PopCondition();
        }

        Stmt PopCondition()
        {
            var currentControlFlowStack = mainControlFlowStack.Peek();
            return currentControlFlowStack.ControlDependencies.Pop();
        }

        /// <summary>
        /// Retorna true si escapamos de un loop
        /// </summary>
        public bool Break()
        {
            bool loopStmtFound;
            Stmt lastPopedStmt;
            do
            {
                lastPopedStmt = PopCondition();
                loopStmtFound = IsLoopOrSwitchStatement(lastPopedStmt);
            } while (!loopStmtFound);

            return IsLoopStatement(lastPopedStmt);
        }

        public void Continue()
        {
            // XXX: Para nosotros en terminos del ControlManager, el continue es lo mismo que el break,
            // porque hay que hacer popear hasta la condicion, que luego se volvera a pushear al volver a
            // entrar en el LOOP. Se deja el Continue() por separado, porque si no, esto hay que explicarlo
            // en el ProcessConsumer, y da la sensacion de romper el encapsulamiento.

            // XXX(2): NO, Continue no hace nada. Tanto la salida del loop como el i++ (caso común o el iterator que haya) se vuelve a ejecutar. No hay que salir ni nada.
            //Break();

            // XXX(3): Hay que salir de los if. De lo demás salimos normal. 

            var currentControlFlowStack = mainControlFlowStack.Peek();
            var currentCondition = currentControlFlowStack.ControlDependencies.Peek();
            while (!IsLoopOrSwitchStatement(currentCondition))
            {
                currentControlFlowStack.ControlDependencies.Pop();
                currentCondition = currentControlFlowStack.ControlDependencies.Peek();
            }
        }

        public void PrintCallGraph(string path)
        {
            callGraph.PrintGraph(path);
        }

        public static bool IsLoopOrSwitchStatement(Stmt conditionStmt)
        {
            var controlStmt = conditionStmt.CSharpSyntaxNode;
            return (controlStmt is WhileStatementSyntax ||
                controlStmt is ForStatementSyntax ||
                controlStmt is DoStatementSyntax ||
                controlStmt is SwitchStatementSyntax ||
                controlStmt is SwitchExpressionSyntax ||
                controlStmt is WhenClauseSyntax ||
                controlStmt is ForEachStatementSyntax);
        }

        public static bool IsLoopStatement(Stmt conditionStmt)
        {
            var controlStmt = conditionStmt.CSharpSyntaxNode;
            return (controlStmt is WhileStatementSyntax ||
                controlStmt is ForStatementSyntax ||
                controlStmt is DoStatementSyntax ||
                controlStmt is ForEachStatementSyntax);
        }

        class CallInfo
        {
            public uint? DGVertex { get; set; }
            public Stmt EnterMethodStatement { get; set; }
            public Stack<Stmt> ControlDependencies { get; set; }
            public Dictionary<Stmt, uint> DGInfo { get; set; }
            public CallInfo(uint? dgVertex, Stmt enterMethodStatement, Stack<Stmt> controlDependencies, Dictionary<Stmt, uint> dgInfo)
            {
                DGVertex = dgVertex;
                EnterMethodStatement = enterMethodStatement;
                ControlDependencies = controlDependencies;
                DGInfo = dgInfo;
            }
        }
    }
}
