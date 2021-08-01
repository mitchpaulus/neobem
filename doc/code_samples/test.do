#!/bin/sh

exec >&2

set -e

redo-ifchange *.nbem

for file in *.nbem; do
    printf "%s\n" "$file" >&2
    nbem "$file" > /dev/null
done
