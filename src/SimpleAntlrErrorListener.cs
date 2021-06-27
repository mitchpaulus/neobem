using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

namespace src
{
    public class SimpleAntlrErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        public List<AntlrError> Errors = new List<AntlrError>();
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Errors.Add(new AntlrError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e));
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine,
            string msg, RecognitionException e)
        {
            Errors.Add(new AntlrError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e));
        }
    }

    public enum LexerParserError
    {
        Lexer = 0,
        Parser = 1,
    }

    public class AntlrError
    {
        public readonly LexerParserError Type;
        public readonly TextWriter Output;
        public readonly IRecognizer Recognizer;
        public readonly IToken OffendingSymbolToken;
        public readonly int OffendingSymbolInt;
        public readonly int Line;
        public readonly int CharPositionInLine;
        public readonly string Msg;
        public readonly RecognitionException E;

        public AntlrError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
                                      string msg, RecognitionException e)
        {
            Type = LexerParserError.Parser;
            Output = output;
            Recognizer = recognizer;
            OffendingSymbolToken = offendingSymbol;
            Line = line;
            CharPositionInLine = charPositionInLine;
            Msg = msg;
            E = e;
        }

        public AntlrError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Type = LexerParserError.Lexer;
            Output = output;
            Recognizer = recognizer;
            OffendingSymbolInt = offendingSymbol;
            Line = line;
            CharPositionInLine = charPositionInLine;
            Msg = msg;
            E = e;
        }

        public string WriteError() => $"Line {Line}:{CharPositionInLine} {Msg}";

    }
}