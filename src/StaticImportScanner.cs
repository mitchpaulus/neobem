using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace src
{
    // Statically walks a parsed neobem file for import statements of the exact form
    //
    //     import 'string literal'
    //
    // and records the referenced files as dependencies. Because these paths are plain
    // string literals, they can be resolved without executing the file — so they are
    // captured even when runtime evaluation never reaches (or fails before) the import.
    //
    // Imports whose path is any other kind of expression (concatenation, variable, a
    // function call, an http URI, ...) are inherently dynamic and are left to the
    // runtime collection in IdfPlusVisitor.
    public static class StaticImportScanner
    {
        // Scan the given tree, resolving literal imports relative to baseDirectory, and
        // add every discovered file to Dependencies.Set. Recurses into imported files
        // that exist and parse, so nested literal imports are found too. The shared
        // Dependencies.Set doubles as the visited guard against cyclic imports.
        public static void Scan(NeobemParser.IdfContext tree, string baseDirectory, FileType fileType)
        {
            List<string> literalImports = CollectLiteralImports(tree);

            foreach (string literal in literalImports)
            {
                // http{s} imports have no meaningful local base directory and can't be read
                // statically, so skip them here. Match only a real URI scheme so a local
                // file whose name merely begins with "http" is still resolved as a file.
                if (literal.StartsWith("http://") || literal.StartsWith("https://")) continue;

                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(literal, baseDirectory);
                }
                catch (Exception)
                {
                    // A malformed or empty literal (e.g. `import ''`) can't be resolved to a
                    // path. Without --deps this would fail gracefully at runtime, so we must
                    // not let the static scan turn it into a crash - just skip it and leave
                    // the runtime to report the real error.
                    continue;
                }

                // Already seen (either via runtime collection or an earlier scan) - skip to
                // avoid re-parsing and to break import cycles.
                if (!Dependencies.Set.Add(fullPath)) continue;

                // Only recurse into files that actually exist and parse cleanly. Anything
                // else is still recorded above; we just can't dig deeper.
                if (!File.Exists(fullPath)) continue;

                NeobemParser.IdfContext childTree = TryParse(fullPath, fileType);
                if (childTree == null) continue;

                Scan(childTree, Path.GetDirectoryName(fullPath), fileType);
            }
        }

        private static List<string> CollectLiteralImports(NeobemParser.IdfContext tree)
        {
            LiteralImportListener listener = new();
            ParseTreeWalker walker = new();
            walker.Walk(listener, tree);
            return listener.Literals;
        }

        private static NeobemParser.IdfContext TryParse(string fullPath, FileType fileType)
        {
            try
            {
                AntlrInputStream inputStream = new(File.ReadAllText(fullPath));
                NeobemLexer lexer = new(inputStream) { FileType = fileType };
                lexer.RemoveErrorListeners();
                SimpleAntlrErrorListener lexerErrors = new();
                lexer.AddErrorListener(lexerErrors);

                CommonTokenStream tokens = new(lexer);
                NeobemParser parser = new(tokens);
                parser.RemoveErrorListeners();
                SimpleAntlrErrorListener parserErrors = new();
                parser.AddErrorListener(parserErrors);

                NeobemParser.IdfContext childTree = parser.idf();

                if (lexerErrors.Errors.Count > 0 || parserErrors.Errors.Count > 0) return null;
                return childTree;
            }
            catch
            {
                return null;
            }
        }

        private sealed class LiteralImportListener : NeobemParserBaseListener
        {
            public readonly List<string> Literals = new();

            public override void EnterImport_statement(NeobemParser.Import_statementContext context)
            {
                // Only the exact form `import 'literal'` - the import path expression is a
                // bare string literal (the StringExp alternative of the expression rule).
                if (context.expression() is NeobemParser.StringExpContext stringExp)
                {
                    string text = stringExp.GetText();
                    // STRING is a single-quoted token with no escaping, so the value is just
                    // the contents between the surrounding quotes.
                    Literals.Add(text.Substring(1, text.Length - 2));
                }
            }
        }
    }
}
