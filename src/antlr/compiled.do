set -e

redo-ifchange NeobemLexer.g4 NeobemParser.g4
./compile_grammer
date
