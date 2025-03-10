#!/bin/sh
redo-ifchange "$2".md
redo-ifchange syntax.xml ../kde-syntax/console.xml
pandoc --syntax-definition syntax.xml \
       --syntax-definition ../kde-syntax/console.xml \
       -t latex "$2".md
