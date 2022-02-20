//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.8
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from ./ExcelRange.g4 by ANTLR 4.8

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.8")]
[System.CLSCompliant(false)]
public partial class ExcelRangeParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, COLUMN=2, ROW=3;
	public const int
		RULE_range = 0, RULE_fullrange = 1, RULE_startcell = 2, RULE_startrowwithcols = 3;
	public static readonly string[] ruleNames = {
		"range", "fullrange", "startcell", "startrowwithcols"
	};

	private static readonly string[] _LiteralNames = {
		null, "':'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, "COLUMN", "ROW"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "ExcelRange.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static ExcelRangeParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

		public ExcelRangeParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public ExcelRangeParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	public partial class RangeContext : ParserRuleContext {
		public FullrangeContext fullrange() {
			return GetRuleContext<FullrangeContext>(0);
		}
		public StartcellContext startcell() {
			return GetRuleContext<StartcellContext>(0);
		}
		public StartrowwithcolsContext startrowwithcols() {
			return GetRuleContext<StartrowwithcolsContext>(0);
		}
		public RangeContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_range; } }
		public override void EnterRule(IParseTreeListener listener) {
			IExcelRangeListener typedListener = listener as IExcelRangeListener;
			if (typedListener != null) typedListener.EnterRange(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IExcelRangeListener typedListener = listener as IExcelRangeListener;
			if (typedListener != null) typedListener.ExitRange(this);
		}
	}

	[RuleVersion(0)]
	public RangeContext range() {
		RangeContext _localctx = new RangeContext(Context, State);
		EnterRule(_localctx, 0, RULE_range);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 11;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,0,Context) ) {
			case 1:
				{
				State = 8; fullrange();
				}
				break;
			case 2:
				{
				State = 9; startcell();
				}
				break;
			case 3:
				{
				State = 10; startrowwithcols();
				}
				break;
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class FullrangeContext : ParserRuleContext {
		public ITerminalNode[] COLUMN() { return GetTokens(ExcelRangeParser.COLUMN); }
		public ITerminalNode COLUMN(int i) {
			return GetToken(ExcelRangeParser.COLUMN, i);
		}
		public ITerminalNode[] ROW() { return GetTokens(ExcelRangeParser.ROW); }
		public ITerminalNode ROW(int i) {
			return GetToken(ExcelRangeParser.ROW, i);
		}
		public FullrangeContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_fullrange; } }
		public override void EnterRule(IParseTreeListener listener) {
			IExcelRangeListener typedListener = listener as IExcelRangeListener;
			if (typedListener != null) typedListener.EnterFullrange(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IExcelRangeListener typedListener = listener as IExcelRangeListener;
			if (typedListener != null) typedListener.ExitFullrange(this);
		}
	}

	[RuleVersion(0)]
	public FullrangeContext fullrange() {
		FullrangeContext _localctx = new FullrangeContext(Context, State);
		EnterRule(_localctx, 2, RULE_fullrange);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 13; Match(COLUMN);
			State = 14; Match(ROW);
			State = 15; Match(T__0);
			State = 16; Match(COLUMN);
			State = 17; Match(ROW);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class StartcellContext : ParserRuleContext {
		public ITerminalNode COLUMN() { return GetToken(ExcelRangeParser.COLUMN, 0); }
		public ITerminalNode ROW() { return GetToken(ExcelRangeParser.ROW, 0); }
		public StartcellContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_startcell; } }
		public override void EnterRule(IParseTreeListener listener) {
			IExcelRangeListener typedListener = listener as IExcelRangeListener;
			if (typedListener != null) typedListener.EnterStartcell(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IExcelRangeListener typedListener = listener as IExcelRangeListener;
			if (typedListener != null) typedListener.ExitStartcell(this);
		}
	}

	[RuleVersion(0)]
	public StartcellContext startcell() {
		StartcellContext _localctx = new StartcellContext(Context, State);
		EnterRule(_localctx, 4, RULE_startcell);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 19; Match(COLUMN);
			State = 20; Match(ROW);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class StartrowwithcolsContext : ParserRuleContext {
		public ITerminalNode ROW() { return GetToken(ExcelRangeParser.ROW, 0); }
		public ITerminalNode[] COLUMN() { return GetTokens(ExcelRangeParser.COLUMN); }
		public ITerminalNode COLUMN(int i) {
			return GetToken(ExcelRangeParser.COLUMN, i);
		}
		public StartrowwithcolsContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_startrowwithcols; } }
		public override void EnterRule(IParseTreeListener listener) {
			IExcelRangeListener typedListener = listener as IExcelRangeListener;
			if (typedListener != null) typedListener.EnterStartrowwithcols(this);
		}
		public override void ExitRule(IParseTreeListener listener) {
			IExcelRangeListener typedListener = listener as IExcelRangeListener;
			if (typedListener != null) typedListener.ExitStartrowwithcols(this);
		}
	}

	[RuleVersion(0)]
	public StartrowwithcolsContext startrowwithcols() {
		StartrowwithcolsContext _localctx = new StartrowwithcolsContext(Context, State);
		EnterRule(_localctx, 6, RULE_startrowwithcols);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 22; Match(ROW);
			State = 23; Match(T__0);
			State = 24; Match(COLUMN);
			State = 25; Match(T__0);
			State = 26; Match(COLUMN);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x3', '\x5', '\x1F', '\x4', '\x2', '\t', '\x2', '\x4', '\x3', 
		'\t', '\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', '\x5', '\x3', 
		'\x2', '\x3', '\x2', '\x3', '\x2', '\x5', '\x2', '\xE', '\n', '\x2', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x5', '\x3', 
		'\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', 
		'\x5', '\x2', '\x2', '\x6', '\x2', '\x4', '\x6', '\b', '\x2', '\x2', '\x2', 
		'\x1C', '\x2', '\r', '\x3', '\x2', '\x2', '\x2', '\x4', '\xF', '\x3', 
		'\x2', '\x2', '\x2', '\x6', '\x15', '\x3', '\x2', '\x2', '\x2', '\b', 
		'\x18', '\x3', '\x2', '\x2', '\x2', '\n', '\xE', '\x5', '\x4', '\x3', 
		'\x2', '\v', '\xE', '\x5', '\x6', '\x4', '\x2', '\f', '\xE', '\x5', '\b', 
		'\x5', '\x2', '\r', '\n', '\x3', '\x2', '\x2', '\x2', '\r', '\v', '\x3', 
		'\x2', '\x2', '\x2', '\r', '\f', '\x3', '\x2', '\x2', '\x2', '\xE', '\x3', 
		'\x3', '\x2', '\x2', '\x2', '\xF', '\x10', '\a', '\x4', '\x2', '\x2', 
		'\x10', '\x11', '\a', '\x5', '\x2', '\x2', '\x11', '\x12', '\a', '\x3', 
		'\x2', '\x2', '\x12', '\x13', '\a', '\x4', '\x2', '\x2', '\x13', '\x14', 
		'\a', '\x5', '\x2', '\x2', '\x14', '\x5', '\x3', '\x2', '\x2', '\x2', 
		'\x15', '\x16', '\a', '\x4', '\x2', '\x2', '\x16', '\x17', '\a', '\x5', 
		'\x2', '\x2', '\x17', '\a', '\x3', '\x2', '\x2', '\x2', '\x18', '\x19', 
		'\a', '\x5', '\x2', '\x2', '\x19', '\x1A', '\a', '\x3', '\x2', '\x2', 
		'\x1A', '\x1B', '\a', '\x4', '\x2', '\x2', '\x1B', '\x1C', '\a', '\x3', 
		'\x2', '\x2', '\x1C', '\x1D', '\a', '\x4', '\x2', '\x2', '\x1D', '\t', 
		'\x3', '\x2', '\x2', '\x2', '\x3', '\r',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
