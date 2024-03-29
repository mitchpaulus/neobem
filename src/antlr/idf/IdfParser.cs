//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from ./Idf.g4 by ANTLR 4.13.1

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

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
public partial class IdfParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		COMMENT=1, FIELDSEP=2, TERMINATOR=3, IDFFIELD=4, WS=5;
	public const int
		RULE_idfFile = 0, RULE_idfObject = 1, RULE_idfHeader = 2;
	public static readonly string[] ruleNames = {
		"idfFile", "idfObject", "idfHeader"
	};

	private static readonly string[] _LiteralNames = {
		null, null, "','", "';'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "COMMENT", "FIELDSEP", "TERMINATOR", "IDFFIELD", "WS"
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

	public override string GrammarFileName { get { return "Idf.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static IdfParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

		public IdfParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public IdfParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	public partial class IdfFileContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Eof() { return GetToken(IdfParser.Eof, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public IdfObjectContext[] idfObject() {
			return GetRuleContexts<IdfObjectContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public IdfObjectContext idfObject(int i) {
			return GetRuleContext<IdfObjectContext>(i);
		}
		public IdfFileContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_idfFile; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IIdfListener typedListener = listener as IIdfListener;
			if (typedListener != null) typedListener.EnterIdfFile(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IIdfListener typedListener = listener as IIdfListener;
			if (typedListener != null) typedListener.ExitIdfFile(this);
		}
	}

	[RuleVersion(0)]
	public IdfFileContext idfFile() {
		IdfFileContext _localctx = new IdfFileContext(Context, State);
		EnterRule(_localctx, 0, RULE_idfFile);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 9;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==IDFFIELD) {
				{
				{
				State = 6;
				idfObject();
				}
				}
				State = 11;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 12;
			Match(Eof);
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

	public partial class IdfObjectContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public IdfHeaderContext idfHeader() {
			return GetRuleContext<IdfHeaderContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode[] FIELDSEP() { return GetTokens(IdfParser.FIELDSEP); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode FIELDSEP(int i) {
			return GetToken(IdfParser.FIELDSEP, i);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode TERMINATOR() { return GetToken(IdfParser.TERMINATOR, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode[] IDFFIELD() { return GetTokens(IdfParser.IDFFIELD); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode IDFFIELD(int i) {
			return GetToken(IdfParser.IDFFIELD, i);
		}
		public IdfObjectContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_idfObject; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IIdfListener typedListener = listener as IIdfListener;
			if (typedListener != null) typedListener.EnterIdfObject(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IIdfListener typedListener = listener as IIdfListener;
			if (typedListener != null) typedListener.ExitIdfObject(this);
		}
	}

	[RuleVersion(0)]
	public IdfObjectContext idfObject() {
		IdfObjectContext _localctx = new IdfObjectContext(Context, State);
		EnterRule(_localctx, 2, RULE_idfObject);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 14;
			idfHeader();
			State = 15;
			Match(FIELDSEP);
			State = 17;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			if (_la==IDFFIELD) {
				{
				State = 16;
				Match(IDFFIELD);
				}
			}

			State = 25;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==FIELDSEP) {
				{
				{
				State = 19;
				Match(FIELDSEP);
				State = 21;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				if (_la==IDFFIELD) {
					{
					State = 20;
					Match(IDFFIELD);
					}
				}

				}
				}
				State = 27;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 28;
			Match(TERMINATOR);
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

	public partial class IdfHeaderContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode IDFFIELD() { return GetToken(IdfParser.IDFFIELD, 0); }
		public IdfHeaderContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_idfHeader; } }
		[System.Diagnostics.DebuggerNonUserCode]
		public override void EnterRule(IParseTreeListener listener) {
			IIdfListener typedListener = listener as IIdfListener;
			if (typedListener != null) typedListener.EnterIdfHeader(this);
		}
		[System.Diagnostics.DebuggerNonUserCode]
		public override void ExitRule(IParseTreeListener listener) {
			IIdfListener typedListener = listener as IIdfListener;
			if (typedListener != null) typedListener.ExitIdfHeader(this);
		}
	}

	[RuleVersion(0)]
	public IdfHeaderContext idfHeader() {
		IdfHeaderContext _localctx = new IdfHeaderContext(Context, State);
		EnterRule(_localctx, 4, RULE_idfHeader);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 30;
			Match(IDFFIELD);
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

	private static int[] _serializedATN = {
		4,1,5,33,2,0,7,0,2,1,7,1,2,2,7,2,1,0,5,0,8,8,0,10,0,12,0,11,9,0,1,0,1,
		0,1,1,1,1,1,1,3,1,18,8,1,1,1,1,1,3,1,22,8,1,5,1,24,8,1,10,1,12,1,27,9,
		1,1,1,1,1,1,2,1,2,1,2,0,0,3,0,2,4,0,0,33,0,9,1,0,0,0,2,14,1,0,0,0,4,30,
		1,0,0,0,6,8,3,2,1,0,7,6,1,0,0,0,8,11,1,0,0,0,9,7,1,0,0,0,9,10,1,0,0,0,
		10,12,1,0,0,0,11,9,1,0,0,0,12,13,5,0,0,1,13,1,1,0,0,0,14,15,3,4,2,0,15,
		17,5,2,0,0,16,18,5,4,0,0,17,16,1,0,0,0,17,18,1,0,0,0,18,25,1,0,0,0,19,
		21,5,2,0,0,20,22,5,4,0,0,21,20,1,0,0,0,21,22,1,0,0,0,22,24,1,0,0,0,23,
		19,1,0,0,0,24,27,1,0,0,0,25,23,1,0,0,0,25,26,1,0,0,0,26,28,1,0,0,0,27,
		25,1,0,0,0,28,29,5,3,0,0,29,3,1,0,0,0,30,31,5,4,0,0,31,5,1,0,0,0,4,9,17,
		21,25
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
