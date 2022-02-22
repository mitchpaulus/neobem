#!/bin/sh

# Compile both C# and Java
java -jar /usr/local/lib/antlr-4.9.3-complete.jar -Dlanguage=CSharp  ./*.g4

java -jar /usr/local/lib/antlr-4.9.3-complete.jar ./*.g4
javac ./*.java
