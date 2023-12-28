#!/bin/sh

exec 1>&2
redo-ifchange antlr/compiled antlr/excelrange/compiled antlr/idf/compiled Help.cs Version.cs
dotnet build
