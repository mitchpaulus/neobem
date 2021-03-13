#!/bin/sh

redo-ifchange "$2.inc.md"
awk -f get_includes.awk "$2.inc.md" | xargs redo-ifchange
awk -f replace_includes.awk "$2.inc.md"
