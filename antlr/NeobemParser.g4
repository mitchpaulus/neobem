parser grammar NeobemParser;

options {
    language=CSharp;
    tokenVocab=NeobemLexer;
}


variable_declaration : IDENTIFIER EQUALS expression;

// See pg. 41 of Antlr reference
expression :   expression MEMBER_ACCESS expression        # MemberAccessExp
             | funcexp=expression function_application    # FunctionExp
             | <assoc=right> expression CARET expression  # Exponientiate
             | expression op=(MULTOP|DIVIDEOP) expression # MultDivide
             | expression op=(PLUSOP|MINUSOP) expression  # AddSub
             | expression boolean_exp_operator expression # BooleanExp
             | expression op=(AND_OP|OR_OP) expression    # LogicExp
             | expression RANGE_OPERATOR expression         # RangeExp
             | STRING                                     # StringExp
             | NUMERIC                                    # NumericExp
             | BOOLEAN_LITERAL_TRUE                       # BooleanLiteralTrueExp
             | BOOLEAN_LITERAL_FALSE                      # BooleanLiteralFalseExp
             | IDENTIFIER                                 # VariableExp
             | list                                       # ListExp
             | if_exp                                     # IfExp
             | idfplus_object                             # ObjExp
             | lambda_def                                 # LambdaExp
             | let_binding                                # LetBindingExp
             | inline_table                               # InlineTable
             | expression op=(MAP_OPERATOR|PIPE_OPERATOR) expression # MapPipeExp
             | LPAREN expression RPAREN                   # ParensExp
             ;

function_application : LPAREN RPAREN |
                       LPAREN function_parameter (COMMA function_parameter)* RPAREN ;

function_parameter : expression ;

boolean_exp_operator : LESSTHAN |
                       GREATERTHAN |
                       EQUAL_TO|NOT_EQUAL_TO |
                       LESS_THAN_OR_EQUAL_TO |
                       GREATER_THAN_OR_EQUAL_TO ;

if_exp : IF expression THEN expression ELSE expression ;

lambda_def : FUNCTION_BEGIN IDENTIFIER* LCURLY
               (expression | function_statement*) RCURLY ;

return_statement : RETURN expression ;

idfplus_object : LCURLY (idfplus_object_property_def)
                   (COMMA idfplus_object_property_def)* COMMA?
                   RCURLY |
                 LCURLY RCURLY;

idfplus_object_property_def : expression STRUCT_SEP expression ;

list :  LSQUARE RSQUARE |
        LSQUARE expression (COMMA expression)* COMMA? RSQUARE ;

import_statement : IMPORT expression import_option* ;

import_option : AS IDENTIFIER                #AsOption   |
                ONLY LPAREN IDENTIFIER+ RPAREN #OnlyOption |
                NOT LPAREN IDENTIFIER+ RPAREN  #NotOption  ;

export_statement : EXPORT LPAREN IDENTIFIER+ RPAREN ;

print_statment : PRINT expression ;

log_statement : LOG expression ;

inline_table : INLINE_TABLE_BEGIN_END_SEP
               inline_table_header
               inline_table_header_separator
               inline_table_data_row+
               INLINE_TABLE_BEGIN_END_SEP ;

inline_table_header : STRING (INLINE_TABLE_COL_SEP STRING)* ;

inline_table_header_separator :
  INLINE_TABLE_BEGIN_END_SEP
  (INLINE_TABLE_COL_SEP INLINE_TABLE_BEGIN_END_SEP)* ;

inline_table_data_row : expression (INLINE_TABLE_COL_SEP expression)* ;


function_statement :  COMMENT   # FunctionIdfComment
                    | object    # FunctionObjectDeclaration
                    | variable_declaration # FunctionVariableDeclaration
                    | print_statment # FunctionPrintStatement
                    | return_statement # ReturnStatement
                    | log_statement    # FunctionLogStatement
                    ;

base_idf : COMMENT                # IdfComment
           | object               # ObjectDeclaration
           | variable_declaration # VariableDeclaration
           | import_statement     # ImportStatement
           | export_statement     # ExportStatment
           | print_statment       # PrintStatment
           | log_statement        # LogStatement
           ;

let_binding : LET IDENTIFIER EQUALS expression
              (COMMA IDENTIFIER EQUALS expression)* COMMA?
              IN let_expression ;

let_expression : expression ;

object : OBJECT_TYPE OBJECT_COMMENT?
           (FIELD_SEP OBJECT_COMMENT? (FIELD OBJECT_COMMENT?)?)+
           OBJECT_TERMINATOR ;

idf : (base_idf)* EOF;

