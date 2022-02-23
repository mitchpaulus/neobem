//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.3
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from NeobemParser.g4 by ANTLR 4.9.3

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="NeobemParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.3")]
[System.CLSCompliant(false)]
public interface INeobemParserListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.variable_declaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariable_declaration([NotNull] NeobemParser.Variable_declarationContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.variable_declaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariable_declaration([NotNull] NeobemParser.Variable_declarationContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>InlineTable</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterInlineTable([NotNull] NeobemParser.InlineTableContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>InlineTable</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitInlineTable([NotNull] NeobemParser.InlineTableContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>FunctionExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunctionExp([NotNull] NeobemParser.FunctionExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>FunctionExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunctionExp([NotNull] NeobemParser.FunctionExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ObjExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterObjExp([NotNull] NeobemParser.ObjExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ObjExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitObjExp([NotNull] NeobemParser.ObjExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ParensExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterParensExp([NotNull] NeobemParser.ParensExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ParensExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitParensExp([NotNull] NeobemParser.ParensExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>StringExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStringExp([NotNull] NeobemParser.StringExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>StringExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStringExp([NotNull] NeobemParser.StringExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>LambdaExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLambdaExp([NotNull] NeobemParser.LambdaExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>LambdaExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLambdaExp([NotNull] NeobemParser.LambdaExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>MemberAccessExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMemberAccessExp([NotNull] NeobemParser.MemberAccessExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>MemberAccessExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMemberAccessExp([NotNull] NeobemParser.MemberAccessExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>AddSub</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAddSub([NotNull] NeobemParser.AddSubContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>AddSub</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAddSub([NotNull] NeobemParser.AddSubContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>NumericExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNumericExp([NotNull] NeobemParser.NumericExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>NumericExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNumericExp([NotNull] NeobemParser.NumericExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>LetBindingExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLetBindingExp([NotNull] NeobemParser.LetBindingExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>LetBindingExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLetBindingExp([NotNull] NeobemParser.LetBindingExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>BooleanLiteralFalseExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBooleanLiteralFalseExp([NotNull] NeobemParser.BooleanLiteralFalseExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>BooleanLiteralFalseExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBooleanLiteralFalseExp([NotNull] NeobemParser.BooleanLiteralFalseExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>RangeExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRangeExp([NotNull] NeobemParser.RangeExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>RangeExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRangeExp([NotNull] NeobemParser.RangeExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ListExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterListExp([NotNull] NeobemParser.ListExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ListExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitListExp([NotNull] NeobemParser.ListExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>MapPipeFilterExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMapPipeFilterExp([NotNull] NeobemParser.MapPipeFilterExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>MapPipeFilterExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMapPipeFilterExp([NotNull] NeobemParser.MapPipeFilterExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>BooleanLiteralTrueExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBooleanLiteralTrueExp([NotNull] NeobemParser.BooleanLiteralTrueExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>BooleanLiteralTrueExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBooleanLiteralTrueExp([NotNull] NeobemParser.BooleanLiteralTrueExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>VariableExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariableExp([NotNull] NeobemParser.VariableExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>VariableExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariableExp([NotNull] NeobemParser.VariableExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>IfExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIfExp([NotNull] NeobemParser.IfExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>IfExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIfExp([NotNull] NeobemParser.IfExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>BclExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBclExp([NotNull] NeobemParser.BclExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>BclExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBclExp([NotNull] NeobemParser.BclExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>BooleanExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBooleanExp([NotNull] NeobemParser.BooleanExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>BooleanExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBooleanExp([NotNull] NeobemParser.BooleanExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Exponientiate</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExponientiate([NotNull] NeobemParser.ExponientiateContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Exponientiate</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExponientiate([NotNull] NeobemParser.ExponientiateContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>LogicExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLogicExp([NotNull] NeobemParser.LogicExpContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>LogicExp</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLogicExp([NotNull] NeobemParser.LogicExpContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>MultDivide</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMultDivide([NotNull] NeobemParser.MultDivideContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>MultDivide</c>
	/// labeled alternative in <see cref="NeobemParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMultDivide([NotNull] NeobemParser.MultDivideContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.functional_operator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunctional_operator([NotNull] NeobemParser.Functional_operatorContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.functional_operator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunctional_operator([NotNull] NeobemParser.Functional_operatorContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.function_application"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunction_application([NotNull] NeobemParser.Function_applicationContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.function_application"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunction_application([NotNull] NeobemParser.Function_applicationContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.function_parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunction_parameter([NotNull] NeobemParser.Function_parameterContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.function_parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunction_parameter([NotNull] NeobemParser.Function_parameterContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.boolean_exp_operator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBoolean_exp_operator([NotNull] NeobemParser.Boolean_exp_operatorContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.boolean_exp_operator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBoolean_exp_operator([NotNull] NeobemParser.Boolean_exp_operatorContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.if_exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIf_exp([NotNull] NeobemParser.If_expContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.if_exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIf_exp([NotNull] NeobemParser.If_expContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.lambda_def"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLambda_def([NotNull] NeobemParser.Lambda_defContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.lambda_def"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLambda_def([NotNull] NeobemParser.Lambda_defContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.return_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterReturn_statement([NotNull] NeobemParser.Return_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.return_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitReturn_statement([NotNull] NeobemParser.Return_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.idfplus_object"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIdfplus_object([NotNull] NeobemParser.Idfplus_objectContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.idfplus_object"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIdfplus_object([NotNull] NeobemParser.Idfplus_objectContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.idfplus_object_property_def"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIdfplus_object_property_def([NotNull] NeobemParser.Idfplus_object_property_defContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.idfplus_object_property_def"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIdfplus_object_property_def([NotNull] NeobemParser.Idfplus_object_property_defContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.list"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterList([NotNull] NeobemParser.ListContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.list"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitList([NotNull] NeobemParser.ListContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.import_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterImport_statement([NotNull] NeobemParser.Import_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.import_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitImport_statement([NotNull] NeobemParser.Import_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>AsOption</c>
	/// labeled alternative in <see cref="NeobemParser.import_option"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAsOption([NotNull] NeobemParser.AsOptionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>AsOption</c>
	/// labeled alternative in <see cref="NeobemParser.import_option"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAsOption([NotNull] NeobemParser.AsOptionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>OnlyOption</c>
	/// labeled alternative in <see cref="NeobemParser.import_option"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterOnlyOption([NotNull] NeobemParser.OnlyOptionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>OnlyOption</c>
	/// labeled alternative in <see cref="NeobemParser.import_option"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitOnlyOption([NotNull] NeobemParser.OnlyOptionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>NotOption</c>
	/// labeled alternative in <see cref="NeobemParser.import_option"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNotOption([NotNull] NeobemParser.NotOptionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>NotOption</c>
	/// labeled alternative in <see cref="NeobemParser.import_option"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNotOption([NotNull] NeobemParser.NotOptionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.export_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExport_statement([NotNull] NeobemParser.Export_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.export_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExport_statement([NotNull] NeobemParser.Export_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.print_statment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPrint_statment([NotNull] NeobemParser.Print_statmentContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.print_statment"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPrint_statment([NotNull] NeobemParser.Print_statmentContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.log_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLog_statement([NotNull] NeobemParser.Log_statementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.log_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLog_statement([NotNull] NeobemParser.Log_statementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.inline_table"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterInline_table([NotNull] NeobemParser.Inline_tableContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.inline_table"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitInline_table([NotNull] NeobemParser.Inline_tableContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.inline_table_header"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterInline_table_header([NotNull] NeobemParser.Inline_table_headerContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.inline_table_header"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitInline_table_header([NotNull] NeobemParser.Inline_table_headerContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.inline_table_header_separator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterInline_table_header_separator([NotNull] NeobemParser.Inline_table_header_separatorContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.inline_table_header_separator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitInline_table_header_separator([NotNull] NeobemParser.Inline_table_header_separatorContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.inline_table_data_row"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterInline_table_data_row([NotNull] NeobemParser.Inline_table_data_rowContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.inline_table_data_row"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitInline_table_data_row([NotNull] NeobemParser.Inline_table_data_rowContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>FunctionIdfComment</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunctionIdfComment([NotNull] NeobemParser.FunctionIdfCommentContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>FunctionIdfComment</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunctionIdfComment([NotNull] NeobemParser.FunctionIdfCommentContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>FunctionObjectDeclaration</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunctionObjectDeclaration([NotNull] NeobemParser.FunctionObjectDeclarationContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>FunctionObjectDeclaration</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunctionObjectDeclaration([NotNull] NeobemParser.FunctionObjectDeclarationContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>FunctionDoe2ObjectDeclaration</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunctionDoe2ObjectDeclaration([NotNull] NeobemParser.FunctionDoe2ObjectDeclarationContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>FunctionDoe2ObjectDeclaration</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunctionDoe2ObjectDeclaration([NotNull] NeobemParser.FunctionDoe2ObjectDeclarationContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>FunctionVariableDeclaration</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunctionVariableDeclaration([NotNull] NeobemParser.FunctionVariableDeclarationContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>FunctionVariableDeclaration</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunctionVariableDeclaration([NotNull] NeobemParser.FunctionVariableDeclarationContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>FunctionPrintStatement</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunctionPrintStatement([NotNull] NeobemParser.FunctionPrintStatementContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>FunctionPrintStatement</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunctionPrintStatement([NotNull] NeobemParser.FunctionPrintStatementContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ReturnStatement</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterReturnStatement([NotNull] NeobemParser.ReturnStatementContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ReturnStatement</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitReturnStatement([NotNull] NeobemParser.ReturnStatementContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>FunctionLogStatement</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunctionLogStatement([NotNull] NeobemParser.FunctionLogStatementContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>FunctionLogStatement</c>
	/// labeled alternative in <see cref="NeobemParser.function_statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunctionLogStatement([NotNull] NeobemParser.FunctionLogStatementContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>IdfComment</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIdfComment([NotNull] NeobemParser.IdfCommentContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>IdfComment</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIdfComment([NotNull] NeobemParser.IdfCommentContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Doe2Comment</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDoe2Comment([NotNull] NeobemParser.Doe2CommentContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Doe2Comment</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDoe2Comment([NotNull] NeobemParser.Doe2CommentContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ObjectDeclaration</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterObjectDeclaration([NotNull] NeobemParser.ObjectDeclarationContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ObjectDeclaration</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitObjectDeclaration([NotNull] NeobemParser.ObjectDeclarationContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Doe2ObjectDeclaration</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDoe2ObjectDeclaration([NotNull] NeobemParser.Doe2ObjectDeclarationContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Doe2ObjectDeclaration</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDoe2ObjectDeclaration([NotNull] NeobemParser.Doe2ObjectDeclarationContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>VariableDeclaration</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariableDeclaration([NotNull] NeobemParser.VariableDeclarationContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>VariableDeclaration</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariableDeclaration([NotNull] NeobemParser.VariableDeclarationContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ImportStatement</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterImportStatement([NotNull] NeobemParser.ImportStatementContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ImportStatement</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitImportStatement([NotNull] NeobemParser.ImportStatementContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ExportStatment</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExportStatment([NotNull] NeobemParser.ExportStatmentContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ExportStatment</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExportStatment([NotNull] NeobemParser.ExportStatmentContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>PrintStatment</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPrintStatment([NotNull] NeobemParser.PrintStatmentContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>PrintStatment</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPrintStatment([NotNull] NeobemParser.PrintStatmentContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>LogStatement</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLogStatement([NotNull] NeobemParser.LogStatementContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>LogStatement</c>
	/// labeled alternative in <see cref="NeobemParser.base_idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLogStatement([NotNull] NeobemParser.LogStatementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.let_binding"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLet_binding([NotNull] NeobemParser.Let_bindingContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.let_binding"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLet_binding([NotNull] NeobemParser.Let_bindingContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.let_expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLet_expression([NotNull] NeobemParser.Let_expressionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.let_expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLet_expression([NotNull] NeobemParser.Let_expressionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.object"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterObject([NotNull] NeobemParser.ObjectContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.object"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitObject([NotNull] NeobemParser.ObjectContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.doe2object"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDoe2object([NotNull] NeobemParser.Doe2objectContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.doe2object"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDoe2object([NotNull] NeobemParser.Doe2objectContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.doe2word"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDoe2word([NotNull] NeobemParser.Doe2wordContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.doe2word"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDoe2word([NotNull] NeobemParser.Doe2wordContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.doe2_list_item"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDoe2_list_item([NotNull] NeobemParser.Doe2_list_itemContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.doe2_list_item"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDoe2_list_item([NotNull] NeobemParser.Doe2_list_itemContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.doe2list"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDoe2list([NotNull] NeobemParser.Doe2listContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.doe2list"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDoe2list([NotNull] NeobemParser.Doe2listContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="NeobemParser.idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIdf([NotNull] NeobemParser.IdfContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="NeobemParser.idf"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIdf([NotNull] NeobemParser.IdfContext context);
}
