using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Antlr4.Runtime.Tree;

namespace src
{
    public class LoadFunctionExpression : FunctionExpression
    {
        public LoadFunctionExpression() : base(new List<Dictionary<string, Expression>>(), new List<string>{ "options"}, FileType.Any)
        {
        }

        public override (string, Expression) Evaluate(List<Expression> inputs, string baseDirectory)
        {
            // If a string is passed as first argument, read the file as a delimited text file.
            // Default is tab delimited.
            if (inputs[0] is StringExpression stringExpression)
            {
                DelimitedFileReader reader = new();
                var fullPath = Path.GetFullPath(stringExpression.Text, baseDirectory);

                Dependencies.Set.Add(fullPath);
                return EvaluateDelimitedFile(fullPath, reader, true, 0);
            }
            else if (inputs[0] is IdfPlusObjectExpression objectExpression)
            {
                if (!objectExpression.Members.TryGetValue("type", out Expression typeExpression))
                {
                    throw new ArgumentException("'type' is a mandatory member of the dictionary for the load function.");
                }

                if (!objectExpression.Members.TryGetValue("path", out Expression pathExpression))
                {
                    throw new ArgumentException("'path' is a mandatory member of the dictionary for the load function.");
                }

                if (typeExpression is not StringExpression typeStringExpression)
                {
                    throw new ArgumentException(
                        $"The 'type' member of the dictionary passed to load is expected to evaluate to a string, found a {typeExpression.TypeName()} with value {typeExpression.AsErrorString()}");
                }

                if (pathExpression is not StringExpression pathStringExpression)
                {
                    throw new ArgumentException(
                        $"The 'path' member of the dictionary passed to load is expected to evaluate to a string, found a {pathExpression.TypeName()} with value {pathExpression.AsErrorString()}");
                }

                if (typeStringExpression.TextEqualsCaseIns("text"))
                {
                    string delimiter;
                    if (objectExpression.Members.TryGetValue("delimiter", out Expression delimiterExp) && delimiterExp is StringExpression delimiterStringExpression)
                    {
                        delimiter = delimiterStringExpression.Text;
                    }
                    else delimiter = "\t";

                    bool hasHeaderLine;
                    if (objectExpression.Members.TryGetValue("has header", out Expression headerExp) && headerExp is BooleanExpression headerBooleanExpression)
                    {
                        hasHeaderLine = headerBooleanExpression.Value;
                    }
                    else hasHeaderLine = true;

                    int skipLines = 0;
                    if (objectExpression.Members.TryGetValue("skip", out Expression skipExp) && skipExp is NumericExpression skipIntExp)
                        skipLines = Convert.ToInt32(Math.Round(skipIntExp.Value));

                    DelimitedFileReader reader = new(delimiter);
                    var fullPath = Path.GetFullPath(pathStringExpression.Text, baseDirectory);

                    Dependencies.Set.Add(fullPath);
                    return EvaluateDelimitedFile(fullPath, reader, hasHeaderLine, skipLines);
                }
                else if (typeStringExpression.Text == "Excel")
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

                    Dependencies.Set.Add(fullFilePath);

                    return ("", ExcelDataLoader.Load( fullFilePath, worksheetName, range));
                }
                else if (objectExpression.Members["type"] is StringExpression {Text: "JSON"})
                {
                    string filePath = ((StringExpression) objectExpression.Members["path"]).Text;
                    var fullFilePath = Path.GetFullPath(filePath, baseDirectory);
                    Dependencies.Set.Add(fullFilePath);
                    string jsonData = File.ReadAllText(fullFilePath);
                    JsonDataLoader jsonLoader = new();

                    return ("", jsonLoader.Load(jsonData));
                }
                else if (objectExpression.Members["type"] is StringExpression {Text: "XML"})
                {
                    string filePath = ((StringExpression) objectExpression.Members["path"]).Text;
                    var fullFilePath = Path.GetFullPath(filePath, baseDirectory);
                    Dependencies.Set.Add(fullFilePath);
                    string jsonData = File.ReadAllText(fullFilePath);
                    XmlDataLoader xmlDataLoader = new();

                    return ("", xmlDataLoader.Load(jsonData));
                }
                else
                {
                    throw new NotImplementedException($"Non string input for load function not implemented yet.");
                }
            }

            throw new ArgumentException($"load function expects string or dictionary - found {inputs[0].TypeName()}");
        }

