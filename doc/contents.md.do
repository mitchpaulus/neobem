redo-ifchange intro.md \
    getting_started.md \
    tutorials.md \
    formatting.md \
    reference.md \
    design_goals.md \
    faq.md \
    thank_yous_and_inspirations.md

for file in intro.md getting_started.md tutorials.md formatting.md reference.md design_goals.md faq.md thank_yous_and_inspirations.md; do
    cat $file
    echo "" # Newline between files
done
