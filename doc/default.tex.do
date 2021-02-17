redo-ifchange "$2".md
redo-ifchange ../kde-syntax/neobem.xml
pandoc --syntax-definition ../kde-syntax/neobem.xml -t latex "$2".md
