lexer grammar NeobemLexer;
@lexer::header  {using src;}
@lexer::members {public FileType FileType = FileType.Idf;}
options {
    language=CSharp;
}

EQUALS : '=' ;

// Arithmetic Operators
LPAREN : '(' ;
RPAREN : ')' ;
CARET : '^' ;
MULTOP : '*' ;
DIVIDEOP : '/' ;
PLUSOP : '+' ;
MINUSOP : '-' ;

// Comparison Operators
LESSTHAN : '<' ;
GREATERTHAN : '>' ;
LESS_THAN_OR_EQUAL_TO : '<=' ;
GREATER_THAN_OR_EQUAL_TO : '>=' ;

// Equality Operators
EQUAL_TO : '==' ;
NOT_EQUAL_TO : '!=' ;

MAP_OPERATOR : '|=' ;
FILTER_OPERATOR : '|>' ;
PIPE_OPERATOR : '->' ;

AND_OP : 'and' ;
OR_OP : 'or' ;

IF : 'if' ;
THEN : 'then' ;
ELSE : 'else' ;

FUNCTION_BEGIN : '\\' | 'λ' ;

LCURLY : '{' ;
RCURLY : '}' ;

RETURN : 'return' ;

LSQUARE : '[' ;
RSQUARE : ']' ;

IMPORT : 'import' ;
AS : 'as' ;
ONLY : 'only' ;
NOT : 'not' ;
EXPORT : 'export' ;
PRINT : 'print' ;
LOG : 'log' ;

// Let expressions
LET : 'let' ;
IN : 'in' ;

RANGE_OPERATOR : '..' ;
MEMBER_ACCESS : '.' ;

STRUCT_SEP : ':' ;

COMMA : ',' ;

BCL_ID : 'bcl:' ;
fragment HEX_CHAR : [0-9a-fA-F] ;

UUID : HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR
       HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR '-'
       HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR '-'
       HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR '-'
       HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR '-'
       HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR HEX_CHAR ;

// 3 or more
//  - underscores,
//  - 'Box Drawings Light Horizontal' (U+2500),
//  - 'Box Drawings Light Down and Horizontal' (U+252C)
//  - 'Box Drawings Light Up and Horizontal' (U+2534)
//  - or hyphens
//  to begin/end inline data table.
// Essentially, for horizontal portions, any "horizontal like"
// even though a well formatted table will use the sensible character.
INLINE_TABLE_BEGIN_END_SEP :  [─┴┬_-] [─┴┬_-] [─┴┬_-] [─┴┬_-]* ;

// Column separators are either
//  - normal pipe
//  - 'Box Drawings Light Vertical' (U+2502)
//  - 'Box Drawings Light Vertical and Horizontal' (U+253C)
INLINE_TABLE_COL_SEP : '|' | '│' | '┼';

BOOLEAN_LITERAL_TRUE : 'true' | '✓' ;
BOOLEAN_LITERAL_FALSE : 'false' | '✗' ;

IDENTIFIER : [a-z][a-zA-Z0-9@_]* ;

COMMENT :  '!' .*? '\r'?'\n' ;

DOE2COMMENT : '$' .*? '\r'?'\n' ;

NEOBEM_COMMENT : '#' .*? '\r'?'\n' -> channel(1) ;

NUMERIC : '-'?(([1-9][0-9]*|'0')('.'[0-9]+)? |
          ('.'[0-9]+))([eE]'-'?[0-9]+)? ;

STRING : '\'' .*? '\'' ;

OBJECT_TYPE : [A-Z][a-zA-Z0-9:]* { FileType == FileType.Idf }? -> pushMode(IDFOBJECT)  ;

DOE2IDENTIFIER : [A-Z]([a-zA-Z0-9-] | '<' .*? '>')* { FileType == FileType.Doe2 }? -> pushMode(DOE2OBJECT)  ;
DOE2STRING_UNAME : '"' .*? '"' -> pushMode(DOE2OBJECT) ;

WS : [ \t\r\n]+ -> channel(2) ;

mode IDFOBJECT;

FIELD : ~[,!;$\r\n]+ ;
FIELD_SEP : ',' [ \t\r\n]* ;
OBJECT_COMMENT : '!' .*? '\r'?'\n' ;
OBJECT_TERMINATOR : (';' [ \t]* ('!' .*? '\r'?'\n')? | '$' ) -> popMode ;
OBJECT_WS : [ \t\r\n]+ -> skip ;

mode DOE2OBJECT;

// Technically, these separators are the same as blanks (see pg. 24 of DOE22 Volume 1 Basics)
// But we'll want to keep them for output.
DOE2_LIST_START : '(' ;
DOE2_LIST_END : ')' ;
DOE2_OBJECT_COMMENT : '$' .*? ('$' | '\r'?'\n') ;
DOE2_NEOBEM_COMMENT : '#' .*? '\r'?'\n' -> channel(1) ;
DOE2_STRING : '"' .*? '"' ;
DOE2_LITERAL : '*' .*? '*' ;
DOE2_TERMINATOR : '..' -> popMode ;
DOE2_FIELD_SEP :  [ \t\r\n=,]+ ;
DOE2_FIELD : (~[,= ()\t\r\n] | '<' .*? '>') + ;
