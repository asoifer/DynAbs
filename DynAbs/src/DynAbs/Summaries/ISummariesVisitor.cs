﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.6.6
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\alexd\Desktop\Slicer\DynAbs\src\DynAbs.Summaries\Summaries.g4 by ANTLR 4.6.6

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace DynAbs.Summaries
{
	using Antlr4.Runtime.Misc;
	using Antlr4.Runtime.Tree;
	using IToken = Antlr4.Runtime.IToken;

	/// <summary>
	/// This interface defines a complete generic visitor for a parse tree produced
	/// by <see cref="SummariesParser"/>.
	/// </summary>
	/// <typeparam name="Result">The return type of the visit operation.</typeparam>
	[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.6.6")]
	[System.CLSCompliant(false)]
	public interface ISummariesVisitor<Result> : IParseTreeVisitor<Result>
	{
		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.s"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitS([NotNull] SummariesParser.SContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.rv"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitRv([NotNull] SummariesParser.RvContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.ro"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitRo([NotNull] SummariesParser.RoContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.r"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitR([NotNull] SummariesParser.RContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.w"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitW([NotNull] SummariesParser.WContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.cn"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitCn([NotNull] SummariesParser.CnContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.null"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitNull([NotNull] SummariesParser.NullContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.fresh"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitFresh([NotNull] SummariesParser.FreshContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.isIn"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitIsIn([NotNull] SummariesParser.IsInContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.singleMany"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitSingleMany([NotNull] SummariesParser.SingleManyContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.single"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitSingle([NotNull] SummariesParser.SingleContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.many"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitMany([NotNull] SummariesParser.ManyContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.types"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitTypes([NotNull] SummariesParser.TypesContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.type"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitType([NotNull] SummariesParser.TypeContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.kinds"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitKinds([NotNull] SummariesParser.KindsContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.kind"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitKind([NotNull] SummariesParser.KindContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.words"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitWords([NotNull] SummariesParser.WordsContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.elementType"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitElementType([NotNull] SummariesParser.ElementTypeContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.parametricType"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitParametricType([NotNull] SummariesParser.ParametricTypeContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.field"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitField([NotNull] SummariesParser.FieldContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.anyField"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitAnyField([NotNull] SummariesParser.AnyFieldContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.mc"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitMc([NotNull] SummariesParser.McContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.c"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitC([NotNull] SummariesParser.CContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.metf"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitMetf([NotNull] SummariesParser.MetfContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.etf"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitEtf([NotNull] SummariesParser.EtfContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.fa"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitFa([NotNull] SummariesParser.FaContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.met"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitMet([NotNull] SummariesParser.MetContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.et"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitEt([NotNull] SummariesParser.EtContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.bf"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitBf([NotNull] SummariesParser.BfContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.b"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitB([NotNull] SummariesParser.BContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.f"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitF([NotNull] SummariesParser.FContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.filter"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitFilter([NotNull] SummariesParser.FilterContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.filterT"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitFilterT([NotNull] SummariesParser.FilterTContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.filterUT"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitFilterUT([NotNull] SummariesParser.FilterUTContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.filterOT"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitFilterOT([NotNull] SummariesParser.FilterOTContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.filterOK"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitFilterOK([NotNull] SummariesParser.FilterOKContext context);

		/// <summary>
		/// Visit a parse tree produced by <see cref="SummariesParser.number"/>.
		/// </summary>
		/// <param name="context">The parse tree.</param>
		/// <return>The visitor result.</return>
		Result VisitNumber([NotNull] SummariesParser.NumberContext context);
	}
} // namespace DynAbs.Summaries