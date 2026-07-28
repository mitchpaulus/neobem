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

        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
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
