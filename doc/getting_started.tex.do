#!/bin/sh

redo-ifchange 'getting_started.md' 'replace_includes.awk' 'help_output.md' '../kde-syntax/neobem.xml' '../kde-syntax/console.xml'

awk -f replace_includes.awk getting_started.md | \
pandoc --syntax-definition ../kde-syntax/neobem.xml \
       --syntax-definition ../kde-syntax/console.xml \
       -t latex
