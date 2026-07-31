using System.Collections.Generic;
using Antlr4.Runtime;
using src;

namespace gui.Parsing;

// Highlighting categories. Deliberately coarse - one color per category, mapped
// from lexer token types rather than from regexes, so highlighting can never
// disagree with how the file actually lexes.
public enum TokenCategory
{
    Default,
    Comment,
    Keyword,
    Constant,
    Number,
    String,
    ObjectType,
    Field,
    BuiltIn,
    Operator,
}

// Half-open [Start, End) char range of the source with the color it gets.
public record SyntaxSpan(int Start, int End, TokenCategory Category);

public static class NeobemTokenizer
{
    // Lexes for highlighting only. This is a separate pass from the parse that
    // builds the structure tree and language-service indexes, because those run
    // over a token stream that drops comments and whitespace - which is exactly
    // what highlighting needs to see.
    public static IReadOnlyList<SyntaxSpan> Tokenize(string text, FileType fileType)
    {
        var lexer = new NeobemLexer(new AntlrInputStream(text)) { FileType = fileType };
        lexer.RemoveErrorListeners();

        List<SyntaxSpan> spans = new();
        foreach (IToken token in lexer.GetAllTokens())
        {
            if (token.StartIndex < 0 || token.StopIndex < token.StartIndex) continue;

            TokenCategory category = Categorize(token);
            if (category == TokenCategory.Default) continue;

            spans.Add(new SyntaxSpan(token.StartIndex, token.StopIndex + 1, category));
        }

        return spans;
    }

    private static TokenCategory Categorize(IToken token) => token.Type switch
    {
        NeobemLexer.COMMENT or
        NeobemLexer.NEOBEM_COMMENT or
        NeobemLexer.DOE2COMMENT or
        NeobemLexer.OBJECT_COMMENT or
        NeobemLexer.DOE2_OBJECT_COMMENT or
        NeobemLexer.DOE2_NEOBEM_COMMENT => TokenCategory.Comment,

        NeobemLexer.IF or NeobemLexer.THEN or NeobemLexer.ELSE or
        NeobemLexer.AND_OP or NeobemLexer.OR_OP or NeobemLexer.NOT or
        NeobemLexer.RETURN or NeobemLexer.IMPORT or NeobemLexer.AS or
        NeobemLexer.ONLY or NeobemLexer.EXPORT or NeobemLexer.PRINT or
        NeobemLexer.LOG or NeobemLexer.LET or NeobemLexer.IN or
        NeobemLexer.FUNCTION_BEGIN => TokenCategory.Keyword,

        NeobemLexer.BOOLEAN_LITERAL_TRUE or
        NeobemLexer.BOOLEAN_LITERAL_FALSE => TokenCategory.Constant,

        NeobemLexer.NUMERIC or NeobemLexer.UUID or NeobemLexer.BCL_ID => TokenCategory.Number,

        NeobemLexer.STRING or NeobemLexer.DOE2_STRING or
        NeobemLexer.DOE2STRING_UNAME or NeobemLexer.DOE2_LITERAL => TokenCategory.String,

        NeobemLexer.OBJECT_TYPE or NeobemLexer.DOE2IDENTIFIER => TokenCategory.ObjectType,

        NeobemLexer.FIELD or NeobemLexer.DOE2_FIELD => TokenCategory.Field,

        NeobemLexer.EQUALS or NeobemLexer.CARET or NeobemLexer.MULTOP or
        NeobemLexer.DIVIDEOP or NeobemLexer.PLUSOP or NeobemLexer.MINUSOP or
        NeobemLexer.LESSTHAN or NeobemLexer.GREATERTHAN or
        NeobemLexer.LESS_THAN_OR_EQUAL_TO or NeobemLexer.GREATER_THAN_OR_EQUAL_TO or
        NeobemLexer.EQUAL_TO or NeobemLexer.NOT_EQUAL_TO or
        NeobemLexer.MAP_OPERATOR or NeobemLexer.FILTER_OPERATOR or
        NeobemLexer.PIPE_OPERATOR or NeobemLexer.RANGE_OPERATOR or
        NeobemLexer.MEMBER_ACCESS or NeobemLexer.STRUCT_SEP or
        NeobemLexer.INLINE_TABLE_BEGIN_END_SEP or
        NeobemLexer.INLINE_TABLE_COL_SEP => TokenCategory.Operator,

        // Built-in functions get their own color; everything else the user named
        // stays the default foreground.
        NeobemLexer.IDENTIFIER => LanguageServer.TryGetBuiltInHoverMarkdown(token.Text) is not null
            ? TokenCategory.BuiltIn
            : TokenCategory.Default,

        _ => TokenCategory.Default,
    };
}
