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
        public ListExpression ReadFile(string contents, bool hasHeaderLine = true)
        {
            List<string> lines = new List<string>();
            using (StringReader reader = new StringReader(contents))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line)) lines.Add(line);
                }
            }

            var headerLine = lines[0];

            IdentifierTransformer transformer = new IdentifierTransformer();
            var headers = headerLine.Split('\t').Select(s => transformer.ToIdentifier(s)).ToList();

            var objects = lines.Skip(1).Select((line, index) => RecordToObject(line, headers, index + 2)).Cast<Expression>().ToList();

            return new ListExpression(objects);
        }

        private static IdfPlusObjectExpression RecordToObject(string record, List<string> headers, int lineNum)
        {
            var fields = record.Split('\t');

            if (fields.Length != headers.Count)
                throw new FileLoadException(
                    $"Number of fields in record ({fields.Length}) does not equal number of headers ({headers.Count}).\nRecord:{record}\nLineNum:{lineNum}");

            var zipped = headers.Zip(fields, (s, s1) => (Header: s, Field: s1)).ToList();

            IdfPlusObjectExpression objectExpression = new IdfPlusObjectExpression();

            foreach (var (header, field) in zipped)
            {
                string trimmedField = field.Trim();
                if (double.TryParse(trimmedField, out double numericValue))
                {
                    objectExpression.Members[header] = new NumericExpression(numericValue);
                }
                else
                {
                    objectExpression.Members[header] = new StringExpression(trimmedField);
                }
            }

            return objectExpression;
        }
    }

    public class IdentifierTransformer
    {
        public string ToIdentifier(string input)
        {
            Regex regex = new Regex("[^a-zA-Z0-9_]+");
            var inputWithValidChars = regex.Replace(input, "_").ToLower();
            Regex multipleUnderscores = new Regex("_+");
            inputWithValidChars =  multipleUnderscores.Replace(input, "_");
            return inputWithValidChars.TrimStart('_').TrimEnd('_');
        }
    }
}