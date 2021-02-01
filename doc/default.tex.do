redo-ifchange "$2".md
redo-ifchange ../kde-syntax/bemp.xml
pandoc --syntax-definition ../kde-syntax/bemp.xml -t latex "$2".md
