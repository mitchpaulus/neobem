#!/bin/sh

redo-ifchange 'README.template'

awk '
/INCLUDE/ {
    system("cat " $2)
    next
}
{
    print
}
' README.template
