redo-ifchange html_template.html
awk -f get_includes.awk html_template.html | xargs redo-ifchange
awk -f replace_includes.awk html_template.html
