redo-ifchange "$2".md
redo-ifchange neobem_syntax_kde.txt

# Read first line of neobem_syntax_kde.txt to variable
SYNTAX_HIGHLIGHT_FILE=$(head -n 1 neobem_syntax_kde.txt)
redo-ifchange "$SYNTAX_HIGHLIGHT_FILE" ../kde-syntax/console.xml

pandoc \
    --syntax-definition "$SYNTAX_HIGHLIGHT_FILE" \
    --syntax-definition ../kde-syntax/console.xml \
    -o "$3" \
    --to html \
    "$2".md > /dev/null