        private static (string, Expression) EvaluateDelimitedFile(string fullPath, DelimitedFileReader reader, bool hasHeaderLine, int skipLines)
        {
            if (File.Exists(fullPath))
            {
                string contents = File.ReadAllText(fullPath);
                ListExpression listExpression = reader.ReadFile(contents, hasHeaderLine, skipLines);
                return ("", listExpression);
            }
            else
            {
                throw new FileNotFoundException($"The file {fullPath} could not be found.");
            }
        }

        public override string AsString() => "Load";
    }

    public class ExcelDataLoader
    {
        public static ListExpression Load(string fullFilePath, string worksheet, ExcelRange range)
        {
            if (!File.Exists(fullFilePath)) throw new FileNotFoundException($"Could not find the file {fullFilePath}.");

            using XlsxWorkbook workbook = XlsxWorkbook.Open(fullFilePath);

            XlsxWorksheet sheet;
            if (worksheet == null)
            {
                sheet = workbook.FirstWorksheet();
            }
            else
            {
                sheet = workbook.GetWorksheet(worksheet);

                if (sheet != null) return range.ReadSheet(sheet);

                // Throw argument exception as the worksheet name is not found.
                // Provide a list of all the worksheets in the file.
                string message = $"The sheet '{worksheet}' was not found in file {fullFilePath}.";
                string allSheets = string.Join(", ", workbook.WorksheetNames.Select(name => $"'{name}'"));
                message += $" Available sheets are: {allSheets}";
                throw new ArgumentException(message);
            }
            return range.ReadSheet(sheet);
        }

    }

    public interface ExcelRange
    {
        public ListExpression ReadSheet(XlsxWorksheet sheet);
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

        public ListExpression ReadSheet(XlsxWorksheet sheet)
        {
            List<string> headers = new List<string>();

            List<Expression> objects = new List<Expression>();

            for (var row = _startRow; row <= _endRow; row++)
            {
                if (row == _startRow)
                {
                    for (var column = _startCol; column <= _endCol; column++)
                    {
                        headers.Add(sheet.GetCell(row, column).Text);
                    }
                }
                else
                {
                    IdfPlusObjectExpression objectExpression = new IdfPlusObjectExpression();
                    var index = 0;
                    for (var column = _startCol; column <= _endCol; column++)
                    {
                        var header = headers[index];
                        objectExpression.Members[header] = sheet.GetCell(row, column).Value;

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

        public ListExpression ReadSheet(XlsxWorksheet sheet)
        {
            List<string> headers = new List<string>();

            List<Expression> objects = new List<Expression>();

            var row = _startRow;
            var col = _startCol;
            while (!string.IsNullOrWhiteSpace(sheet.GetCell(row, col).Text))
            {
                headers.Add(sheet.GetCell(row, col).Text);
                col++;
            }

            row++;

            while (!RecordValues(row, _startCol, _startCol + headers.Count - 1, sheet).All(string.IsNullOrWhiteSpace))
            {
                IdfPlusObjectExpression objectExpression = new IdfPlusObjectExpression();
                var index = 0;
                for (var column = _startCol; column < _startCol + headers.Count(); column++)
                {
                    var header = headers[index];
                    objectExpression.Members[header] = sheet.GetCell(row, column).Value;

                    index++;
                }
                objects.Add(objectExpression);
                row++;
            }

            return new ListExpression(objects);
        }

        private List<string> RecordValues(int row, int startColumn, int endColumn, XlsxWorksheet sheet)
        {
            List<string> recordValues = new List<string>();
            for (int col = startColumn; col < endColumn; col++)
            {
                recordValues.Add(sheet.GetCell(row, col).Text);
            }

            return recordValues;
        }

    }

    public class SheetDimensionRange : ExcelRange
    {
        public ListExpression ReadSheet(XlsxWorksheet sheet)
        {
            int startRow = sheet.StartRow;
            int endRow = sheet.EndRow;
            int startColumn = sheet.StartColumn;
            int endColumn = sheet.EndColumn;

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

    /// <summary>
    /// A single cell read out of an xlsx worksheet. <see cref="Text"/> is the string form used for
    /// header names and blank-cell detection, while <see cref="Value"/> carries the correctly typed
    /// expression (numeric, boolean, or string) taken directly from the cell's stored value.
    /// </summary>
    public class XlsxCell
    {
        public string Text { get; }
        public Expression Value { get; }

        private XlsxCell(string text, Expression value)
        {
            Text = text;
            Value = value;
        }

        public static XlsxCell OfString(string text) => new XlsxCell(text, new StringExpression(text));
        public static XlsxCell OfNumber(double value, string text) => new XlsxCell(text, new NumericExpression(value));
        public static XlsxCell OfBoolean(bool value) => new XlsxCell(value ? "TRUE" : "FALSE", new BooleanExpression(value));
    }

    /// <summary>
    /// An in-memory representation of a single worksheet, built from the raw sheet XML. Cells are
    /// stored sparsely by (row, column); the dimension properties describe the used range.
    /// </summary>
    public class XlsxWorksheet
    {
        private readonly Dictionary<(int Row, int Col), XlsxCell> _cells;

        public string Name { get; }
        public int StartRow { get; }
        public int EndRow { get; }
        public int StartColumn { get; }
        public int EndColumn { get; }

        public XlsxWorksheet(string name, Dictionary<(int Row, int Col), XlsxCell> cells)
        {
            Name = name;
            _cells = cells;

            if (cells.Count == 0)
            {
                // An empty range: loops over [StartRow, EndRow] / [StartColumn, EndColumn] do nothing.
                StartRow = 1;
                StartColumn = 1;
                EndRow = 0;
                EndColumn = 0;
            }
            else
            {
                StartRow = cells.Keys.Min(k => k.Row);
                EndRow = cells.Keys.Max(k => k.Row);
                StartColumn = cells.Keys.Min(k => k.Col);
                EndColumn = cells.Keys.Max(k => k.Col);
            }
        }

        public XlsxCell GetCell(int row, int col) =>
            _cells.TryGetValue((row, col), out XlsxCell cell) ? cell : XlsxCell.OfString("");
    }

    /// <summary>
    /// Reads an .xlsx file (an OOXML zip package) directly, without any third-party library. The
    /// workbook, its relationships, the shared string table, and worksheet parts are all plain XML
    /// inside the zip; here we open the archive and parse those parts on demand.
    /// </summary>
    public class XlsxWorkbook : IDisposable
    {
        private const string Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string DocRel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private const string PackageRel = "http://schemas.openxmlformats.org/package/2006/relationships";

        private readonly ZipArchive _archive;
        private readonly List<string> _sharedStrings;
        private readonly List<(string Name, string Path)> _sheets;

        private XlsxWorkbook(ZipArchive archive)
        {
            _archive = archive;
            _sharedStrings = ReadSharedStrings(archive);
            _sheets = ReadSheetIndex(archive);
        }

        public static XlsxWorkbook Open(string path) => new XlsxWorkbook(ZipFile.OpenRead(path));

        public IEnumerable<string> WorksheetNames => _sheets.Select(s => s.Name);

        public XlsxWorksheet FirstWorksheet()
        {
            if (_sheets.Count == 0)
                throw new ArgumentException("The workbook does not contain any worksheets.");
            return ReadWorksheet(_sheets[0]);
        }

        public XlsxWorksheet GetWorksheet(string name)
        {
            foreach (var sheet in _sheets)
                if (sheet.Name == name) return ReadWorksheet(sheet);
            return null;
        }

        private XlsxWorksheet ReadWorksheet((string Name, string Path) sheet)
        {
            ZipArchiveEntry entry = _archive.GetEntry(sheet.Path);
            if (entry == null)
                throw new ArgumentException($"Could not find the worksheet part '{sheet.Path}' inside the workbook.");

            XNamespace ns = Main;
            XDocument doc = LoadXml(entry);

            var cells = new Dictionary<(int Row, int Col), XlsxCell>();
            XElement sheetData = doc.Root?.Element(ns + "sheetData");
            if (sheetData != null)
            {
                foreach (XElement cellElement in sheetData.Elements(ns + "row").Elements(ns + "c"))
                {
                    string reference = cellElement.Attribute("r")?.Value;
                    if (reference == null) continue;
                    (int row, int col) = ParseCellReference(reference);
                    cells[(row, col)] = ReadCell(cellElement, ns);
                }
            }

            return new XlsxWorksheet(sheet.Name, cells);
        }

        private XlsxCell ReadCell(XElement cell, XNamespace ns)
        {
            // The 't' attribute gives the cell type; absent means a number. The stored value lives in
            // <v>, except inline strings which sit in <is>.
            string type = cell.Attribute("t")?.Value;
            switch (type)
            {
                case "s":
                {
                    string raw = cell.Element(ns + "v")?.Value;
                    if (raw == null) return XlsxCell.OfString("");
                    int index = int.Parse(raw, CultureInfo.InvariantCulture);
                    string text = index >= 0 && index < _sharedStrings.Count ? _sharedStrings[index] : "";
                    return XlsxCell.OfString(text);
                }
                case "inlineStr":
                {
                    XElement inline = cell.Element(ns + "is");
                    string text = inline == null ? "" : ConcatText(inline, ns);
                    return XlsxCell.OfString(text);
                }
                case "str": // result of a string-valued formula
                case "e":   // error value, e.g. #DIV/0!
                    return XlsxCell.OfString(cell.Element(ns + "v")?.Value ?? "");
                case "b":
                    return XlsxCell.OfBoolean(cell.Element(ns + "v")?.Value == "1");
                default: // number
                {
                    string raw = cell.Element(ns + "v")?.Value;
                    if (string.IsNullOrEmpty(raw)) return XlsxCell.OfString("");
                    double value = double.Parse(raw, CultureInfo.InvariantCulture);
                    return XlsxCell.OfNumber(value, raw);
                }
            }
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var strings = new List<string>();
            ZipArchiveEntry entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null) return strings;

            XNamespace ns = Main;
            XDocument doc = LoadXml(entry);
            foreach (XElement si in doc.Root?.Elements(ns + "si") ?? Enumerable.Empty<XElement>())
                strings.Add(ConcatText(si, ns));
            return strings;
        }

        private static List<(string Name, string Path)> ReadSheetIndex(ZipArchive archive)
        {
            ZipArchiveEntry entry = archive.GetEntry("xl/workbook.xml");
            if (entry == null)
                throw new ArgumentException("The file does not appear to be a valid xlsx workbook (missing xl/workbook.xml).");

            XNamespace ns = Main;
            XNamespace r = DocRel;
            Dictionary<string, string> rels = ReadWorkbookRelationships(archive);
            XDocument doc = LoadXml(entry);

            var sheets = new List<(string, string)>();
            XElement sheetsElement = doc.Root?.Element(ns + "sheets");
            if (sheetsElement == null) return sheets;

            foreach (XElement sheet in sheetsElement.Elements(ns + "sheet"))
            {
                string name = sheet.Attribute("name")?.Value;
                string relId = sheet.Attribute(r + "id")?.Value;
                if (name == null || relId == null) continue;
                if (rels.TryGetValue(relId, out string target))
                    sheets.Add((name, target));
            }
            return sheets;
        }

        private static Dictionary<string, string> ReadWorkbookRelationships(ZipArchive archive)
        {
            var map = new Dictionary<string, string>();
            ZipArchiveEntry entry = archive.GetEntry("xl/_rels/workbook.xml.rels");
            if (entry == null) return map;

            XNamespace pr = PackageRel;
            XDocument doc = LoadXml(entry);
            foreach (XElement relationship in doc.Root?.Elements(pr + "Relationship") ?? Enumerable.Empty<XElement>())
            {
                string id = relationship.Attribute("Id")?.Value;
                string target = relationship.Attribute("Target")?.Value;
                if (id == null || target == null) continue;
                map[id] = ResolveTarget(target);
            }
            return map;
        }

        // Relationship targets are relative to the xl/ folder (or absolute from the package root).
        private static string ResolveTarget(string target)
        {
            string combined = target.StartsWith("/") ? target.TrimStart('/') : "xl/" + target;
            var parts = new List<string>();
            foreach (string segment in combined.Split('/'))
            {
                if (segment == "" || segment == ".") continue;
                if (segment == "..")
                {
                    if (parts.Count > 0) parts.RemoveAt(parts.Count - 1);
                }
                else parts.Add(segment);
            }
            return string.Join("/", parts);
        }

        // A string item (<si>) or inline string (<is>) holds its text in one or more <t> elements,
        // split across runs (<r>) when the text is rich-formatted. Concatenate them all.
        private static string ConcatText(XElement element, XNamespace ns) =>
            string.Concat(element.Descendants(ns + "t").Select(t => t.Value));

        private static (int Row, int Col) ParseCellReference(string reference)
        {
            int i = 0;
            while (i < reference.Length && char.IsLetter(reference[i])) i++;
            int col = reference.Substring(0, i).ExcelColumnNameToInt();
            int row = int.Parse(reference.Substring(i), CultureInfo.InvariantCulture);
            return (row, col);
        }

        private static XDocument LoadXml(ZipArchiveEntry entry)
        {
            using Stream stream = entry.Open();
            return XDocument.Load(stream);
        }

        public void Dispose() => _archive?.Dispose();
    }
}
