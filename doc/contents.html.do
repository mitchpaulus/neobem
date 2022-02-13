redo-ifchange neobem_syntax_kde.txt

# Read first line of neobem_syntax_kde.txt to variable
SYNTAX_HIGHLIGHT_FILE=$(head -n 1 neobem_syntax_kde.txt)

redo-ifchange "$SYNTAX_HIGHLIGHT_FILE"
redo-ifchange ../kde-syntax/*.xml contents.md
pandoc \
    --syntax-definition "$SYNTAX_HIGHLIGHT_FILE" \
    --syntax-definition ../kde-syntax/console.xml \
    -o "$3" \
    --to html \
    contents.md > /dev/null
