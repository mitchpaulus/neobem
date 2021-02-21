#!/bin/sh

redo-ifchange "$(which nbem)"

printf "\`\`\`console\nmp@mp-computer:~$ nbem -h\n"

nbem -h

printf "\`\`\`\n"
