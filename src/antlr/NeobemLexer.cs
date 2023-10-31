//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from NeobemLexer.g4 by ANTLR 4.13.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using src;
using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
public partial class NeobemLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		EQUALS=1, LPAREN=2, RPAREN=3, CARET=4, MULTOP=5, DIVIDEOP=6, PLUSOP=7, 
		MINUSOP=8, LESSTHAN=9, GREATERTHAN=10, LESS_THAN_OR_EQUAL_TO=11, GREATER_THAN_OR_EQUAL_TO=12, 
		EQUAL_TO=13, NOT_EQUAL_TO=14, MAP_OPERATOR=15, FILTER_OPERATOR=16, PIPE_OPERATOR=17, 
		AND_OP=18, OR_OP=19, IF=20, THEN=21, ELSE=22, FUNCTION_BEGIN=23, LCURLY=24, 
		RCURLY=25, RETURN=26, LSQUARE=27, RSQUARE=28, IMPORT=29, AS=30, ONLY=31, 
		NOT=32, EXPORT=33, PRINT=34, LOG=35, LET=36, IN=37, RANGE_OPERATOR=38, 
		MEMBER_ACCESS=39, STRUCT_SEP=40, COMMA=41, BCL_ID=42, UUID=43, INLINE_TABLE_BEGIN_END_SEP=44, 
		INLINE_TABLE_COL_SEP=45, BOOLEAN_LITERAL_TRUE=46, BOOLEAN_LITERAL_FALSE=47, 
		IDENTIFIER=48, COMMENT=49, DOE2COMMENT=50, NEOBEM_COMMENT=51, NUMERIC=52, 
		STRING=53, OBJECT_TYPE=54, DOE2IDENTIFIER=55, DOE2STRING_UNAME=56, WS=57, 
		FIELD=58, FIELD_SEP=59, OBJECT_COMMENT=60, OBJECT_TERMINATOR=61, OBJECT_WS=62, 
		DOE2_LIST_START=63, DOE2_LIST_END=64, DOE2_OBJECT_COMMENT=65, DOE2_NEOBEM_COMMENT=66, 
		DOE2_STRING=67, DOE2_LITERAL=68, DOE2_TERMINATOR=69, DOE2_FIELD_SEP=70, 
		DOE2_FIELD=71;
	public const int
		IDFOBJECT=1, DOE2OBJECT=2;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE", "IDFOBJECT", "DOE2OBJECT"
	};

	public static readonly string[] ruleNames = {
		"EQUALS", "LPAREN", "RPAREN", "CARET", "MULTOP", "DIVIDEOP", "PLUSOP", 
		"MINUSOP", "LESSTHAN", "GREATERTHAN", "LESS_THAN_OR_EQUAL_TO", "GREATER_THAN_OR_EQUAL_TO", 
		"EQUAL_TO", "NOT_EQUAL_TO", "MAP_OPERATOR", "FILTER_OPERATOR", "PIPE_OPERATOR", 
		"AND_OP", "OR_OP", "IF", "THEN", "ELSE", "FUNCTION_BEGIN", "LCURLY", "RCURLY", 
		"RETURN", "LSQUARE", "RSQUARE", "IMPORT", "AS", "ONLY", "NOT", "EXPORT", 
		"PRINT", "LOG", "LET", "IN", "RANGE_OPERATOR", "MEMBER_ACCESS", "STRUCT_SEP", 
		"COMMA", "BCL_ID", "HEX_CHAR", "UUID", "INLINE_TABLE_BEGIN_END_SEP", "INLINE_TABLE_COL_SEP", 
		"BOOLEAN_LITERAL_TRUE", "BOOLEAN_LITERAL_FALSE", "IDENTIFIER", "COMMENT", 
		"DOE2COMMENT", "NEOBEM_COMMENT", "NUMERIC", "STRING", "OBJECT_TYPE", "DOE2IDENTIFIER", 
		"DOE2STRING_UNAME", "WS", "FIELD", "FIELD_SEP", "OBJECT_COMMENT", "OBJECT_TERMINATOR", 
		"OBJECT_WS", "DOE2_LIST_START", "DOE2_LIST_END", "DOE2_OBJECT_COMMENT", 
		"DOE2_NEOBEM_COMMENT", "DOE2_STRING", "DOE2_LITERAL", "DOE2_TERMINATOR", 
		"DOE2_FIELD_SEP", "DOE2_FIELD"
	};

	public FileType FileType = FileType.Idf;

	public NeobemLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public NeobemLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'='", null, null, "'^'", "'*'", "'/'", "'+'", "'-'", "'<'", "'>'", 
		"'<='", "'>='", "'=='", "'!='", "'|='", "'|>'", "'->'", "'and'", "'or'", 
		"'if'", "'then'", "'else'", null, "'{'", "'}'", "'return'", "'['", "']'", 
		"'import'", "'as'", "'only'", "'not'", "'export'", "'print'", "'log'", 
		"'let'", "'in'", null, "'.'", "':'", "','", "'bcl:'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "EQUALS", "LPAREN", "RPAREN", "CARET", "MULTOP", "DIVIDEOP", "PLUSOP", 
		"MINUSOP", "LESSTHAN", "GREATERTHAN", "LESS_THAN_OR_EQUAL_TO", "GREATER_THAN_OR_EQUAL_TO", 
		"EQUAL_TO", "NOT_EQUAL_TO", "MAP_OPERATOR", "FILTER_OPERATOR", "PIPE_OPERATOR", 
		"AND_OP", "OR_OP", "IF", "THEN", "ELSE", "FUNCTION_BEGIN", "LCURLY", "RCURLY", 
		"RETURN", "LSQUARE", "RSQUARE", "IMPORT", "AS", "ONLY", "NOT", "EXPORT", 
		"PRINT", "LOG", "LET", "IN", "RANGE_OPERATOR", "MEMBER_ACCESS", "STRUCT_SEP", 
		"COMMA", "BCL_ID", "UUID", "INLINE_TABLE_BEGIN_END_SEP", "INLINE_TABLE_COL_SEP", 
		"BOOLEAN_LITERAL_TRUE", "BOOLEAN_LITERAL_FALSE", "IDENTIFIER", "COMMENT", 
		"DOE2COMMENT", "NEOBEM_COMMENT", "NUMERIC", "STRING", "OBJECT_TYPE", "DOE2IDENTIFIER", 
		"DOE2STRING_UNAME", "WS", "FIELD", "FIELD_SEP", "OBJECT_COMMENT", "OBJECT_TERMINATOR", 
		"OBJECT_WS", "DOE2_LIST_START", "DOE2_LIST_END", "DOE2_OBJECT_COMMENT", 
		"DOE2_NEOBEM_COMMENT", "DOE2_STRING", "DOE2_LITERAL", "DOE2_TERMINATOR", 
		"DOE2_FIELD_SEP", "DOE2_FIELD"
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

	public override string GrammarFileName { get { return "NeobemLexer.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static NeobemLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	public override bool Sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 54 : return OBJECT_TYPE_sempred(_localctx, predIndex);
		case 55 : return DOE2IDENTIFIER_sempred(_localctx, predIndex);
		}
		return true;
	}
	private bool OBJECT_TYPE_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0: return  FileType == FileType.Idf ;
		}
		return true;
	}
	private bool DOE2IDENTIFIER_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 1: return  FileType == FileType.Doe2 ;
		}
		return true;
	}

	private static int[] _serializedATN = {
		4,0,71,618,6,-1,6,-1,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,
		7,5,2,6,7,6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,
		7,13,2,14,7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,
		7,20,2,21,7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,
		7,27,2,28,7,28,2,29,7,29,2,30,7,30,2,31,7,31,2,32,7,32,2,33,7,33,2,34,
		7,34,2,35,7,35,2,36,7,36,2,37,7,37,2,38,7,38,2,39,7,39,2,40,7,40,2,41,
		7,41,2,42,7,42,2,43,7,43,2,44,7,44,2,45,7,45,2,46,7,46,2,47,7,47,2,48,
		7,48,2,49,7,49,2,50,7,50,2,51,7,51,2,52,7,52,2,53,7,53,2,54,7,54,2,55,
		7,55,2,56,7,56,2,57,7,57,2,58,7,58,2,59,7,59,2,60,7,60,2,61,7,61,2,62,
		7,62,2,63,7,63,2,64,7,64,2,65,7,65,2,66,7,66,2,67,7,67,2,68,7,68,2,69,
		7,69,2,70,7,70,2,71,7,71,1,0,1,0,1,1,1,1,1,2,1,2,1,3,1,3,1,4,1,4,1,5,1,
		5,1,6,1,6,1,7,1,7,1,8,1,8,1,9,1,9,1,10,1,10,1,10,1,11,1,11,1,11,1,12,1,
		12,1,12,1,13,1,13,1,13,1,14,1,14,1,14,1,15,1,15,1,15,1,16,1,16,1,16,1,
		17,1,17,1,17,1,17,1,18,1,18,1,18,1,19,1,19,1,19,1,20,1,20,1,20,1,20,1,
		20,1,21,1,21,1,21,1,21,1,21,1,22,1,22,1,23,1,23,1,24,1,24,1,25,1,25,1,
		25,1,25,1,25,1,25,1,25,1,26,1,26,1,27,1,27,1,28,1,28,1,28,1,28,1,28,1,
		28,1,28,1,29,1,29,1,29,1,30,1,30,1,30,1,30,1,30,1,31,1,31,1,31,1,31,1,
		32,1,32,1,32,1,32,1,32,1,32,1,32,1,33,1,33,1,33,1,33,1,33,1,33,1,34,1,
		34,1,34,1,34,1,35,1,35,1,35,1,35,1,36,1,36,1,36,1,37,1,37,1,37,1,38,1,
		38,1,39,1,39,1,40,1,40,1,41,1,41,1,41,1,41,1,41,1,42,1,42,1,43,1,43,1,
		43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,
		43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,
		43,1,43,1,43,1,43,1,43,1,43,1,43,1,44,1,44,1,44,1,44,5,44,326,8,44,10,
		44,12,44,329,9,44,1,45,1,45,1,46,1,46,1,46,1,46,1,46,3,46,338,8,46,1,47,
		1,47,1,47,1,47,1,47,1,47,3,47,346,8,47,1,48,1,48,5,48,350,8,48,10,48,12,
		48,353,9,48,1,49,1,49,5,49,357,8,49,10,49,12,49,360,9,49,1,49,3,49,363,
		8,49,1,49,1,49,1,50,1,50,5,50,369,8,50,10,50,12,50,372,9,50,1,50,3,50,
		375,8,50,1,50,1,50,1,51,1,51,5,51,381,8,51,10,51,12,51,384,9,51,1,51,3,
		51,387,8,51,1,51,1,51,1,51,1,51,1,52,3,52,394,8,52,1,52,1,52,5,52,398,
		8,52,10,52,12,52,401,9,52,1,52,3,52,404,8,52,1,52,1,52,4,52,408,8,52,11,
		52,12,52,409,3,52,412,8,52,1,52,1,52,4,52,416,8,52,11,52,12,52,417,3,52,
		420,8,52,1,52,1,52,3,52,424,8,52,1,52,4,52,427,8,52,11,52,12,52,428,3,
		52,431,8,52,1,53,1,53,5,53,435,8,53,10,53,12,53,438,9,53,1,53,1,53,1,54,
		1,54,5,54,444,8,54,10,54,12,54,447,9,54,1,54,1,54,1,54,1,54,1,55,1,55,
		1,55,1,55,5,55,457,8,55,10,55,12,55,460,9,55,1,55,5,55,463,8,55,10,55,
		12,55,466,9,55,1,55,1,55,1,55,1,55,1,56,1,56,5,56,474,8,56,10,56,12,56,
		477,9,56,1,56,1,56,1,56,1,56,1,57,4,57,484,8,57,11,57,12,57,485,1,57,1,
		57,1,58,4,58,491,8,58,11,58,12,58,492,1,59,1,59,5,59,497,8,59,10,59,12,
		59,500,9,59,1,60,1,60,5,60,504,8,60,10,60,12,60,507,9,60,1,60,3,60,510,
		8,60,1,60,1,60,1,61,1,61,5,61,516,8,61,10,61,12,61,519,9,61,1,61,1,61,
		5,61,523,8,61,10,61,12,61,526,9,61,1,61,3,61,529,8,61,1,61,3,61,532,8,
		61,1,61,3,61,535,8,61,1,61,1,61,1,62,4,62,540,8,62,11,62,12,62,541,1,62,
		1,62,1,63,1,63,1,64,1,64,1,65,1,65,5,65,552,8,65,10,65,12,65,555,9,65,
		1,65,1,65,3,65,559,8,65,1,65,3,65,562,8,65,1,66,1,66,5,66,566,8,66,10,
		66,12,66,569,9,66,1,66,3,66,572,8,66,1,66,1,66,1,66,1,66,1,67,1,67,5,67,
		580,8,67,10,67,12,67,583,9,67,1,67,1,67,1,68,1,68,5,68,589,8,68,10,68,
		12,68,592,9,68,1,68,1,68,1,69,1,69,1,69,1,69,1,69,1,70,4,70,602,8,70,11,
		70,12,70,603,1,71,1,71,1,71,5,71,609,8,71,10,71,12,71,612,9,71,1,71,4,
		71,615,8,71,11,71,12,71,616,13,358,370,382,436,458,475,505,524,553,567,
		581,590,610,0,72,3,1,5,2,7,3,9,4,11,5,13,6,15,7,17,8,19,9,21,10,23,11,
		25,12,27,13,29,14,31,15,33,16,35,17,37,18,39,19,41,20,43,21,45,22,47,23,
		49,24,51,25,53,26,55,27,57,28,59,29,61,30,63,31,65,32,67,33,69,34,71,35,
		73,36,75,37,77,38,79,39,81,40,83,41,85,42,87,0,89,43,91,44,93,45,95,46,
		97,47,99,48,101,49,103,50,105,51,107,52,109,53,111,54,113,55,115,56,117,
		57,119,58,121,59,123,60,125,61,127,62,129,63,131,64,133,65,135,66,137,
		67,139,68,141,69,143,70,145,71,3,0,1,2,17,2,0,92,92,955,955,3,0,48,57,
		65,70,97,102,5,0,45,45,95,95,9472,9472,9516,9516,9524,9524,3,0,124,124,
		9474,9474,9532,9532,1,0,97,122,4,0,48,57,64,90,95,95,97,122,1,0,49,57,
		1,0,48,57,2,0,69,69,101,101,1,0,65,90,3,0,48,58,65,90,97,122,4,0,45,45,
		48,57,65,90,97,122,3,0,9,10,13,13,32,32,6,0,10,10,13,13,33,33,36,36,44,
		44,59,59,2,0,9,9,32,32,5,0,9,10,13,13,32,32,44,44,61,61,6,0,9,10,13,13,
		32,32,40,41,44,44,61,61,662,0,3,1,0,0,0,0,5,1,0,0,0,0,7,1,0,0,0,0,9,1,
		0,0,0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,0,0,0,0,17,1,0,0,0,0,19,1,0,0,0,
		0,21,1,0,0,0,0,23,1,0,0,0,0,25,1,0,0,0,0,27,1,0,0,0,0,29,1,0,0,0,0,31,
		1,0,0,0,0,33,1,0,0,0,0,35,1,0,0,0,0,37,1,0,0,0,0,39,1,0,0,0,0,41,1,0,0,
		0,0,43,1,0,0,0,0,45,1,0,0,0,0,47,1,0,0,0,0,49,1,0,0,0,0,51,1,0,0,0,0,53,
		1,0,0,0,0,55,1,0,0,0,0,57,1,0,0,0,0,59,1,0,0,0,0,61,1,0,0,0,0,63,1,0,0,
		0,0,65,1,0,0,0,0,67,1,0,0,0,0,69,1,0,0,0,0,71,1,0,0,0,0,73,1,0,0,0,0,75,
		1,0,0,0,0,77,1,0,0,0,0,79,1,0,0,0,0,81,1,0,0,0,0,83,1,0,0,0,0,85,1,0,0,
		0,0,89,1,0,0,0,0,91,1,0,0,0,0,93,1,0,0,0,0,95,1,0,0,0,0,97,1,0,0,0,0,99,
		1,0,0,0,0,101,1,0,0,0,0,103,1,0,0,0,0,105,1,0,0,0,0,107,1,0,0,0,0,109,
		1,0,0,0,0,111,1,0,0,0,0,113,1,0,0,0,0,115,1,0,0,0,0,117,1,0,0,0,1,119,
		1,0,0,0,1,121,1,0,0,0,1,123,1,0,0,0,1,125,1,0,0,0,1,127,1,0,0,0,2,129,
		1,0,0,0,2,131,1,0,0,0,2,133,1,0,0,0,2,135,1,0,0,0,2,137,1,0,0,0,2,139,
		1,0,0,0,2,141,1,0,0,0,2,143,1,0,0,0,2,145,1,0,0,0,3,147,1,0,0,0,5,149,
		1,0,0,0,7,151,1,0,0,0,9,153,1,0,0,0,11,155,1,0,0,0,13,157,1,0,0,0,15,159,
		1,0,0,0,17,161,1,0,0,0,19,163,1,0,0,0,21,165,1,0,0,0,23,167,1,0,0,0,25,
		170,1,0,0,0,27,173,1,0,0,0,29,176,1,0,0,0,31,179,1,0,0,0,33,182,1,0,0,
		0,35,185,1,0,0,0,37,188,1,0,0,0,39,192,1,0,0,0,41,195,1,0,0,0,43,198,1,
		0,0,0,45,203,1,0,0,0,47,208,1,0,0,0,49,210,1,0,0,0,51,212,1,0,0,0,53,214,
		1,0,0,0,55,221,1,0,0,0,57,223,1,0,0,0,59,225,1,0,0,0,61,232,1,0,0,0,63,
		235,1,0,0,0,65,240,1,0,0,0,67,244,1,0,0,0,69,251,1,0,0,0,71,257,1,0,0,
		0,73,261,1,0,0,0,75,265,1,0,0,0,77,268,1,0,0,0,79,271,1,0,0,0,81,273,1,
		0,0,0,83,275,1,0,0,0,85,277,1,0,0,0,87,282,1,0,0,0,89,284,1,0,0,0,91,321,
		1,0,0,0,93,330,1,0,0,0,95,337,1,0,0,0,97,345,1,0,0,0,99,347,1,0,0,0,101,
		354,1,0,0,0,103,366,1,0,0,0,105,378,1,0,0,0,107,393,1,0,0,0,109,432,1,
		0,0,0,111,441,1,0,0,0,113,452,1,0,0,0,115,471,1,0,0,0,117,483,1,0,0,0,
		119,490,1,0,0,0,121,494,1,0,0,0,123,501,1,0,0,0,125,534,1,0,0,0,127,539,
		1,0,0,0,129,545,1,0,0,0,131,547,1,0,0,0,133,549,1,0,0,0,135,563,1,0,0,
		0,137,577,1,0,0,0,139,586,1,0,0,0,141,595,1,0,0,0,143,601,1,0,0,0,145,
		614,1,0,0,0,147,148,5,61,0,0,148,4,1,0,0,0,149,150,5,40,0,0,150,6,1,0,
		0,0,151,152,5,41,0,0,152,8,1,0,0,0,153,154,5,94,0,0,154,10,1,0,0,0,155,
		156,5,42,0,0,156,12,1,0,0,0,157,158,5,47,0,0,158,14,1,0,0,0,159,160,5,
		43,0,0,160,16,1,0,0,0,161,162,5,45,0,0,162,18,1,0,0,0,163,164,5,60,0,0,
		164,20,1,0,0,0,165,166,5,62,0,0,166,22,1,0,0,0,167,168,5,60,0,0,168,169,
		5,61,0,0,169,24,1,0,0,0,170,171,5,62,0,0,171,172,5,61,0,0,172,26,1,0,0,
		0,173,174,5,61,0,0,174,175,5,61,0,0,175,28,1,0,0,0,176,177,5,33,0,0,177,
		178,5,61,0,0,178,30,1,0,0,0,179,180,5,124,0,0,180,181,5,61,0,0,181,32,
		1,0,0,0,182,183,5,124,0,0,183,184,5,62,0,0,184,34,1,0,0,0,185,186,5,45,
		0,0,186,187,5,62,0,0,187,36,1,0,0,0,188,189,5,97,0,0,189,190,5,110,0,0,
		190,191,5,100,0,0,191,38,1,0,0,0,192,193,5,111,0,0,193,194,5,114,0,0,194,
		40,1,0,0,0,195,196,5,105,0,0,196,197,5,102,0,0,197,42,1,0,0,0,198,199,
		5,116,0,0,199,200,5,104,0,0,200,201,5,101,0,0,201,202,5,110,0,0,202,44,
		1,0,0,0,203,204,5,101,0,0,204,205,5,108,0,0,205,206,5,115,0,0,206,207,
		5,101,0,0,207,46,1,0,0,0,208,209,7,0,0,0,209,48,1,0,0,0,210,211,5,123,
		0,0,211,50,1,0,0,0,212,213,5,125,0,0,213,52,1,0,0,0,214,215,5,114,0,0,
		215,216,5,101,0,0,216,217,5,116,0,0,217,218,5,117,0,0,218,219,5,114,0,
		0,219,220,5,110,0,0,220,54,1,0,0,0,221,222,5,91,0,0,222,56,1,0,0,0,223,
		224,5,93,0,0,224,58,1,0,0,0,225,226,5,105,0,0,226,227,5,109,0,0,227,228,
		5,112,0,0,228,229,5,111,0,0,229,230,5,114,0,0,230,231,5,116,0,0,231,60,
		1,0,0,0,232,233,5,97,0,0,233,234,5,115,0,0,234,62,1,0,0,0,235,236,5,111,
		0,0,236,237,5,110,0,0,237,238,5,108,0,0,238,239,5,121,0,0,239,64,1,0,0,
		0,240,241,5,110,0,0,241,242,5,111,0,0,242,243,5,116,0,0,243,66,1,0,0,0,
		244,245,5,101,0,0,245,246,5,120,0,0,246,247,5,112,0,0,247,248,5,111,0,
		0,248,249,5,114,0,0,249,250,5,116,0,0,250,68,1,0,0,0,251,252,5,112,0,0,
		252,253,5,114,0,0,253,254,5,105,0,0,254,255,5,110,0,0,255,256,5,116,0,
		0,256,70,1,0,0,0,257,258,5,108,0,0,258,259,5,111,0,0,259,260,5,103,0,0,
		260,72,1,0,0,0,261,262,5,108,0,0,262,263,5,101,0,0,263,264,5,116,0,0,264,
		74,1,0,0,0,265,266,5,105,0,0,266,267,5,110,0,0,267,76,1,0,0,0,268,269,
		5,46,0,0,269,270,5,46,0,0,270,78,1,0,0,0,271,272,5,46,0,0,272,80,1,0,0,
		0,273,274,5,58,0,0,274,82,1,0,0,0,275,276,5,44,0,0,276,84,1,0,0,0,277,
		278,5,98,0,0,278,279,5,99,0,0,279,280,5,108,0,0,280,281,5,58,0,0,281,86,
		1,0,0,0,282,283,7,1,0,0,283,88,1,0,0,0,284,285,3,87,42,0,285,286,3,87,
		42,0,286,287,3,87,42,0,287,288,3,87,42,0,288,289,3,87,42,0,289,290,3,87,
		42,0,290,291,3,87,42,0,291,292,3,87,42,0,292,293,5,45,0,0,293,294,3,87,
		42,0,294,295,3,87,42,0,295,296,3,87,42,0,296,297,3,87,42,0,297,298,5,45,
		0,0,298,299,3,87,42,0,299,300,3,87,42,0,300,301,3,87,42,0,301,302,3,87,
		42,0,302,303,5,45,0,0,303,304,3,87,42,0,304,305,3,87,42,0,305,306,3,87,
		42,0,306,307,3,87,42,0,307,308,5,45,0,0,308,309,3,87,42,0,309,310,3,87,
		42,0,310,311,3,87,42,0,311,312,3,87,42,0,312,313,3,87,42,0,313,314,3,87,
		42,0,314,315,3,87,42,0,315,316,3,87,42,0,316,317,3,87,42,0,317,318,3,87,
		42,0,318,319,3,87,42,0,319,320,3,87,42,0,320,90,1,0,0,0,321,322,7,2,0,
		0,322,323,7,2,0,0,323,327,7,2,0,0,324,326,7,2,0,0,325,324,1,0,0,0,326,
		329,1,0,0,0,327,325,1,0,0,0,327,328,1,0,0,0,328,92,1,0,0,0,329,327,1,0,
		0,0,330,331,7,3,0,0,331,94,1,0,0,0,332,333,5,116,0,0,333,334,5,114,0,0,
		334,335,5,117,0,0,335,338,5,101,0,0,336,338,5,10003,0,0,337,332,1,0,0,
		0,337,336,1,0,0,0,338,96,1,0,0,0,339,340,5,102,0,0,340,341,5,97,0,0,341,
		342,5,108,0,0,342,343,5,115,0,0,343,346,5,101,0,0,344,346,5,10007,0,0,
		345,339,1,0,0,0,345,344,1,0,0,0,346,98,1,0,0,0,347,351,7,4,0,0,348,350,
		7,5,0,0,349,348,1,0,0,0,350,353,1,0,0,0,351,349,1,0,0,0,351,352,1,0,0,
		0,352,100,1,0,0,0,353,351,1,0,0,0,354,358,5,33,0,0,355,357,9,0,0,0,356,
		355,1,0,0,0,357,360,1,0,0,0,358,359,1,0,0,0,358,356,1,0,0,0,359,362,1,
		0,0,0,360,358,1,0,0,0,361,363,5,13,0,0,362,361,1,0,0,0,362,363,1,0,0,0,
		363,364,1,0,0,0,364,365,5,10,0,0,365,102,1,0,0,0,366,370,5,36,0,0,367,
		369,9,0,0,0,368,367,1,0,0,0,369,372,1,0,0,0,370,371,1,0,0,0,370,368,1,
		0,0,0,371,374,1,0,0,0,372,370,1,0,0,0,373,375,5,13,0,0,374,373,1,0,0,0,
		374,375,1,0,0,0,375,376,1,0,0,0,376,377,5,10,0,0,377,104,1,0,0,0,378,382,
		5,35,0,0,379,381,9,0,0,0,380,379,1,0,0,0,381,384,1,0,0,0,382,383,1,0,0,
		0,382,380,1,0,0,0,383,386,1,0,0,0,384,382,1,0,0,0,385,387,5,13,0,0,386,
		385,1,0,0,0,386,387,1,0,0,0,387,388,1,0,0,0,388,389,5,10,0,0,389,390,1,
		0,0,0,390,391,6,51,0,0,391,106,1,0,0,0,392,394,5,45,0,0,393,392,1,0,0,
		0,393,394,1,0,0,0,394,419,1,0,0,0,395,399,7,6,0,0,396,398,7,7,0,0,397,
		396,1,0,0,0,398,401,1,0,0,0,399,397,1,0,0,0,399,400,1,0,0,0,400,404,1,
		0,0,0,401,399,1,0,0,0,402,404,5,48,0,0,403,395,1,0,0,0,403,402,1,0,0,0,
		404,411,1,0,0,0,405,407,5,46,0,0,406,408,7,7,0,0,407,406,1,0,0,0,408,409,
		1,0,0,0,409,407,1,0,0,0,409,410,1,0,0,0,410,412,1,0,0,0,411,405,1,0,0,
		0,411,412,1,0,0,0,412,420,1,0,0,0,413,415,5,46,0,0,414,416,7,7,0,0,415,
		414,1,0,0,0,416,417,1,0,0,0,417,415,1,0,0,0,417,418,1,0,0,0,418,420,1,
		0,0,0,419,403,1,0,0,0,419,413,1,0,0,0,420,430,1,0,0,0,421,423,7,8,0,0,
		422,424,5,45,0,0,423,422,1,0,0,0,423,424,1,0,0,0,424,426,1,0,0,0,425,427,
		7,7,0,0,426,425,1,0,0,0,427,428,1,0,0,0,428,426,1,0,0,0,428,429,1,0,0,
		0,429,431,1,0,0,0,430,421,1,0,0,0,430,431,1,0,0,0,431,108,1,0,0,0,432,
		436,5,39,0,0,433,435,9,0,0,0,434,433,1,0,0,0,435,438,1,0,0,0,436,437,1,
		0,0,0,436,434,1,0,0,0,437,439,1,0,0,0,438,436,1,0,0,0,439,440,5,39,0,0,
		440,110,1,0,0,0,441,445,7,9,0,0,442,444,7,10,0,0,443,442,1,0,0,0,444,447,
		1,0,0,0,445,443,1,0,0,0,445,446,1,0,0,0,446,448,1,0,0,0,447,445,1,0,0,
		0,448,449,4,54,0,0,449,450,1,0,0,0,450,451,6,54,1,0,451,112,1,0,0,0,452,
		464,7,9,0,0,453,463,7,11,0,0,454,458,5,60,0,0,455,457,9,0,0,0,456,455,
		1,0,0,0,457,460,1,0,0,0,458,459,1,0,0,0,458,456,1,0,0,0,459,461,1,0,0,
		0,460,458,1,0,0,0,461,463,5,62,0,0,462,453,1,0,0,0,462,454,1,0,0,0,463,
		466,1,0,0,0,464,462,1,0,0,0,464,465,1,0,0,0,465,467,1,0,0,0,466,464,1,
		0,0,0,467,468,4,55,1,0,468,469,1,0,0,0,469,470,6,55,2,0,470,114,1,0,0,
		0,471,475,5,34,0,0,472,474,9,0,0,0,473,472,1,0,0,0,474,477,1,0,0,0,475,
		476,1,0,0,0,475,473,1,0,0,0,476,478,1,0,0,0,477,475,1,0,0,0,478,479,5,
		34,0,0,479,480,1,0,0,0,480,481,6,56,2,0,481,116,1,0,0,0,482,484,7,12,0,
		0,483,482,1,0,0,0,484,485,1,0,0,0,485,483,1,0,0,0,485,486,1,0,0,0,486,
		487,1,0,0,0,487,488,6,57,3,0,488,118,1,0,0,0,489,491,8,13,0,0,490,489,
		1,0,0,0,491,492,1,0,0,0,492,490,1,0,0,0,492,493,1,0,0,0,493,120,1,0,0,
		0,494,498,5,44,0,0,495,497,7,12,0,0,496,495,1,0,0,0,497,500,1,0,0,0,498,
		496,1,0,0,0,498,499,1,0,0,0,499,122,1,0,0,0,500,498,1,0,0,0,501,505,5,
		33,0,0,502,504,9,0,0,0,503,502,1,0,0,0,504,507,1,0,0,0,505,506,1,0,0,0,
		505,503,1,0,0,0,506,509,1,0,0,0,507,505,1,0,0,0,508,510,5,13,0,0,509,508,
		1,0,0,0,509,510,1,0,0,0,510,511,1,0,0,0,511,512,5,10,0,0,512,124,1,0,0,
		0,513,517,5,59,0,0,514,516,7,14,0,0,515,514,1,0,0,0,516,519,1,0,0,0,517,
		515,1,0,0,0,517,518,1,0,0,0,518,531,1,0,0,0,519,517,1,0,0,0,520,524,5,
		33,0,0,521,523,9,0,0,0,522,521,1,0,0,0,523,526,1,0,0,0,524,525,1,0,0,0,
		524,522,1,0,0,0,525,528,1,0,0,0,526,524,1,0,0,0,527,529,5,13,0,0,528,527,
		1,0,0,0,528,529,1,0,0,0,529,530,1,0,0,0,530,532,5,10,0,0,531,520,1,0,0,
		0,531,532,1,0,0,0,532,535,1,0,0,0,533,535,5,36,0,0,534,513,1,0,0,0,534,
		533,1,0,0,0,535,536,1,0,0,0,536,537,6,61,4,0,537,126,1,0,0,0,538,540,7,
		12,0,0,539,538,1,0,0,0,540,541,1,0,0,0,541,539,1,0,0,0,541,542,1,0,0,0,
		542,543,1,0,0,0,543,544,6,62,5,0,544,128,1,0,0,0,545,546,5,40,0,0,546,
		130,1,0,0,0,547,548,5,41,0,0,548,132,1,0,0,0,549,553,5,36,0,0,550,552,
		9,0,0,0,551,550,1,0,0,0,552,555,1,0,0,0,553,554,1,0,0,0,553,551,1,0,0,
		0,554,561,1,0,0,0,555,553,1,0,0,0,556,562,5,36,0,0,557,559,5,13,0,0,558,
		557,1,0,0,0,558,559,1,0,0,0,559,560,1,0,0,0,560,562,5,10,0,0,561,556,1,
		0,0,0,561,558,1,0,0,0,562,134,1,0,0,0,563,567,5,35,0,0,564,566,9,0,0,0,
		565,564,1,0,0,0,566,569,1,0,0,0,567,568,1,0,0,0,567,565,1,0,0,0,568,571,
		1,0,0,0,569,567,1,0,0,0,570,572,5,13,0,0,571,570,1,0,0,0,571,572,1,0,0,
		0,572,573,1,0,0,0,573,574,5,10,0,0,574,575,1,0,0,0,575,576,6,66,0,0,576,
		136,1,0,0,0,577,581,5,34,0,0,578,580,9,0,0,0,579,578,1,0,0,0,580,583,1,
		0,0,0,581,582,1,0,0,0,581,579,1,0,0,0,582,584,1,0,0,0,583,581,1,0,0,0,
		584,585,5,34,0,0,585,138,1,0,0,0,586,590,5,42,0,0,587,589,9,0,0,0,588,
		587,1,0,0,0,589,592,1,0,0,0,590,591,1,0,0,0,590,588,1,0,0,0,591,593,1,
		0,0,0,592,590,1,0,0,0,593,594,5,42,0,0,594,140,1,0,0,0,595,596,5,46,0,
		0,596,597,5,46,0,0,597,598,1,0,0,0,598,599,6,69,4,0,599,142,1,0,0,0,600,
		602,7,15,0,0,601,600,1,0,0,0,602,603,1,0,0,0,603,601,1,0,0,0,603,604,1,
		0,0,0,604,144,1,0,0,0,605,615,8,16,0,0,606,610,5,60,0,0,607,609,9,0,0,
		0,608,607,1,0,0,0,609,612,1,0,0,0,610,611,1,0,0,0,610,608,1,0,0,0,611,
		613,1,0,0,0,612,610,1,0,0,0,613,615,5,62,0,0,614,605,1,0,0,0,614,606,1,
		0,0,0,615,616,1,0,0,0,616,614,1,0,0,0,616,617,1,0,0,0,617,146,1,0,0,0,
		51,0,1,2,327,337,345,351,358,362,370,374,382,386,393,399,403,409,411,417,
		419,423,428,430,436,445,458,462,464,475,485,492,498,505,509,517,524,528,
		531,534,541,553,558,561,567,571,581,590,603,610,614,616,6,0,1,0,5,1,0,
		5,2,0,0,2,0,4,0,0,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
