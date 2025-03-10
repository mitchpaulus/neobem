#!/bin/sh

# Compile both C# and Java
java -Dfile.encoding=UTF-8 -jar "$ANTLR_JAR" -Dlanguage=CSharp  ./*.g4
java -Dfile.encoding=UTF-8 -jar "$ANTLR_JAR" ./*.g4
javac ./*.java
