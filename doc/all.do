#!/bin/sh
set -e
redo doc.runtex
redo-ifchange doc.html doc.pdf
