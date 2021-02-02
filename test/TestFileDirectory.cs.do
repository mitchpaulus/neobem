#!/bin/sh

DIR="$(pwd)"

cat <<EOF
// This file is generated. Edit at your own risk.
namespace test
{
    public static class TestDir
    {
        public static string Dir = "$DIR/test_files";
    }
}
EOF
