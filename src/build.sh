#!/bin/sh
redo-ifchange ../csharp || exit 1

rm -rf publish/*

for runtime in win-x64 win-x86 win-arm win-arm64 linux-x64 linux-musl-x64 linux-arm linux-arm64 osx-x64; do
    mkdir -p publish/"$runtime"
    rm -rf publish/"$runtime"/*
    # See https://github.com/dotnet/sdk/issues/5575#issuecomment-271062056
    # for -p:DebugType=None. This prevent the .pdb files from being added to the publish output
    dotnet publish -r "$runtime" -o publish/"$runtime" -c Release -p:DebugType=None --self-contained

    zip -r -j publish/"$runtime".zip -r publish/"$runtime"/*
    rm -rf publish/"$runtime"
done

redo ../doc/all || exit 1

cp ../doc/doc.pdf publish/neobem.pdf
