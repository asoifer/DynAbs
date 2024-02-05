using Microsoft.CodeAnalysis;
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
    public class TraceAnalyzer
    {
        ITraceConsumer _traceConsumer;
        ISemanticModelsContainer _semanticModelsContainer;
        InstrumentationResult _instrumentationResult;
        ExecutedStatementsContainer _executedStatements;

        public List<ResultSummaryData> SlicesSummaryData { get; set; }

        public TraceAnalyzer(
            ITraceConsumer traceConsumer,
            ISemanticModelsContainer semanticModelsContainer,
            InstrumentationResult instrumentationResult,
            ExecutedStatementsContainer executedStatements)
        {
            _traceConsumer = traceConsumer;
            _semanticModelsContainer = semanticModelsContainer;
            _instrumentationResult = instrumentationResult;
            _executedStatements = executedStatements;
            SlicesSummaryData = new List<ResultSummaryData>();
        }

        public void Analyze(out string entryPointClassName)
        {
            entryPointClassName = (_traceConsumer.HasNext()) ? _traceConsumer.ObserveNextStatement().FileName : "";
            while (_traceConsumer.HasNext())
            {
                var sliceCriteriaReached = _traceConsumer.SliceCriteriaReached();
                var nextStmt = _traceConsumer.GetNextStatement();
                if (Utils.IsEnterMethodOrConstructor(nextStmt.TraceType))
                {
                    if (nextStmt.TraceType == TraceType.BeforeConstructor)
                        _executedStatements.AddClass(nextStmt);
                    _executedStatements.AddMethod(nextStmt);
                }
                if (nextStmt.TraceType == TraceType.SimpleStatement)
                {
                    _executedStatements.Add(nextStmt);
                    var operation = GetOperation(nextStmt.CSharpSyntaxNode);
                    if (operation == null)
                        _executedStatements.AddPropertyOrFieldInitialization(nextStmt);

                    if (sliceCriteriaReached)
                    {
                        var Data = new ResultSummaryData(nextStmt.FileName, nextStmt.Line, _traceConsumer, 
                            _executedStatements, DateTime.Now.Subtract(Globals.start_time));
                        Data.SlicedStatements = new HashSet<Stmt>();
                        this.SlicesSummaryData.Add(Data);
                        if (_traceConsumer.RemoveCriteria())
                            break; ;
                    }
                }
            }
        }
        IOperation GetOperation(CSharpSyntaxNode syntaxNode)
        {
            if (syntaxNode is ParenthesizedExpressionSyntax)
                syntaxNode = ((ParenthesizedExpressionSyntax)syntaxNode).Expression;

            var p_semanticModel = _semanticModelsContainer.GetBySyntaxNode(syntaxNode);
            var p_operation = p_semanticModel.GetOperation(syntaxNode);
            return p_operation;
        }
    }
}
