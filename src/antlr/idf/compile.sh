#!/bin/sh

# Compile both C# and Java
java "$ANTLR_JAR" -Dlanguage=CSharp  ./*.g4

java "$ANTLR_JAR" ./*.g4
javac ./*.java
