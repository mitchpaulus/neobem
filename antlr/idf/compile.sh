#!/bin/sh

java -jar /usr/local/lib/antlr-4.8-complete.jar ./*.g4
java -jar /usr/local/lib/antlr-4.8-complete.jar -Dlanguage=CSharp -o ../../src/antlr-idf/ ./*.g4

javac *.java
