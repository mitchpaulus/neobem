grammar Idfplus;

options {
    language=CSharp;
}

/*variable_declaration : VARIABLE member_access* '=' expression;*/
variable_declaration : IDENTIFIER member_access* '=' expression;

// See pg. 41 of Antlr reference
expression :   expression member_access                                               # MemberAccessExp
             | funcexp=expression ( '(' ')' |  '(' expression (',' expression)* ')' ) # FunctionExp
             | <assoc=right> expression '^' expression                                # Exponientiate
             | expression op=('*'|'/') expression                                     # MultDivide
             | expression op=('+'|'-') expression                                     # AddSub
             | expression op=('<'|'>'|'=='|'!='|'<='|'>=') expression                 # BooleanExp
             | expression op=('and'|'or') expression                                  # LogicExp
             | STRING                                                                 # StringExp
             | NUMERIC                                                                # NumericExp
             | BOOLEAN_LITERAL                                                        # BooleanLiteralExp
             | IDENTIFIER                                                             # VariableExp
             | list                                                                   # ListExp
             | if_exp                                                                 # IfExp
             | idfplus_object                                                         # ObjExp
             | lambda_def                                                             # LambdaExp
             | let_binding
             | inline_table                                                           # InlineTable
             | '(' expression ')'                                                     # ParensExp
             ;

if_exp : 'if' expression 'then' expression 'else' expression ;

lambda_def : ('\\' | 'λ' ) IDENTIFIER* '{' (expression | function_statement*) '}' ;

return_statement : 'return' expression ;

idfplus_object : '{' (idfplus_object_property_def) (',' idfplus_object_property_def)* '}' ;

idfplus_object_property_def : IDENTIFIER ':' expression ;

list :  '[' ']' |
        '[' expression (',' expression)* ']' ;

import_statement : 'import' STRING ;

print_statment : 'print' expression ;

inline_table : INLINE_TABLE_BEGIN_END inline_table_header inline_table_header_separator inline_table_data_row+ INLINE_TABLE_BEGIN_END ;

// 3 or more underscores to begin
INLINE_TABLE_BEGIN_END : '___' '_'* ;

INLINE_TABLE_HEADER_SEP : '---' '-'* ;

inline_table_header : IDENTIFIER ('|' IDENTIFIER)* ;

inline_table_header_separator : INLINE_TABLE_HEADER_SEP ('|' INLINE_TABLE_HEADER_SEP)* ;

inline_table_data_row : expression ('|' expression)* ;


function_statement :  COMMENT   # FunctionIdfComment
                    | object    # FunctionObjectDeclaration
                    | variable_declaration # FunctionVariableDeclaration
                    | return_statement # ReturnStatement
                    ;

base_idf : COMMENT                # IdfComment
           | object               # ObjectDeclaration
           | variable_declaration # VariableDeclaration
           | import_statement     # ImportStatement
           | print_statment       # PrintStatment
           ;

idf : (base_idf)* EOF;

member_access : '.' IDENTIFIER ;

BOOLEAN_LITERAL : 'true' | 'false' | '✓' | '✗' ;

let_binding : 'let' variable_declaration (',' variable_declaration)* 'in' expression ;

/*object : OBJECT ;*/

object : OBJECT_TYPE ',' .*? OBJECT_TERMINATOR COMMENT? ;

IDENTIFIER : [a-z][a-zA-Z0-9_]* ;

COMMENT :  '!' .*? '\r'?'\n' ;

OBJECT_TERMINATOR : ';' ;

NUMERIC : '-'?(([1-9][0-9]*|'0')('.'[0-9]+)? | ('.'[0-9]+))([eE]'-'?[0-9]+)? ;

STRING : '\'' .*? '\'' ;

OBJECT_TYPE : [A-Z][a-zA-Z0-9:]* ;

WS : [ \t\r\n]+ -> skip ;

