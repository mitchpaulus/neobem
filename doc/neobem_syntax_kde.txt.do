#!/bin/sh
if test -z "$REPOS"; then
    printf "Environment variable REPOS is not set.\n"
    exit 1
fi

if test -f "$REPOS/neobem-kde-syntax/neobem.xml"; then
    printf  "%s/neobem-kde-syntax/neobem.xml\n" "$REPOS"
else
    printf "Could not find file: '%s/neobem-kde-syntax/neobem.xml'\n" "$REPOS"
    exit 1
fi
