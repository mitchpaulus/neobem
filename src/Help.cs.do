#!/bin/sh
cat <<EOF
using System.Text;
namespace src {

    public static class Help {
        public static string Text() {
            StringBuilder builder = new StringBuilder();
EOF

awk '
{
    print "            builder.Append(\"" $0 "\\n\");"
}
' help.txt


cat <<EOF
            return builder.ToString();
        }
    }
}

EOF
