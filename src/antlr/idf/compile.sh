#!/bin/sh

# Compile both C# and Java
java -jar "$ANTLR_JAR" -Dlanguage=CSharp  ./*.g4
java -jar "$ANTLR_JAR" ./*.g4
javac ./*.java
