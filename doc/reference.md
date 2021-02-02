## General

Whitespace is generally ignored, expect within the object definitions and
comments.

When parsing the main file, there are 5 possibilities of what is being
parsed:

1. An idf comment (ex: `! comment`{.bemp})
2. An idf object (ex: `Version,9.4;`{.bemp})
3. A variable declaration (ex: `name = <expression>`)
4. An `import` statement (ex: `import 'filename.bemp'`{.bemp})
5. Print statement (ex: `print <expression>`{.bemp})

idf comments and idf objects result in text being output to the final
target.

The purpose of the `print` statement is to be able to evaluate an
expression without naming it^[The other main purpose of having a unique
token before the expression is easier parsing of the file. Having lone
expressions everywhere makes things tricky.].

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
        - `'Chiller ' + '1'`{.bemp} equals `'Chiller 1'`{.bemp}
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


## Mathematical Functions

A number of mathematical functions are built in.

- `abs(x)`{.bemp}: absolute value
- `acos(x)`{.bemp}: inverse cosine - return value is in radians
- `asin(x)`{.bemp}: inverse sine - return value is in radians
- `atan2(x, y)`{.bemp}: inverse tangent, returns the angle whose tangent is the quotient of two specified numbers.
- `ceiling(x)`{.bemp}: Returns the smallest integral value that is greater than or equal to the specified number.
- `cos(x)`{.bemp}: Returns the cosine of the angle specified in radians
- `floor(x)`{.bemp}: Returns the smallest integral value that is less than or equal to the specified number.
- `ln(x)`{.bemp}: Natural logarithm, logarithm with base $e$
- `log10(x)`{.bemp}: logarithm with base 10
- `log2(x)`{.bemp}: logarithm with base 2
- `sin(x)`{.bemp}: Returns the sine of the angle specified in radians
- `sqrt(x)`{.bemp}: Returns the positive root of the value (always remember there are 2 roots!)
- `tan(x)`{.bemp}: Returns the tangent of the angle specified in radians
