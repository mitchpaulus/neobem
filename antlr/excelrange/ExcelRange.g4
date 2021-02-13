grammar ExcelRange;

options {
    language=CSharp;
}


range : (fullrange | startcell | startrowwithcols) ;

fullrange : COLUMN ROW ':' COLUMN ROW ;

startcell : COLUMN ROW ;

startrowwithcols : ROW ':' COLUMN ':' COLUMN ;

COLUMN : [A-Za-z]+ ;
ROW    : [1-9][0-9]* ;
