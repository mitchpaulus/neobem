#!/bin/sh

DIR="$(pwd)"

cat <<EOF
// This file is generated. Edit at your own risk.
using System.IO;

namespace test
{
    public static class TestDir
    {
        public static string Dir = "$DIR/test_files";
        public static string LoadTestFiles = Path.Combine(Dir, "load_test_files");
    }
}
EOF
