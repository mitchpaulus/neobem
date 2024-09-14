#!/bin/sh

redo-ifchange version.txt

# This shell script generates the C# code for help text.
cat <<EOF
namespace src
{
    public static class Version
    {
        public static string Num()
        {
EOF

printf "            return \"%s\";\n" "$(cat version.txt)"

cat <<EOF
        }
    }
}

EOF
