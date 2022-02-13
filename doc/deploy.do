redo-ifchange doc.html doc.pdf main.css

if test -z "$NEOBEMDIR"; then
    printf "NEOBEMDIR not set\n"
    exit 1
fi

rsync doc.html "$NEOBEMDIR"/manual.html
rsync -a img "$NEOBEMDIR"
rsync main.css "$NEOBEMDIR"/main.css
rsync doc.pdf "$NEOBEMDIR"/neobem.pdf
