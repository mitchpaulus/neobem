redo-ifchange "$2".md
pandoc --syntax-definition ../kde-syntax/bemp.xml -t latex "$2".md
