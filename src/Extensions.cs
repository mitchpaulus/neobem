using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace src
{
    public static class Extensions
    {
        public static NeobemParser ToParser(this string input, FileType fileType)
        {
            AntlrInputStream inputStream = new AntlrInputStream(input);
            NeobemLexer lexer = new NeobemLexer(inputStream);
            lexer.FileType = fileType;
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            return new NeobemParser(tokens);
        }

        public static NeobemParser ToIdfParser(this string input) => ToParser(input, FileType.Idf);

        public static string Format(this string input)
        {
            AntlrInputStream inputStream = new(input);
            NeobemLexer lexer = new(inputStream);
            CommonTokenStream tokens = new(lexer);
            NeobemParser parser = new(tokens);
            NeobemParser.IdfContext idfTree = parser.idf();
            FormatVisitor visitor = new(0, 0, tokens);
            return visitor.Visit(idfTree);
        }

        public static ExcelRangeParser ToExcelRangeParser(this string input)
        {
            AntlrInputStream inputStream = new AntlrInputStream(input);
            ExcelRangeLexer lexer = new ExcelRangeLexer(inputStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            return new ExcelRangeParser(tokens);
        }


        public static Expression CellTextToExpression(this string cellValue)
        {
            if (double.TryParse(cellValue, out double numericValue))
                return new NumericExpression(numericValue);
            if (string.Equals(cellValue, "true", StringComparison.OrdinalIgnoreCase))
                return new BooleanExpression(true);
            if (string.Equals(cellValue, "false", StringComparison.OrdinalIgnoreCase))
                return new BooleanExpression(false);
            return new StringExpression(cellValue);
        }

        public static int ExcelColumnNameToInt(this string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException(nameof(columnName));

            columnName = columnName.ToUpperInvariant();

            int sum = 0;

            foreach (char c in columnName)
            {
                sum *= 26;
                sum += (c - 'A' + 1);
            }

            return sum;
        }

        public static string SanitizeIdentifier(this string value)
        {
            var removeNonBeginAlphaChars = Regex.Replace(value, "^[^a-zA-Z]*", "");

            if (removeNonBeginAlphaChars.Length < 1)
                throw new ArgumentException($"There are no alpha characters in the input string '{value}'.");

            char lowerCaseFirstChar = Char.ToLower(removeNonBeginAlphaChars[0]);

            var fixRemainingChars = Regex.Replace( Regex.Replace(removeNonBeginAlphaChars.Substring(1), "[^a-zA-Z0-9_]+", "_"), "_+$", "");

            return lowerCaseFirstChar + fixRemainingChars;
        }

        public static List<string> SplitLines(this string input)
        {
            List<string> output = new List<string>();
            using (StringReader sr = new StringReader(input)) {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    output.Add(line);
                }
            }

            return output;
        }
        public static string JoinNull(this IEnumerable<string> strings) => string.Join("", strings);
        public static string Join(this IEnumerable<string> strings, string join = "") => string.Join(join, strings);

        public static int NumLines(this StringBuilder builder) => builder.ToString().SplitLines().Count;
        public static int NumLines(this string input) => input.SplitLines().Count;

        // Get the length of the last line of a string.
        public static int CurrentPosition(this string input) => input.SplitLines().Last().Length;

        /// <summary>
        /// Split an idf object by comment.  Ex:
        /// my field, ! Comment
        /// returns SplitIdfLine('my field, ', '! Comment')
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static SplitIdfLine SplitComment(this string input)
        {
            var indexOfExclamation = input.IndexOf('!');

            if (indexOfExclamation < 0)
            {
                return new SplitIdfLine()
                {
                    IdfText = input, Comment = ""
                };
            }
            else
            {
                return new SplitIdfLine()
                {
                    IdfText = input.Substring(0, indexOfExclamation),
                    Comment = input.Substring(indexOfExclamation)
                };
            }
        }

        public static string LineEnding(this string input)
        {
            foreach (var character in input)
            {
                if (character == '\r') return "\r\n";
                if (character == '\n') return "\n";
            }
            return "\n";
        }

        public static string IndentSpaces(this int indentLevel, int indentSpaces) =>
            new string(' ', indentLevel * indentSpaces);

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> list, int start = 0) =>
            list.Select((item, index) => (item, index + start));

        public static bool PreviousTokenEndsWithNewline(this BufferedTokenStream tokenStream, int tokenIndex)
        {
            if (tokenIndex - 1 >= 0)
            {
                IToken token = tokenStream.Get(tokenIndex - 1);
                // This is here because the formatter for IDF objects always puts a newline at the end of the object.
                // The final token from the lexer only has the newline if there was a final comment. So this is a bit fragile,
                // but this should be the only edge case.
                if (token.Type == NeobemLexer.OBJECT_TERMINATOR) return true;
                string text = token.Text;
                return text.EndsWith("\n");
            }
            return false;
        }

        public static bool PreviousTokenEndsWithNewline(this BufferedTokenStream tokenStream, IToken token) =>
            tokenStream.PreviousTokenEndsWithNewline(token.TokenIndex);

        public static string FixComment(this string commentText)
        {
            if (!commentText.StartsWith("#"))
                throw new ArgumentException($"Looks like improper usage of FixComment. Input should start with #. Received '{commentText}'");

            return Regex.Replace(commentText, "#[ ]*", "# ").TrimEnd() + "\n";
        }

        public static string Pluralize(this int value) => value == 1 ? "" : "s";

        public static string ToSigFigs(this double value, int sigFigs)
        {
            // Always have to handle 0 specially.
            if (value == 0) return "0";
            //   Original: 100  10  1  0.1
            //     Log 10:   2   1  0   -1
            // 4 sig figs:   1   2  3    4
            int log = (int)Math.Floor(Math.Log10(Math.Abs(value)));
            var digits = Math.Max(0, sigFigs - 1 - log);

            // Not exactly *true* significant figures, if the value is an integer, that's fine.
            var rounded = Math.Round(value, digits);

            // Don't want to deal with large numbers getting put into scientific notation on accident.
            // The 'G' format will take care of trailing zeros for numbers with decimals.
            return rounded.ToString(digits == 0 ? "F0" : $"G{sigFigs}");
        }

        public static int NumNewLinesRequired(this string input, int numNewLines = 2)
        {
            int newLinesFound = 0;
            for (int i = input.Length - 1; i > -1; i--)
            {
                if (input[i] == '\n') newLinesFound++;
                else break;
            }
            return numNewLines - newLinesFound;
        }

        public static string AddNewLines(this string input, int numNewLines = 2)
        {
            int additionalNewLineCount = input.NumNewLinesRequired(numNewLines);
            if (additionalNewLineCount == 0) return input;
            if (additionalNewLineCount < 0) return input[..(input.Length + additionalNewLineCount)];
            string additionalNewLines = new('\n', additionalNewLineCount);
            return $"{input}{additionalNewLines}";
        }

        public static string ParagraphJoin(this IEnumerable<string> items)
        {
            StringBuilder builder = new();
            bool firstItem = true;
            int newLinesRequired = 0;
            foreach (string item in items)
            {
                if (firstItem)
                {
                    builder.Append(item);
                    firstItem = false;
                }
                else
                {
                    if (newLinesRequired == 0) builder.Append(item);
                    else builder.Append($"{new string('\n', newLinesRequired)}{item}");
                }
                newLinesRequired = item.NumNewLinesRequired(2);
            }
            return builder.ToString();
        }

        public static string ToString(this IToken token, NeobemLexer lexer)
        {
            string tokenTypeDisplayName = lexer.Vocabulary.GetDisplayName(token.Type);

            string replacedText = token.Text.Replace("\n", "\\n").Replace("\t", "\\t").Replace("\r", "\\r");

            string channelString = token.Channel == 0 ? "" : $"channel={token.Channel}";

            return
                $"[@{token.TokenIndex},{token.StartIndex}:{token.StopIndex}='{replacedText}',<{tokenTypeDisplayName}>{channelString},{token.Line},{token.Column}";
        }

    }

    public class SplitIdfLine
    {
        public string Comment;
        public string IdfText;
    }
}