redo-ifchange ../kde-syntax/*.xml contents.md 
pandoc \
    --syntax-definition ../kde-syntax/neobem.xml \
    --syntax-definition ../kde-syntax/console.xml \
    -o "$3" \
    --to html \
    contents.md > /dev/null
