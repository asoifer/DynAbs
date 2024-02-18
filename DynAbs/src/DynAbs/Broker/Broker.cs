using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    public class Broker : IBroker
    {
        public IAliasingSolver Solver { get; set; }
        public IDependencyGraph DependencyGraph { get; set; }
        IControlManagement ControlManagement { get; set; }
        UserSliceConfiguration UserConfiguration { get; set; }
        AnnotationsUtils AnnotationsUtils { get; set; }

        Stack<bool> _SliceCriteriaReached { get; set; }
        public bool SliceCriteriaReached
        {
            get
            {
                return _SliceCriteriaReached.Peek();
            }
            set
            {
                if (_SliceCriteriaReached.Count > 0)
                { 
                    _SliceCriteriaReached.Pop();
                    _SliceCriteriaReached.Push(value);
                }
            }
        }

        public List<ResultSummaryData> SlicesSummaryData { get; set; }

        public Broker(UserSliceConfiguration userSliceConfiguration, IAliasingSolver aliasingSolver, IDependencyGraph dependencyGraph, IControlManagement controlManagement)
        {
            UserConfiguration = userSliceConfiguration;
            Solver = aliasingSolver;
            DependencyGraph = dependencyGraph;
            ControlManagement = controlManagement;
            _SliceCriteriaReached = new Stack<bool>();
            SlicesSummaryData = new List<ResultSummaryData>();
            AnnotationsUtils = new AnnotationsUtils(userSliceConfiguration);
        }

        public void Break()
        {
            ControlManagement.Break();
        }

        public void Continue()
        {
            ControlManagement.Continue();
        }

        public void EnterCondition(Stmt stmt)
        {
            // Acá chequear si estamos dentro de un for y mandarle GAS
            ControlManagement.EnterCondition(stmt);
        }

        public void ExitCondition(Stmt stmt)
        {
            ControlManagement.ExitCondition();
        }

        public void EnterMethod(string methodSymbol, List<Term> argumentTermList, List<Term> parameterTermList, Term receiver, Term @this, Stmt invocationPoint, Stmt enterMethodStatement)
        {
            uint? control_dep_caller = null;
            if (invocationPoint != null)
            {
                var uses = new HashSet<uint>();
                if (Globals.include_receiver_use_on_calls && receiver != null)
                    uses.UnionWith(Solver.LastDef_Get(receiver));
                control_dep_caller = AddDgVertex(invocationPoint, uses);
            }

            _SliceCriteriaReached.Push(false);
            ControlManagement.EnterMethod(enterMethodStatement, control_dep_caller);

            var formalParamsDGVertices = new List<uint>();
            var formalParameters = new List<Term>();
            uint thisVtx = 0;

            int fpIndex = 0;
            foreach (var actualTerm in argumentTermList)
            {
                Term formalTerm = parameterTermList[fpIndex];

                formalParameters.Add(formalTerm);

                var vertices = Solver.LastDef_Get(actualTerm);
                var formalParameterVertex = AddDgVertex(formalTerm.Stmt, vertices);

                formalParamsDGVertices.Add(formalParameterVertex);
                fpIndex++;
            }

            if (receiver != null)
            {
                var thisDependencies = Solver.LastDef_Get(receiver);
                thisVtx = AddDgVertex(@this.Stmt, thisDependencies);
            }

            Solver.EnterMethodAndBind(methodSymbol, argumentTermList, formalParameters, receiver, @this);

            fpIndex = 0;
            foreach (var formalTerm in formalParameters)
            {
                Term actualTerm = argumentTermList[fpIndex];
                if (actualTerm.IsScalar && formalTerm.IsTypeObject)
                {
                    /* Se realiza el Alloc del termino, por mas que el actualTerm
                     * sea escalar, ya que ahora es una referencia por ser object. */
                    Alloc(formalTerm);
                }
                if (fpIndex < formalParamsDGVertices.Count)
                {
                    var formalParameterVertex = formalParamsDGVertices[fpIndex];
                    Solver.LastDef_Set(formalTerm, formalParameterVertex);
                }
                fpIndex++;
            }
            if (thisVtx != 0)
            {
                Solver.LastDef_Set(@this, thisVtx);
            }
        }

        public void ExitMethod(Stmt exitMethodStatement, Term returnValue, Term returnExceptionTerm, Term exceptionTerm)
        {
            ControlManagement.ExitMethod(exitMethodStatement);
            var returnDgVtx = Solver.ExitMethodAndUnbind(returnValue, returnExceptionTerm, exceptionTerm);

            _SliceCriteriaReached.Pop();

            if (returnValue != null && returnDgVtx != null)
            {
                var invocationDgVtx = AddDgVertex(returnValue.Stmt, returnDgVtx ?? new HashSet<uint>());
                Solver.LastDef_Set(returnValue, invocationDgVtx);
            }
        }

        public void Alloc(Term term)
        {
            Solver.Alloc(term);
        }

        public void DefExternalOperation(Term defTerm)
        {
            Solver.LastDef_Set(defTerm, DependencyGraphStmtDescriptor.VertexIdForExternal);
            if (!defTerm.IsScalar)
            {
                var defTermProperties = defTerm.AddingField(Field.SigmaField(ISlicerSymbol.CreateNullTypeSymbol()));
                defTermProperties.Stmt = defTerm.Stmt;
                Solver.LastDef_Set(defTermProperties, DependencyGraphStmtDescriptor.VertexIdForExternal);
            }
        }

        public void DefUseOperation(ISet<Term> defTerms, ISet<Term> useTerms)
        {
            // Caso "int x, y, z = a + b" tiene x, y, z como defs y a, b como usos.
            var dependencies = new HashSet<uint>();
            foreach (var useTerm in useTerms)
            {
                var useTermLastDef = Solver.LastDef_Get(useTerm);
                if (useTermLastDef.Count == 0)
                    throw new UninitializedTerm(useTerm);
                dependencies.UnionWith(useTermLastDef);
            }
            // Se incluyen los uses del SET hasta el último punto
            if (UserConfiguration.IncludeAllUses)
                foreach (var defTerm in defTerms.Where(x => x.Count > 1))
                    dependencies.UnionWith(Solver.LastDef_Get(defTerm.DiscardLast()));
            foreach (var defTerm in defTerms)
            {
                var vtx = AddDgVertex(defTerm.Stmt, dependencies);
                Solver.LastDef_Set(defTerm, vtx);
            }
        }

        public void DefUseOperation(Term defTerm, Term[] useTerms)
        {
            DefUseOperation(new HashSet<Term>() { defTerm }, new HashSet<Term>(useTerms));
        }

        public void DefUseOperation(Term defTerm)
        {
            DefUseOperation(defTerm, new Term[] { });
        }

        public void UseOperation(Stmt stmt, List<Term> useTerms)
        {
            ISet<uint> dependencies = new HashSet<uint>();
            foreach (var useTerm in useTerms)
            {
                var useTermLastDef = Solver.LastDef_Get(useTerm);
                if (useTermLastDef.Count == 0)
                    throw new UninitializedTerm(useTerm);
                dependencies = new HashSet<uint>(dependencies.Union(useTermLastDef));
            }
            AddDgVertex(stmt, dependencies);
        }

        public void Assign(Term defTerm, Term useTerm, List<Term> anotherUses)
        {
            ISet<uint> useTermLastDef = new HashSet<uint>();
            if (useTerm != null)
                useTermLastDef = Solver.LastDef_Get(useTerm);

            if (anotherUses != null)
                foreach (var anotherUse in anotherUses)
                    useTermLastDef.UnionWith(Solver.LastDef_Get(anotherUse));

            // It includes the uses of the left hand side
            if (UserConfiguration.IncludeAllUses && defTerm.Count > 1)
                useTermLastDef.UnionWith(Solver.LastDef_Get(defTerm.DiscardLast()));

            var vtx = AddDgVertex(defTerm.Stmt, useTermLastDef);
            Solver.LastDef_Set(defTerm, vtx);

            if (defTerm != null && useTerm != null && !defTerm.IsScalar && (!useTerm.IsScalar || (useTerm.Parts.Count == 1 && useTerm.Parts.Last().Symbol.IsNullSymbol)))
                Solver.Assign(defTerm, useTerm);
        }

        public void Assign(Term defTerm, Term useTerm)
        {
            Assign(defTerm, useTerm, null);
        }

        public void RedefineType(Term term)
        {
            Solver.RedefineType(term);
        }

        public void AssignRV(Term returnValue)
        {
            Solver.AssignRV(returnValue);
        }

        void HandleNonInstrumentedMethod(List<Term> argumentTermList, Term @this, Term returnValue, List<Term> anotherUses, List<Term> returnedValues, AnnotationWithData annotationWithData)
        {
            var tempArgs = new List<Term>(argumentTermList);
            if (anotherUses != null && anotherUses.Count > 0)
                tempArgs.AddRange(anotherUses);
            if (returnedValues != null && returnedValues.Count > 0)
                tempArgs.AddRange(returnedValues);
            
            Solver.Havoc(@this, tempArgs, returnValue, this.AddDgVertex, annotationWithData, returnValue != null ? returnValue.Stmt : @this.Stmt);
        }

        public void HandleNonInstrumentedMethod(List<Term> argumentTermList, Term @this, List<Term> returnedValues, Term returnValue, ISymbol symbol, string methodName = null)
        {
            var summariesLanguageInfo = AnnotationsUtils.GetAnnotation(symbol, methodName);
            AnnotationWithData ad = null;
            if (summariesLanguageInfo != null)
            { 
                var dict = AnnotationsUtils.GetMapping(summariesLanguageInfo, @this, argumentTermList, returnValue, symbol, methodName);
                IDictionary<string, string> fieldsParameters = null;
                if (summariesLanguageInfo.Parameters.Count > 0)
                {
                    fieldsParameters = new Dictionary<string, string>();
                    foreach (var p in summariesLanguageInfo.Parameters)
                    {
                        if (p == "RFieldA")
                        {
                            if (@this.Stmt.FileName.Contains("Main.cs") && @this.Stmt.Line == 48)
                                fieldsParameters.Add(p, "?");
                            else
                                fieldsParameters.Add(p, "#");
                        }
                        else if (p == "RFieldB")
                        {
                            if (@this.Stmt.FileName.Contains("Main.cs") && @this.Stmt.Line == 48)
                                fieldsParameters.Add(p, "?");
                            else
                                fieldsParameters.Add(p, "#");
                        }
                        else if (p == "WField")
                        {
                            if (@this.Stmt.FileName.Contains("MainJPA") || @this.Stmt.Line == 48)
                                fieldsParameters.Add(p, "id");
                            else if (@this.Stmt.Line == 30)
                                fieldsParameters.Add(p, "customerId");
                            else if (@this.Stmt.Line == 103 || @this.Stmt.Line == 79)
                                fieldsParameters.Add(p, "paymentId");
                            else if (@this.Stmt.FileName.Contains("Program"))
                                fieldsParameters.Add(p, "id");
                            else
                                fieldsParameters.Add(p, "?");
                            //throw new Exception("Hay que setear");
                        }
                        else
                            throw new Exception("Unexpected field parameter");
                        //fieldsParameters.Add(p, "id"); // TODO XXX: Reemplazar con lo que corresponda en su momento.
                    }

                    // customerId (30), paymentId (103)
                }

                ad = new AnnotationWithData(summariesLanguageInfo, dict, fieldsParameters);
            }
            LogCall(symbol, methodName);
            HandleNonInstrumentedMethod(argumentTermList, @this, returnValue, null, returnedValues, ad);
        }

        public void HandleArrayInitialization(List<Term> argumentTermList, List<Term> returnedValues, Term returnValue)
        {
            LogCall(returnValue.Last.Symbol.Symbol, "ctor");

            var summariesLanguageInfo = AnnotationsUtils.GetAnnotation(returnValue.Last.Symbol.Symbol, "ctor");
            HandleNonInstrumentedMethod(argumentTermList, null, returnValue, new List<Term>(), new List<Term>(), new AnnotationWithData(summariesLanguageInfo, null, null));
        }

        public void CustomEvent(List<Term> argumentTermList, Term @this, List<Term> returnedValues, Term returnValue, string EventName)
        {
            var summariesLanguageInfo = AnnotationsUtils.GetPredefinedAnnotation(EventName);
            HandleNonInstrumentedMethod(argumentTermList, @this, returnValue, null, returnedValues, new AnnotationWithData(summariesLanguageInfo, null, null));
        }

        public void CreateNonInstrumentedRegion(List<Term> involvedTerms, Term returnValue)
        {
            AnnotationWithData ad = null;
            if (UserConfiguration.UseAnnotations && !UserConfiguration.MixedModes)
                ad = new AnnotationWithData(AnnotationsUtils.GetPredefinedAnnotation("HavocWithoutGlobals_IsIn_Many"), null, null);
            HandleNonInstrumentedMethod(involvedTerms, null, returnValue, new List<Term>(), new List<Term>(), ad);
        }

        public void CatchReturnedValueIntoRegion(Term region, Term returnedValue)
        {
            AnnotationWithData ad = null;
            if (UserConfiguration.UseAnnotations && !UserConfiguration.MixedModes)
                ad = new AnnotationWithData(AnnotationsUtils.GetPredefinedAnnotation("HavocWithoutGlobals_IsIn_Many"), null, null);
            HandleNonInstrumentedMethod(new List<Term>() { returnedValue }, region, null, new List<Term>(), new List<Term>(), ad);
        }

        public bool EnterStaticMode(bool EnterByCallbacks = false)
        {
            return Solver.EnterStaticMode(EnterByCallbacks);
        }

        public void ExitStaticMode()
        {
            Solver.ExitStaticMode();
        }
        
        public bool StaticMode { get { return Solver.StaticMode; } }

        private uint AddDgVertex(Stmt stmt, ISet<uint> dependencies)
        {
            var controlDep = ControlManagement.GetCurrentControlVtx();

            if (controlDep != 0 && UserConfiguration.IncludeControlDependencies)
                dependencies.Add(controlDep);

            var vtx = DependencyGraph.AddVertex(stmt, dependencies);

            if (SliceCriteriaReached)
                DependencyGraph.CriteriaVertex = vtx;

            ControlManagement.StmtToDgVtx(stmt, vtx);

            return vtx;
        }

        public void Slice(ResultSummaryData Data)
        {
            Data.SlicedStatements = DependencyGraph.Slice();
            this.SlicesSummaryData.Add(Data);
        }

        public List<ISet<Stmt>> GetSlices()
        {
            return DependencyGraph.GetSlices();
        }

        public void EnterLoop()
        {
            Solver.EnterLoop();
        }
        public void NextLoopIteration()
        {
            Solver.NextLoopIteration();
        }
        public void ExitLoop()
        {
            Solver.ExitLoop();
        }

        public void PrintCallGraph(string path)
        {
            ControlManagement.PrintCallGraph(path);
        }

        
        public Dictionary<string, CallInformation> CallInfo = new Dictionary<string, CallInformation>();
        public void LogCall(ISymbol symbol, string methodName = null)
        {
            if (UserConfiguration.LogCalls) 
            { 
                var keyCaller = GetKey(symbol, methodName);
                CallInformation callerInfo;
                if (!CallInfo.TryGetValue(keyCaller, out callerInfo))
                {
                    callerInfo = new CallInformation(symbol, methodName);
                    CallInfo.Add(keyCaller, callerInfo);
                }

                var summariesLanguageInfo = AnnotationsUtils.GetAnnotation(symbol, methodName);
                callerInfo.counter++;
                callerInfo.annotation = summariesLanguageInfo != null ? summariesLanguageInfo.Annotation : "FULL HAVOC";
            }
        }

        public void LogCallback(ISymbol callback, ISymbol symbol, string methodName = null)
        {
            if (UserConfiguration.LogCalls)
            { 
                var keyCaller = GetKey(symbol, methodName);
                CallInformation callerInfo;
                if (!CallInfo.TryGetValue(keyCaller, out callerInfo))
                {
                    callerInfo = new CallInformation(symbol, methodName);
                    CallInfo.Add(keyCaller, callerInfo);
                }

                CallInformation callbackInfo;
                var keyCallback = GetKey(callback);
                if (!callerInfo.callbacks.TryGetValue(keyCallback, out callbackInfo))
                {
                    callbackInfo = new CallInformation(callback);
                    callerInfo.callbacks.Add(keyCallback, callbackInfo);
                }

                callbackInfo.counter++;
            }
        }

        public class CallInformation
        {
            public string fullName;
            public string @namespace;
            public string @class;
            public string propertyOrMethodName;
            public string parameters;
            public string kind; // Property or Method
            public int counter;
            public string annotation;
            public Dictionary<string, CallInformation> callbacks = new Dictionary<string, CallInformation>();

            public CallInformation(ISymbol symbol, string methodName = null)
            {
                if (symbol == null && methodName != null)
                {
                    this.@namespace = "";
                    this.@class = "";
                    this.propertyOrMethodName = methodName;
                    this.parameters = "";
                    this.kind = "OTHER";
                } 
                else if (symbol.Kind == SymbolKind.DynamicType)
                {
                    this.@namespace = "";
                    this.@class = "";
                    this.propertyOrMethodName = "";
                    this.parameters = "";
                    this.kind = "DYNAMIC";
                }
                else
                { 
                    this.@namespace = Utils.GetNamespaceName(symbol);
                    this.@class = Utils.GetClassName(symbol);
                    this.propertyOrMethodName = Utils.GetMethodName(symbol, methodName);
                    this.parameters = symbol is IMethodSymbol ? string.Join(",", ((IMethodSymbol)symbol).Parameters.Select(x => x.Type.ToString() + " " + x.Name)) : "";
                    this.kind = symbol is IMethodSymbol || symbol is IArrayTypeSymbol ? "METHOD" : "PROPERTY";;
                }
                this.counter = 0;
                this.annotation = "";
            }

            public string CustomToString()
            {
                return $"{this.fullName}|{this.@namespace}|{this.@class}|{this.propertyOrMethodName}|{this.parameters}|{this.kind}|{this.counter}|{this.annotation}";
            }
        }

        public string GetKey(ISymbol symbol, string methodName = null)
        {
            string key = "";
            if (symbol != null && symbol is IMethodSymbol)
                key = Utils.GetNamespaceName((IMethodSymbol)symbol) + "." + Utils.GetClassName((IMethodSymbol)symbol)
                    + "." + Utils.GetMethodName((IMethodSymbol)symbol);
            else if (symbol != null && symbol is IPropertySymbol)
                key = Utils.GetNamespaceName((IPropertySymbol)symbol) + "." + Utils.GetClassName((IPropertySymbol)symbol)
                    + "." + methodName;
            else if (symbol != null && symbol is INamedTypeSymbol)
                key = Utils.GetNamespaceName((INamedTypeSymbol)symbol) + "." + Utils.GetClassName((INamedTypeSymbol)symbol)
                    + "." + methodName;
            else if (symbol != null && symbol is IArrayTypeSymbol)
                key = Utils.GetNamespaceName(((IArrayTypeSymbol)symbol).ElementType) + "." + Utils.GetClassName(((IArrayTypeSymbol)symbol).ElementType)
                    + "." + methodName + "[]";
            else if (symbol != null && symbol.Kind == SymbolKind.DynamicType)
                key = "dynamic";
            else if (symbol != null && symbol.Kind == SymbolKind.TypeParameter)
                key = symbol.Name + "." + methodName;
            else if (symbol == null && methodName != null)
                key = methodName;
            else
                throw new NotImplementedException();
            return key;
        }

        public void PrintCallsInformation()
        {
            StringBuilder sbCalls = new StringBuilder();
            StringBuilder sbCallbacks = new StringBuilder();
            foreach(var c in CallInfo)
            {
                var s = c.Value.CustomToString();
                sbCalls.AppendLine(s);
                foreach (var cb in c.Value.callbacks)
                    sbCallbacks.AppendLine(s + "|" + cb.Value.CustomToString());
            }
            Console.WriteLine("CALLS:");
            Console.WriteLine(sbCalls.ToString());
            Console.WriteLine("CALLBACKS:");
            Console.WriteLine(sbCallbacks.ToString());
        }
        public void SaveCallsInformation(string methodsFile, string callbacksFile)
        {
            StringBuilder sbCalls = new StringBuilder();
            StringBuilder sbCallbacks = new StringBuilder();
            foreach (var c in CallInfo)
            {
                var s = c.Value.CustomToString();
                sbCalls.AppendLine(s);
                foreach (var cb in c.Value.callbacks)
                    sbCallbacks.AppendLine(s + "|" + cb.Value.CustomToString());
            }
            if (!string.IsNullOrWhiteSpace(methodsFile))
                System.IO.File.WriteAllText(methodsFile, sbCalls.ToString());
            if (!string.IsNullOrWhiteSpace(callbacksFile))
                System.IO.File.WriteAllText(callbacksFile, sbCallbacks.ToString());
        }
    }
}
