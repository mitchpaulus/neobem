redo-ifchange "$2".md

pandoc \
    --syntax-definition ../kde-syntax/neobem.xml \
    --syntax-definition ../kde-syntax/console.xml \
    -o "$3" \
    --to html \
    "$2".md > /dev/null
