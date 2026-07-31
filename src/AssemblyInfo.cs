// This is to remove Antlr compiler warnings.
// References:
//https://github.com/tunnelvisionlabs/antlr4cs/issues/10#issuecomment-66999851

using System;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(false)]
[assembly: InternalsVisibleTo("test")]
// The GUI drives the same language-service logic as the LSP server, but
// in-process instead of over stdio.
[assembly: InternalsVisibleTo("nbem-gui")]
