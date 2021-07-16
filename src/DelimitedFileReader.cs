using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace src
{
    public class DelimitedFileReader
    {
        private readonly string _delimiter;

        public DelimitedFileReader(string delimiter = "\t")
        {
            _delimiter = delimiter;
        }

        public ListExpression ReadFile(string contents, bool hasHeaderLine = true, int skipLines = 0)
        {
            List<string> lines = new();
            int lineNum = 1;
            using (StringReader reader = new(contents))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line) && lineNum > skipLines) lines.Add(line);
                    lineNum++;
                }
            }

            // If there is no data, return empty list.
            // TODO: Potentially add warning capability?
            if (lines.Count == 0) return new ListExpression(new List<Expression>());

            // Generally use header line verbatim. Otherwise use 0-based index as structure members.
            List<string> headers;
            if (hasHeaderLine)
            {
                string headerLine = lines[0];
                headers = headerLine.Split(_delimiter).ToList();
            }
            else
            {
                // Decided to use 1-based index here (the i + 1) for columns. I know that other parts of the code
                // have used 0-based, but for column identifiers like this, I feel like this is a more
                // pragmatic decision.
                headers = lines[0].Split(_delimiter).Select((s, i) => (i + 1).ToString()).ToList();
            }

            // We need to skip the header line if used
            int linesToSkip = hasHeaderLine ? 1 : 0;
            // Passing along the line numbers so if there is an issue in parsing we can present the line number in an error message.
            int lineNumberOffset = linesToSkip + 1;

            List<Expression> objects = lines.Skip(linesToSkip)
                                            .Select((line, index) => RecordToObject(line, headers, index + lineNumberOffset))
                                            .Cast<Expression>().ToList();

            return new ListExpression(objects);
        }

        private IdfPlusObjectExpression RecordToObject(string record, List<string> headers, int lineNum)
        {
            string[] fields = record.Split(_delimiter);

            if (fields.Length != headers.Count)
                throw new FileLoadException(
                    $"Number of fields in record ({fields.Length}) does not equal number of headers ({headers.Count}).\nRecord:{record}\nLineNum:{lineNum}");

            var zipped = headers.Zip(fields, (s, s1) => (Header: s, Field: s1)).ToList();

            IdfPlusObjectExpression objectExpression = new();

            foreach ((string header, string field) in zipped) objectExpression.Members[header] = Expression.Parse(field.Trim());

            return objectExpression;
        }
    }

    public class IdentifierTransformer
    {
        public string ToIdentifier(string input)
        {
            Regex regex = new("[^a-zA-Z0-9_]+");
            var inputWithValidChars = regex.Replace(input, "_").ToLower();
            Regex multipleUnderscores = new("_+");
            inputWithValidChars =  multipleUnderscores.Replace(input, "_");
            return inputWithValidChars.TrimStart('_').TrimEnd('_');
        }
    }
}