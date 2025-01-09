redo-ifchange doc.html doc.pdf neobem.css

if test -z "$NEOBEMDIR"; then
    printf "NEOBEMDIR not set\n"
    exit 1
fi

rsync doc.html "$NEOBEMDIR"/neobem.html
rsync -a img_neobem "$NEOBEMDIR"
rsync neobem.css "$NEOBEMDIR"/neobem.css
rsync doc.pdf "$NEOBEMDIR"/neobem.pdf
