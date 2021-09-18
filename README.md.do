#!/bin/sh

redo-ifchange 'README.template' doc/intro.md doc/getting_started.md

# Need sed command to adjust the root relative path for certain links.
awk '
/INCLUDE/ {
    system("cat " $2)
    next
}
{
    print
}
' README.template | sed 's/img/doc\/img/' | pandoc --to gfm -
