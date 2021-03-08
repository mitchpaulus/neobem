using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime.Tree;
using OfficeOpenXml;

namespace src
{
    public class LoadFunctionExpression : FunctionExpression
    {
        public LoadFunctionExpression(string baseDirectory) : base(new List<Dictionary<string, Expression>>(), new List<string>{ "options"} )
        {
        }

        public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
        {
            if (inputs[0] is StringExpression stringExpression)
            {
                var fullPath = Path.GetFullPath(stringExpression.Text, baseDirectory);

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
                    string worksheetName;
                    var tryGetSheet = objectExpression.Members.TryGetValue("sheet", out Expression expression);
                    if (!tryGetSheet) worksheetName = null;
                    else if (expression is StringExpression sheetExpression) worksheetName = sheetExpression.Text;
                    else
                    {
                        throw new ArgumentException(
                            $"'sheet' property is expected to be a String expression. Received a {expression.TypeName()}");
                    }

                    var tryGetRange = objectExpression.Members.TryGetValue("range", out expression);

                    // By default read the entire sheet possible.
                    ExcelRange range = new SheetDimensionRange();
                    if (tryGetRange)
                    {
                        if (expression is StringExpression rangeExpression)
                        {
                            ExcelRangeParser parser = rangeExpression.Text.ToExcelRangeParser();
                            ExcelRangeParser.RangeContext tree = parser.range();
                            MyExcelRangeListener listener = new MyExcelRangeListener();
                            ParseTreeWalker walker = new ParseTreeWalker();
                            walker.Walk(listener, tree);
                            range = listener.ExcelRange;
                        }
                        else
                        {
                            throw new ArgumentException(
                                $"'range' property is expected to be a string. Received a {expression.TypeName()}");
                        }
                    }

                    string filePath = ((StringExpression) objectExpression.Members["path"]).Text;
                    var fullFilePath = Path.GetFullPath(filePath, baseDirectory);
                    return ("", ExcelDataLoader.Load( fullFilePath, worksheetName, range));
                }
                else
                {
                    throw new NotImplementedException($"Non string input for load function not implemented yet.");
                }
            }

            throw new ArgumentException($"load function expects string or structure - found {inputs[0].TypeName()}");
        }

        public override string AsString() => "Load";
    }

    public class ExcelDataLoader
    {
        public static ListExpression Load(string fullFilePath, string worksheet, ExcelRange range)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            FileInfo fileInfo = new FileInfo(fullFilePath);

            if (!fileInfo.Exists) throw new FileNotFoundException($"Could not find the file {fullFilePath}.");

            using ExcelPackage excelFile = new ExcelPackage(fileInfo);
            ExcelWorksheet sheet;
            if (worksheet == null)
            {
                sheet = excelFile.Workbook.Worksheets.First();
            }
            else
            {
                sheet = excelFile.Workbook.Worksheets.FirstOrDefault(excelWorksheet =>
                    excelWorksheet.Name == worksheet);

                if (sheet == default)
                {
                    throw new ArgumentException($"The sheet '{worksheet}' was not found in file {fileInfo.FullName}.");
                }
            }
            return range.ReadSheet(sheet);
        }

    }

    public interface ExcelRange
    {
        public ListExpression ReadSheet(ExcelWorksheet sheet);
    }


    public class FullRange : ExcelRange
    {
        private readonly int _startRow;
        private readonly int _startCol;
        private readonly int _endRow;
        private readonly int _endCol;

        public FullRange(int startRow, int startCol, int endRow, int endCol)
        {
            _startRow = startRow;
            _startCol = startCol;
            _endRow = endRow;
            _endCol = endCol;
        }

        public ListExpression ReadSheet(ExcelWorksheet sheet)
        {
            List<string> headers = new List<string>();

            List<Expression> objects = new List<Expression>();

            for (var row = _startRow; row <= _endRow; row++)
            {
                if (row == _startRow)
                {
                    for (var column = _startCol; column <= _endCol; column++)
                    {
                        var cellValue = sheet.Cells[row, column].Text;
                        headers.Add(cellValue);
                    }
                }
                else
                {
                    IdfPlusObjectExpression objectExpression = new IdfPlusObjectExpression();
                    var index = 0;
                    for (var column = _startCol; column <= _endCol; column++)
                    {
                        var cellValue = sheet.Cells[row, column].Text;
                        var header = headers[index];
                        objectExpression.Members[header] = cellValue.CellTextToExpression();

                        index++;
                    }
                    objects.Add(objectExpression);
                }
            }

            return new ListExpression(objects);
        }
    }

    public class StartCell : ExcelRange
    {
        private readonly int _startRow;
        private readonly int _startCol;

        public StartCell(int startRow, int startCol)
        {
            _startRow = startRow;
            _startCol = startCol;
        }

        public ListExpression ReadSheet(ExcelWorksheet sheet)
        {
            List<string> headers = new List<string>();

            List<Expression> objects = new List<Expression>();

            var row = _startRow;
            var col = _startCol;
            while (!string.IsNullOrWhiteSpace(sheet.Cells[row, col].Text))
            {
                headers.Add(sheet.Cells[row, col].Text);
                col++;
            }

            row++;

            while (!RecordValues(row, _startCol, _startCol + headers.Count - 1, sheet).All(string.IsNullOrWhiteSpace))
            {
                IdfPlusObjectExpression objectExpression = new IdfPlusObjectExpression();
                var index = 0;
                for (var column = _startCol; column < _startCol + headers.Count(); column++)
                {
                    var cellValue = sheet.Cells[row, column].Text;
                    var header = headers[index];
                    objectExpression.Members[header] = cellValue.CellTextToExpression();

                    index++;
                }
                objects.Add(objectExpression);
                row++;
            }

            return new ListExpression(objects);
        }

        private List<string> RecordValues(int row, int startColumn, int endColumn, ExcelWorksheet sheet)
        {
            List<string> recordValues = new List<string>();
            for (int col = startColumn; col < endColumn; col++)
            {
                recordValues.Add(sheet.Cells[row, col].Text);
            }

            return recordValues;
        }

    }

    public class SheetDimensionRange : ExcelRange
    {
        public ListExpression ReadSheet(ExcelWorksheet sheet)
        {
            int startRow = sheet.Dimension.Start.Row;
            int endRow = sheet.Dimension.End.Row;
            int startColumn = sheet.Dimension.Start.Column;
            int endColumn = sheet.Dimension.End.Column;

            FullRange fullRange = new FullRange(startRow, startColumn, endRow, endColumn);
            return fullRange.ReadSheet(sheet);
        }
    }

    public class MyExcelRangeListener : ExcelRangeBaseListener
    {
        public ExcelRange ExcelRange;
        public override void EnterFullrange(ExcelRangeParser.FullrangeContext context)
        {
            int startRow = int.Parse(context.ROW(0).GetText());
            int endRow = int.Parse(context.ROW(1).GetText());

            int startCol = context.COLUMN(0).GetText().ExcelColumnNameToInt();
            int endCol = context.COLUMN(1).GetText().ExcelColumnNameToInt();

            ExcelRange = new FullRange(startRow, startCol, endRow, endCol);
        }

        public override void EnterStartcell(ExcelRangeParser.StartcellContext context)
        {
            int startRow = int.Parse(context.ROW().GetText());
            int startCol = context.COLUMN().GetText().ExcelColumnNameToInt();
            ExcelRange = new StartCell(startRow, startCol);
        }
    }

    public enum ExcelRangeType
    {
        FullRange = 0,
        StartCell = 1,
        StartRowWithColumns = 2,
    }
}