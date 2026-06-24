#!/bin/sh

# Move all stdout to stderr
exec 1>&2

redo-ifchange ../csharp || exit 1

mkdir -p publish
rm -rf publish/*

# Build two flavors for every supported runtime:
#   * self-contained        -> bundles the .NET runtime, no install required
#                              (published as "<runtime>.zip")
#   * framework-dependent   -> smaller, requires a matching .NET runtime to be
#                              installed (published as "<runtime>-framework-dependent.zip")
#
# See https://github.com/dotnet/sdk/issues/5575#issuecomment-271062056
# for -p:DebugType=None. This prevents the .pdb files from being added to the
# publish output.
for runtime in win-x64 win-x86 win-arm win-arm64 linux-x64 linux-musl-x64 linux-arm linux-arm64 osx-x64; do
    # Self-contained
    mkdir -p publish/"$runtime"
    rm -rf publish/"$runtime"/*
    dotnet publish -r "$runtime" -o publish/"$runtime" -c Release -p:DebugType=None --self-contained

    zip -r -j publish/"$runtime".zip publish/"$runtime"/*
    rm -rf publish/"$runtime"

    # Framework-dependent
    fdd="$runtime"-framework-dependent
    mkdir -p publish/"$fdd"
    rm -rf publish/"$fdd"/*
    dotnet publish -r "$runtime" -o publish/"$fdd" -c Release -p:DebugType=None --self-contained false

    zip -r -j publish/"$fdd".zip publish/"$fdd"/*
    rm -rf publish/"$fdd"
done
