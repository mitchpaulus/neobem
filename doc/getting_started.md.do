#!/bin/sh

redo-ifchange 'getting_started.inc.md' 'replace_includes.awk' 'help_output.md'
awk -f replace_includes.awk getting_started.inc.md
