lexer grammar NeobemLexer;

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

// Let expressions
LET : 'let' ;
IN : 'in' ;

MEMBER_ACCESS : '.' ;

STRUCT_SEP : ':' ;

COMMA : ',' ;

// 3 or more underscores to begin inline data table
INLINE_TABLE_BEGIN_END : '___' '_'*  ;

// Can optionally end inline table with overline character U+203E
INLINE_TABLE_END : '‾‾‾' '‾'* ;

// 3 or more hyphens separate table header entries
INLINE_TABLE_HEADER_SEP : '---' '-'* ;

INLINE_TABLE_COL_SEP : '|' ;

BOOLEAN_LITERAL : 'true' | 'false' | '✓' | '✗' ;

IDENTIFIER : [a-z][a-zA-Z0-9@_]* ;

COMMENT :  '!' .*? '\r'?'\n' ;

NEOBEM_COMMENT : '#' .*? '\r'?'\n' -> skip ;

NUMERIC : '-'?(([1-9][0-9]*|'0')('.'[0-9]+)? |
          ('.'[0-9]+))([eE]'-'?[0-9]+)? ;

STRING : '\'' .*? '\'' ;

OBJECT_TYPE : [A-Z][a-zA-Z0-9:]* -> pushMode(IDFOBJECT) ;

WS : [ \t\r\n]+ -> skip ;

mode IDFOBJECT;

FIELD : ~[,!;\r\n]+ ;
FIELD_SEP : ',' [ \t\r\n]* ;
OBJECT_COMMENT : '!' .*? '\r'?'\n' ;
OBJECT_TERMINATOR : ';' [ \t]* ('!' .*? '\r'?'\n')? -> popMode ;
OBJECT_WS : [ \t\r\n]+ -> skip ;
