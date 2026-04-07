This is a C# repository of a programming language called Neobem.
Neobem is a domain specific language for generating EnergyPlus idf files.
If you have web search, you can find the latest documentation at: <https://mitchellt.com/neobem>.

It is a superset of the idf file syntax.

It is written in C#. The grammar, lexer, and parser are built with ANTLR.
The key ANTLR grammar files are:

```
src/antlr/NeobemLexer.g4
src/antlr/NeobemParser.g4
```

## Building

You can find instructions for building in the main `README.md` file.

## Documentation

All documentation is in the `doc` directory.
Most of the main documentation files are written in `*.inc.md` files.
It can then be compiled to a PDF through Latex and to HTML.

`redo` is the build system.

You can rebuild the HTML or PDF with:

```
redo doc.html
redo doc.pdf
```
