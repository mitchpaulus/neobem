using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;

namespace src
{
    public static class Extensions
    {
        public static NeobemParser ToParser(this string input)
        {
            AntlrInputStream inputStream = new AntlrInputStream(input);
            NeobemLexer lexer = new NeobemLexer(inputStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            return new NeobemParser(tokens);
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
    }

    public class SplitIdfLine
    {
        public string Comment;
        public string IdfText;
    }
}