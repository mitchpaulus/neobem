## General

Whitespace is generally ignored, expect within the object definitions and
comments.

## Data Types

There are two primitive expression types, the same as EnergyPlus

1. String expressions (or alpha in idf terminology)
2. Numeric expressions

There are 4 higher order expression types.

1. List expressions
2. Function expressions
3. Structure expressions
4. Boolean expressions

## Operators

- Exponentiation: '`^`'
    - Valid for numeric expressions only
- Plus '`+`'
    - Addition for numeric types
    - String concatenation for string types
        - `'Chiller ' + '1'` equals `'Chiller 1'`
    - List concatenation for list types
        - `[1, 2, 3] + [4, 5]` equals `[1, 2, 3, 4, 5]`
- Minus '`-`'
    - Subtraction for numeric types
- Multiplication '`*`'
    - Multiplication for numeric types
- Division '`/`'
    - Division for numeric types


## Inline Data

Grammar for inline data tables in Antlr form:

```
inline_table :
    INLINE_TABLE_BEGIN_END
    inline_table_header
    inline_table_header_separator
    inline_table_data_row+
    INLINE_TABLE_BEGIN_END ;

INLINE_TABLE_BEGIN_END : '___' '_'* ;

INLINE_TABLE_HEADER_SEP : '---' '-'* ;

inline_table_header : IDENTIFIER ('|' IDENTIFIER)* ;

inline_table_header_separator : INLINE_TABLE_HEADER_SEP ('|' INLINE_TABLE_HEADER_SEP)* ;

inline_table_data_row : expression ('|' expression)* ;

```

In words, it's marked by 3 or more underscores, then identifiers
separated by pipes (`|`), a header separator row that has sections of 3
or more hyphens separated by pipes, then data in the form of expressions
separated by pipes, finished by 3 or more underscores.

So these are technically parsed the same:

```bemp

zones =
_____________
name | origin
-----|-------
'Z1' | 0
'Z2' | 1
_____________

zones = ___ name|origin---'Z1'|0|'Z2'|1___

```

But I think it's obvious which one is easier for a *human* to parse.


