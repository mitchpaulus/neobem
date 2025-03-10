#!/bin/sh
set -e
redo-ifchange "$2".nbem
nbem "$2".nbem
