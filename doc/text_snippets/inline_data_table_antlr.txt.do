antlr_dir=../../antlr
redo-ifchange "$antlr_dir"/NeobemLexer.g4  "$antlr_dir"/NeobemParser.g4
awk 'tolower($0) ~ /inline.*:/, /;/ { print;  if ($0 ~ /;/) { print ""  } }' "$antlr_dir"/NeobemParser.g4 "$antlr_dir"/NeobemLexer.g4
