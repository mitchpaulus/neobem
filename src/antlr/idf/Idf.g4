grammar Idf;

COMMENT : '!' .*? '\r'? '\n' -> skip ;
FIELDSEP : ',' ;
TERMINATOR : ';' ;

idfFile : idfObject* EOF ;
idfObject : idfHeader FIELDSEP IDFFIELD? (FIELDSEP IDFFIELD?)* TERMINATOR ;
idfHeader : IDFFIELD ;
/* idfFields : IDFFIELD (',' IDFFIELDs)* ; */

IDFFIELD : ~[ \t,;!\r\n] ~[,;!\r\n]* ;

WS : [ \t\r\n]+ -> skip ;
