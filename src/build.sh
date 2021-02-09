#!/bin/sh
dotnet publish -r win-x64   -c Release --self-contained
dotnet publish -r linux-x64 -c Release --self-contained
dotnet publish -r osx-x64   -c Release --self-contained

rm -f zips/*

zip -j zips/bemp_win-x64_"$1".zip bin/Release/netcoreapp3.1/win-x64/publish/*
zip -j zips/bemp_linux-x64_"$1".zip bin/Release/netcoreapp3.1/linux-x64/publish/*
zip -j zips/bemp_osx-x64_"$1".zip bin/Release/netcoreapp3.1/osx-x64/publish/*
