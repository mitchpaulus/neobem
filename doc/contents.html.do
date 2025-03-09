#!/bin/sh
redo-ifchange syntax.xml
redo-ifchange ../kde-syntax/*.xml contents.md
pandoc \
    --syntax-definition "syntax.xml" \
    --syntax-definition ../kde-syntax/console.xml \
    -o "$3" \
    --to html \
    contents.md > /dev/null
