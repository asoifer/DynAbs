//------------------------------------------------------------------------------
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

namespace DynAbs.Summaries {
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.6.6")]
[System.CLSCompliant(false)]
public partial class SummariesLexer : Lexer {
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, T__10=11, SEPARATION=12, NULL=13, FRESH=14, ISIN=15, SINGLE=16, 
		MANY=17, ANY=18, RECEIVER=19, PARAMS=20, GLOBALS=21, RETURNVALUE=22, REACHABLEOBJ=23, 
		UNTILTYPE=24, OFTYPE=25, OFKIND=26, NUMBER=27, NEWLINE=28, WORD=29;
	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "T__8", 
		"T__9", "T__10", "LOWERCASE", "UPPERCASE", "SEPARATION", "NULL", "FRESH", 
		"ISIN", "SINGLE", "MANY", "ANY", "RECEIVER", "PARAMS", "GLOBALS", "RETURNVALUE", 
		"REACHABLEOBJ", "UNTILTYPE", "OFTYPE", "OFKIND", "NUMBER", "NEWLINE", 
		"WORD"
	};


	public SummariesLexer(ICharStream input)
		: base(input)
	{
		_interp = new LexerATNSimulator(this,_ATN);
	}

	private static readonly string[] _LiteralNames = {
		null, "'{}'", "'{'", "'}'", "'{|'", "'|'", "';'", "', '", "'@'", "'['", 
		"']'", "'*'", "'.'", "'Null'", "'Fresh'", "'IsIn'", "'Single'", "'Many'", 
		"'?'", "'R'", "'P'", "'G'", "'RV'", "'RO'", "'UntilType'", "'OfType'", 
		"'OfKind'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, null, null, null, 
		"SEPARATION", "NULL", "FRESH", "ISIN", "SINGLE", "MANY", "ANY", "RECEIVER", 
		"PARAMS", "GLOBALS", "RETURNVALUE", "REACHABLEOBJ", "UNTILTYPE", "OFTYPE", 
		"OFKIND", "NUMBER", "NEWLINE", "WORD"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[System.Obsolete("Use Vocabulary instead.")]
	public static readonly string[] tokenNames = GenerateTokenNames(DefaultVocabulary, _SymbolicNames.Length);

	private static string[] GenerateTokenNames(IVocabulary vocabulary, int length) {
		string[] tokenNames = new string[length];
		for (int i = 0; i < tokenNames.Length; i++) {
			tokenNames[i] = vocabulary.GetLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = vocabulary.GetSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}

		return tokenNames;
	}

	[System.Obsolete("Use IRecognizer.Vocabulary instead.")]
	public override string[] TokenNames
	{
		get
		{
			return tokenNames;
		}
	}

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Summaries.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return _serializedATN; } }

	public static readonly string _serializedATN =
		"\x3\xAF6F\x8320\x479D\xB75C\x4880\x1605\x191C\xAB37\x2\x1F\xB5\b\x1\x4"+
		"\x2\t\x2\x4\x3\t\x3\x4\x4\t\x4\x4\x5\t\x5\x4\x6\t\x6\x4\a\t\a\x4\b\t\b"+
		"\x4\t\t\t\x4\n\t\n\x4\v\t\v\x4\f\t\f\x4\r\t\r\x4\xE\t\xE\x4\xF\t\xF\x4"+
		"\x10\t\x10\x4\x11\t\x11\x4\x12\t\x12\x4\x13\t\x13\x4\x14\t\x14\x4\x15"+
		"\t\x15\x4\x16\t\x16\x4\x17\t\x17\x4\x18\t\x18\x4\x19\t\x19\x4\x1A\t\x1A"+
		"\x4\x1B\t\x1B\x4\x1C\t\x1C\x4\x1D\t\x1D\x4\x1E\t\x1E\x4\x1F\t\x1F\x4 "+
		"\t \x3\x2\x3\x2\x3\x2\x3\x3\x3\x3\x3\x4\x3\x4\x3\x5\x3\x5\x3\x5\x3\x6"+
		"\x3\x6\x3\a\x3\a\x3\b\x3\b\x3\b\x3\t\x3\t\x3\n\x3\n\x3\v\x3\v\x3\f\x3"+
		"\f\x3\r\x3\r\x3\xE\x3\xE\x3\xF\x3\xF\x3\x10\x3\x10\x3\x10\x3\x10\x3\x10"+
		"\x3\x11\x3\x11\x3\x11\x3\x11\x3\x11\x3\x11\x3\x12\x3\x12\x3\x12\x3\x12"+
		"\x3\x12\x3\x13\x3\x13\x3\x13\x3\x13\x3\x13\x3\x13\x3\x13\x3\x14\x3\x14"+
		"\x3\x14\x3\x14\x3\x14\x3\x15\x3\x15\x3\x16\x3\x16\x3\x17\x3\x17\x3\x18"+
		"\x3\x18\x3\x19\x3\x19\x3\x19\x3\x1A\x3\x1A\x3\x1A\x3\x1B\x3\x1B\x3\x1B"+
		"\x3\x1B\x3\x1B\x3\x1B\x3\x1B\x3\x1B\x3\x1B\x3\x1B\x3\x1C\x3\x1C\x3\x1C"+
		"\x3\x1C\x3\x1C\x3\x1C\x3\x1C\x3\x1D\x3\x1D\x3\x1D\x3\x1D\x3\x1D\x3\x1D"+
		"\x3\x1D\x3\x1E\x3\x1E\x3\x1F\x5\x1F\xA6\n\x1F\x3\x1F\x3\x1F\x6\x1F\xAA"+
		"\n\x1F\r\x1F\xE\x1F\xAB\x3 \x3 \x3 \x3 \x6 \xB2\n \r \xE \xB3\x2\x2\x2"+
		"!\x3\x2\x3\x5\x2\x4\a\x2\x5\t\x2\x6\v\x2\a\r\x2\b\xF\x2\t\x11\x2\n\x13"+
		"\x2\v\x15\x2\f\x17\x2\r\x19\x2\x2\x1B\x2\x2\x1D\x2\xE\x1F\x2\xF!\x2\x10"+
		"#\x2\x11%\x2\x12\'\x2\x13)\x2\x14+\x2\x15-\x2\x16/\x2\x17\x31\x2\x18\x33"+
		"\x2\x19\x35\x2\x1A\x37\x2\x1B\x39\x2\x1C;\x2\x1D=\x2\x1E?\x2\x1F\x3\x2"+
		"\x5\x3\x2\x63|\x3\x2\x43\\\x3\x2\x32;\xB9\x2\x3\x3\x2\x2\x2\x2\x5\x3\x2"+
		"\x2\x2\x2\a\x3\x2\x2\x2\x2\t\x3\x2\x2\x2\x2\v\x3\x2\x2\x2\x2\r\x3\x2\x2"+
		"\x2\x2\xF\x3\x2\x2\x2\x2\x11\x3\x2\x2\x2\x2\x13\x3\x2\x2\x2\x2\x15\x3"+
		"\x2\x2\x2\x2\x17\x3\x2\x2\x2\x2\x1D\x3\x2\x2\x2\x2\x1F\x3\x2\x2\x2\x2"+
		"!\x3\x2\x2\x2\x2#\x3\x2\x2\x2\x2%\x3\x2\x2\x2\x2\'\x3\x2\x2\x2\x2)\x3"+
		"\x2\x2\x2\x2+\x3\x2\x2\x2\x2-\x3\x2\x2\x2\x2/\x3\x2\x2\x2\x2\x31\x3\x2"+
		"\x2\x2\x2\x33\x3\x2\x2\x2\x2\x35\x3\x2\x2\x2\x2\x37\x3\x2\x2\x2\x2\x39"+
		"\x3\x2\x2\x2\x2;\x3\x2\x2\x2\x2=\x3\x2\x2\x2\x2?\x3\x2\x2\x2\x3\x41\x3"+
		"\x2\x2\x2\x5\x44\x3\x2\x2\x2\a\x46\x3\x2\x2\x2\tH\x3\x2\x2\x2\vK\x3\x2"+
		"\x2\x2\rM\x3\x2\x2\x2\xFO\x3\x2\x2\x2\x11R\x3\x2\x2\x2\x13T\x3\x2\x2\x2"+
		"\x15V\x3\x2\x2\x2\x17X\x3\x2\x2\x2\x19Z\x3\x2\x2\x2\x1B\\\x3\x2\x2\x2"+
		"\x1D^\x3\x2\x2\x2\x1F`\x3\x2\x2\x2!\x65\x3\x2\x2\x2#k\x3\x2\x2\x2%p\x3"+
		"\x2\x2\x2\'w\x3\x2\x2\x2)|\x3\x2\x2\x2+~\x3\x2\x2\x2-\x80\x3\x2\x2\x2"+
		"/\x82\x3\x2\x2\x2\x31\x84\x3\x2\x2\x2\x33\x87\x3\x2\x2\x2\x35\x8A\x3\x2"+
		"\x2\x2\x37\x94\x3\x2\x2\x2\x39\x9B\x3\x2\x2\x2;\xA2\x3\x2\x2\x2=\xA9\x3"+
		"\x2\x2\x2?\xB1\x3\x2\x2\x2\x41\x42\a}\x2\x2\x42\x43\a\x7F\x2\x2\x43\x4"+
		"\x3\x2\x2\x2\x44\x45\a}\x2\x2\x45\x6\x3\x2\x2\x2\x46G\a\x7F\x2\x2G\b\x3"+
		"\x2\x2\x2HI\a}\x2\x2IJ\a~\x2\x2J\n\x3\x2\x2\x2KL\a~\x2\x2L\f\x3\x2\x2"+
		"\x2MN\a=\x2\x2N\xE\x3\x2\x2\x2OP\a.\x2\x2PQ\a\"\x2\x2Q\x10\x3\x2\x2\x2"+
		"RS\a\x42\x2\x2S\x12\x3\x2\x2\x2TU\a]\x2\x2U\x14\x3\x2\x2\x2VW\a_\x2\x2"+
		"W\x16\x3\x2\x2\x2XY\a,\x2\x2Y\x18\x3\x2\x2\x2Z[\t\x2\x2\x2[\x1A\x3\x2"+
		"\x2\x2\\]\t\x3\x2\x2]\x1C\x3\x2\x2\x2^_\a\x30\x2\x2_\x1E\x3\x2\x2\x2`"+
		"\x61\aP\x2\x2\x61\x62\aw\x2\x2\x62\x63\an\x2\x2\x63\x64\an\x2\x2\x64 "+
		"\x3\x2\x2\x2\x65\x66\aH\x2\x2\x66g\at\x2\x2gh\ag\x2\x2hi\au\x2\x2ij\a"+
		"j\x2\x2j\"\x3\x2\x2\x2kl\aK\x2\x2lm\au\x2\x2mn\aK\x2\x2no\ap\x2\x2o$\x3"+
		"\x2\x2\x2pq\aU\x2\x2qr\ak\x2\x2rs\ap\x2\x2st\ai\x2\x2tu\an\x2\x2uv\ag"+
		"\x2\x2v&\x3\x2\x2\x2wx\aO\x2\x2xy\a\x63\x2\x2yz\ap\x2\x2z{\a{\x2\x2{("+
		"\x3\x2\x2\x2|}\a\x41\x2\x2}*\x3\x2\x2\x2~\x7F\aT\x2\x2\x7F,\x3\x2\x2\x2"+
		"\x80\x81\aR\x2\x2\x81.\x3\x2\x2\x2\x82\x83\aI\x2\x2\x83\x30\x3\x2\x2\x2"+
		"\x84\x85\aT\x2\x2\x85\x86\aX\x2\x2\x86\x32\x3\x2\x2\x2\x87\x88\aT\x2\x2"+
		"\x88\x89\aQ\x2\x2\x89\x34\x3\x2\x2\x2\x8A\x8B\aW\x2\x2\x8B\x8C\ap\x2\x2"+
		"\x8C\x8D\av\x2\x2\x8D\x8E\ak\x2\x2\x8E\x8F\an\x2\x2\x8F\x90\aV\x2\x2\x90"+
		"\x91\a{\x2\x2\x91\x92\ar\x2\x2\x92\x93\ag\x2\x2\x93\x36\x3\x2\x2\x2\x94"+
		"\x95\aQ\x2\x2\x95\x96\ah\x2\x2\x96\x97\aV\x2\x2\x97\x98\a{\x2\x2\x98\x99"+
		"\ar\x2\x2\x99\x9A\ag\x2\x2\x9A\x38\x3\x2\x2\x2\x9B\x9C\aQ\x2\x2\x9C\x9D"+
		"\ah\x2\x2\x9D\x9E\aM\x2\x2\x9E\x9F\ak\x2\x2\x9F\xA0\ap\x2\x2\xA0\xA1\a"+
		"\x66\x2\x2\xA1:\x3\x2\x2\x2\xA2\xA3\t\x4\x2\x2\xA3<\x3\x2\x2\x2\xA4\xA6"+
		"\a\xF\x2\x2\xA5\xA4\x3\x2\x2\x2\xA5\xA6\x3\x2\x2\x2\xA6\xA7\x3\x2\x2\x2"+
		"\xA7\xAA\a\f\x2\x2\xA8\xAA\a\xF\x2\x2\xA9\xA5\x3\x2\x2\x2\xA9\xA8\x3\x2"+
		"\x2\x2\xAA\xAB\x3\x2\x2\x2\xAB\xA9\x3\x2\x2\x2\xAB\xAC\x3\x2\x2\x2\xAC"+
		">\x3\x2\x2\x2\xAD\xB2\x5\x19\r\x2\xAE\xB2\x5\x1B\xE\x2\xAF\xB2\a\x61\x2"+
		"\x2\xB0\xB2\x5;\x1E\x2\xB1\xAD\x3\x2\x2\x2\xB1\xAE\x3\x2\x2\x2\xB1\xAF"+
		"\x3\x2\x2\x2\xB1\xB0\x3\x2\x2\x2\xB2\xB3\x3\x2\x2\x2\xB3\xB1\x3\x2\x2"+
		"\x2\xB3\xB4\x3\x2\x2\x2\xB4@\x3\x2\x2\x2\b\x2\xA5\xA9\xAB\xB1\xB3\x2";
	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());
}
} // namespace DynAbs.Summaries