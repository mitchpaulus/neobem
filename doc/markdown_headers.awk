BEGIN { in_code = 0}

/```/ {
   if (in_code == 1) in_code = 0
   else in_code = 1
}

/^#+/ && in_code == 0 {


    match_index = match($0, /^#+/)

    header_depth = RLENGTH

    # Don't need to go to 4 levels deep
    if (header_depth > 3) next

    header_text = substr($0, RSTART + RLENGTH + 1)

    id_text = tolower(header_text)
    # https://pandoc.org/MANUAL.html#extension-auto_identifiers
    gsub(/`.*`/, "", id_text)
    gsub(/[^a-zA-Z0-9 _.-]/, "", id_text)
    gsub(/ +$/, "", id_text)
    gsub(/ +/, "-", id_text)
    gsub(/^[^a-zA-Z]+/, "", id_text)

    printf "<li class=\"header-%s\"><a class=\"header-link\" href=\"#%s\">%s</a></li>\n", header_depth, id_text, header_text

}
