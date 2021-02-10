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

## Replacements

Within object definitions, arbitrary expressions can be inserted using
angle brackets '`<`' and '`>`'.

What is output *is a string representation of the expression*. As a
reminder, all expressions must evaluate to one of the 6 types listed
above (e.g. String expression, Numeric expression, List expression,
etc.).

The representation of each is as follows:

- *String Expression*: The literal text that makes up the string. This
  is the most straightforward.

- *Numeric Expression*: The value of the numeric expression, currently
  using a default formatting (This will be updated in future versions to
  use a certain number of significant figures by default).

- *List Expression*: The string representation of each element,
  *separated by a comma*. This is critical, as it allows a list to be
  used to fill in the fields for objects in which the final length is
  not known. Examples would be the fields for floor coordinates.

- *Function Expression*: Nothing, the empty string `''`. This should
  rarely be used.

- *Structure Expression*: Returns the string representation of each
  expression for each member of the structure, separated by a comma.
  Note that there is currently no way to specify the order here, unlike
  the list expression, so this is rarely something that you would want
  to do.

- *Boolean Expression*: Returns either `True` or `False`, as one would
  reasonably expect.


The most common types that are expected to be used in replacements are
the string expression, numeric expression, and list expression. The
others are there for consistency, but the usage should be rare.


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

## Functions on Lists

- `length(list)`{.bemp}: number of elements in the list
- `head(list)`{.bemp}: Returns the first element of the list
- `tail(list)`{.bemp}: Returns all elements *except* the first element in the list
- `index(list, integer)`{.bemp}: Returns the element the specified index. The
  index is 0-based, so getting the first element of the list would be
  `index(list, 0)`{.bemp}. The index can also be negative to index from the
  end. `index(list, -1)`{.bemp} returns the last element of the list and
  `index(list, -2)`{.bemp} returns the second to last element.

## Loading Data

- `load('file.txt')`{.bemp}: Load data from a file to a list of structures.

   The parameter can be a relative file path location - `..` represents
   the parent folder.

## Functional Programming Functions

Several functions are staples of functional programming languages.
`bemp` has a few of the most important ones.

- `map(function, list)`{.bemp}: Returns a new list in which the `function` has
  been applied to each element.
    - EX: `map(\x { x + 2}, [1, 2, 3])`{.bemp} will equal `[3, 4, 5]`.