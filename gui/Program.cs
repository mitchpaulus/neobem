using System;
using System.IO;
using System.Linq;
using Avalonia;
using gui.Parsing;

namespace gui;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        // Headless mode for smoke testing and scripting: print the structure
        // tree to stdout instead of launching the GUI.
        if (args.Contains("--dump"))
        {
            string? file = args.FirstOrDefault(a => a != "--dump");
            if (file is null || !File.Exists(file))
            {
                Console.Error.Write("usage: nbem-gui --dump <file.nbem>\n");
                return 1;
            }
            NeobemDocument doc = NeobemDocument.ParseFile(file);
            foreach (Diagnostic diagnostic in doc.Diagnostics) Console.Error.Write(diagnostic.Display + "\n");
            foreach (SymbolNode symbol in doc.Symbols) Dump(symbol, 0);
            return doc.Diagnostics.Count == 0 ? 0 : 1;
        }

        // Headless probe of the language features the GUI exposes, so they can
        // be smoke tested without a display.
        if (args.Contains("--query"))
        {
            string[] rest = args.Where(a => a != "--query").ToArray();
            if (rest.Length != 2 || !File.Exists(rest[0]) || !TryParsePosition(rest[1], out int line, out int column))
            {
                Console.Error.Write("usage: nbem-gui --query <file.nbem> <line>:<column>\n");
                return 1;
            }
            return Query(rest[0], line, column);
        }

        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static bool TryParsePosition(string text, out int line, out int column)
    {
        line = column = 0;
        string[] parts = text.Split(':');
        return parts.Length == 2 && int.TryParse(parts[0], out line) && int.TryParse(parts[1], out column);
    }

    // Prints, for a 1-based line:column, everything the GUI would show for that
    // caret position.
    private static int Query(string file, int line, int column)
    {
        NeobemDocument doc = NeobemDocument.ParseFile(file);
        LanguageService language = doc.Language;
        int offset = language.ToOffset(line - 1, column - 1);

        Antlr4.Runtime.IToken? token = language.FindToken(offset);
        Console.Write($"token: {(token is null ? "(none)" : $"{LanguageService.DescribeTokenType(token)} '{token.Text?.Trim()}'")}\n");

        HoverInfo? hover = language.FindHover(offset);
        Console.Write($"hover: {(hover is null ? "(none)" : hover.Markdown.Replace("\n", " "))}\n");

        Console.Write("definitions:\n");
        foreach (LocationResult definition in language.FindDefinitions(offset)) Console.Write($"  {definition.Display}\n");

        Console.Write("references:\n");
        foreach (LocationResult reference in language.FindReferences(offset)) Console.Write($"  {reference.Display}\n");

        Console.Write("completions:\n");
        foreach (CompletionEntry completion in language.FindCompletions(offset)) Console.Write($"  {completion.Display}\n");

        return 0;
    }

    private static void Dump(SymbolNode node, int depth)
    {
        Console.Write($"{new string(' ', depth * 2)}{node.Glyph,-4} {node.Name} {node.Detail} (line {node.Line})\n");
        foreach (SymbolNode child in node.Children) Dump(child, depth + 1);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
