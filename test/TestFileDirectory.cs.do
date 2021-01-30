#!/bin/sh

DIR="$(pwd)"

cat <<EOF
namespace test
{
    public static class TestDir
    {
        public static string Dir = "$DIR";
    }
}
EOF
