using System;
using System.Linq;
using System.Text;

namespace src
{
    public class IdfObjectPrettyPrinter
    {
        public IdfObjectPrettyPrinter()
        {
        }

        public string ObjectPrettyPrinter(string input, int indentLevel, int indentSpacing)
        {
            StringBuilder builder = new StringBuilder();

            var lines = input.SplitLines();

            bool firstLine = true;
            foreach (var line in lines)
            {
                SplitIdfLine splitComment = line.SplitComment();

                var trimmedFields = splitComment.IdfText.Split(',').Select(s => s.Trim());
                // Trim end so last comma doesn't have extraneous space.
                var cleanText = string.Join(", ", trimmedFields).TrimEnd();

                // If not the first line, increase indent by 1 level.
                var startSpaces = firstLine ?  "" : (indentLevel + 1).IndentSpaces(indentSpacing);

                // Simplest method is same as the default out of the IdfEditor, put all comments in the same position for the entire file.
                var formatSpaces = new string(' ', Math.Max(2, 30 - cleanText.Length - startSpaces.Length));

                builder.Append(!string.IsNullOrWhiteSpace(splitComment.Comment)
                    ? $"{startSpaces}{cleanText}{formatSpaces}{splitComment.Comment}\n"
                    : $"{startSpaces}{cleanText}\n");

                firstLine = false;
            }

            return builder.ToString();
        }
    }
}