using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace src
{
    public class LoadFunctionExpression : FunctionExpression
    {
        public LoadFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>{ "options"} )
        {
        }

        public override (string, Expression) Evaluate(List<Expression> inputs)
        {
            if (inputs[0] is StringExpression stringExpression)
            {
                var fullPath = Path.GetFullPath(stringExpression.Text);

                if (File.Exists(fullPath))
                {
                    DelimitedFileReader reader = new DelimitedFileReader();
                    string contents = File.ReadAllText(fullPath);

                    var listExpression = reader.ReadFile(contents);

                    return ("", listExpression);
                }
                else
                {
                    throw new FileNotFoundException($"The file {stringExpression.Text} could not be found.");
                }
            }
            else if (inputs[0] is IdfPlusObjectExpression objectExpression)
            {

                if (objectExpression.Members["type"] is StringExpression {Text: "Excel"})
                {
                    string filePath = ((StringExpression) objectExpression.Members["path"]).Text;
                    var fullFilePath = Path.GetFullPath(filePath);
                    return ("", ExcelDataLoader.Load(fullFilePath, null, null));
                }
                else
                {
                    throw new NotImplementedException($"Non string input for load function not implemented yet.");
                }
            }

            throw new ArgumentException($"load function expects String or Object - found {inputs[0].GetType()}");
        }

        public override string AsString() => "Load";
    }

    public class ExcelDataLoader
    {
        public static ListExpression Load(string filepath, string worksheet, string range)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            FileInfo fileInfo = new FileInfo(filepath);
            using ExcelPackage excelFile = new ExcelPackage(fileInfo);

            ExcelWorksheet sheet = excelFile.Workbook.Worksheets.First();
            int startRow = sheet.Dimension.Start.Row;
            int endRow = sheet.Dimension.End.Row;
            int startColumn = sheet.Dimension.Start.Column;
            int endColumn = sheet.Dimension.End.Column;

            List<string> headers = new List<string>();

            List<Expression> objects = new List<Expression>();

            for (var row = startRow; row <= endRow; row++)
            {
                if (row == startRow)
                {
                    for (var column = startColumn; column <= endColumn; column++)
                    {
                        var cellValue = sheet.Cells[row, column].Text;
                        headers.Add(cellValue);
                    }
                }
                else
                {
                    IdfPlusObjectExpression objectExpression = new IdfPlusObjectExpression();
                    var index = 0;
                    for (var column = startColumn; column <= endColumn; column++)
                    {
                        var cellValue = sheet.Cells[row, column].Text;
                        var header = headers[index];
                        if (double.TryParse(cellValue, out double numericValue))
                        {
                            objectExpression.Members[header] = new NumericExpression(numericValue);
                        }
                        else if (string.Equals(cellValue, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            objectExpression.Members[header] = new BooleanExpression(true);
                        }
                        else if (string.Equals(cellValue, "false", StringComparison.OrdinalIgnoreCase))
                        {
                            objectExpression.Members[header] = new BooleanExpression(false);
                        }
                        else
                        {
                            objectExpression.Members[header] = new StringExpression(cellValue);
                        }

                        index++;
                    }
                    objects.Add(objectExpression);
                }
            }

            return new ListExpression(objects);
        }
    }
}