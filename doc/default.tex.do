#!/bin/sh

redo-ifchange "$2".md
redo-ifchange ../kde-syntax/neobem.xml ../kde-syntax/console.xml
pandoc --syntax-definition ../kde-syntax/neobem.xml \
       --syntax-definition ../kde-syntax/console.xml \
       -t latex "$2".md
