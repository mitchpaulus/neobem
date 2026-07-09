using System.Text;
namespace src;

public static class Help
{
    public static string Text()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("USAGE: nbem [options..] [input file]\n");
        builder.Append("Compile Neobem file to EnergyPlus or DOE-2 input files.\n");
        builder.Append("\n");
        builder.Append("With no [input file], input is read from file named 'in.nbem' in the current directory.\n");
        builder.Append("If the input file is '-', input is read from standard input rather than from a file.\n");
        builder.Append("\n");
        builder.Append("OPTIONS:\n");
        builder.Append("\n");
        builder.Append("    --doe2              Parse input file in DOE-2 Building Description Language format\n");
        builder.Append("    --deps <filename>   Print dependencies encountered in the input file to the specified file\n");
        builder.Append("-h, --help              Show this help and exit\n");
        builder.Append("-f, --fmt               Format file instead of compiling\n");
        builder.Append("    --flags <flags>     Set flags for simulation. Multiple flags can be set, comma separated.\n");
        builder.Append("    --objects           Print EnergyPlus objects in TSV format\n");
        builder.Append("-o, --output <filename> Output file name. Output is printed to standard output by default.\n");
        builder.Append("    --tokens            Print lexed tokens for debugging\n");
        builder.Append("    --lsp               Start the language server on standard input/output\n");
        builder.Append("    --tree              Print parse tree in Lisp format for debugging\n");
        builder.Append("-v, --version           Print version number and exit\n");
        return builder.ToString();
    }
}
