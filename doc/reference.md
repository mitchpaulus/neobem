## General

Whitespace is generally ignored, expect within the object definitions and
comments.

When parsing the main file, there are 6 possibilities of what is being
parsed:

1. An idf comment (ex: `! comment`{.bemp})
2. An idf object (ex: `Version,9.4;`{.bemp})
3. A variable declaration (ex: `name = <expression>`)
4. An `import` statement (ex: `import 'filename.bemp'`{.bemp})
5. Print statement (ex: `print <expression>`{.bemp})
6. A bemp specific comment (ex: `# bemp comment`{.bemp})

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

### Strings

Strings are entered using single quotes "`'`".

### Boolean Literals

You can enter in a boolean literal in two ways each for True or False.

For true you can use `true`{.bemp} or `✓`{.bemp} (Unicode U+2713, Check Mark)

For false you can use `false`{.bemp} or `✗`{.bemp} (Unicode U+2717, Ballot X)

## Comments

There are two different types of comments in bemp. One is the comment
syntax used by the idf files, beginning with an exclamation point
character (`!`). These comments are available to have portions replaced
by expressions and they are always passed along on to the output.

The second comment type is internal to bemp itself. These are like
traditional comments in that they are parsed an then completely ignored.
They are only there to aid the reader of the code.

So for example, given:

```bemp
# This is an internal comment that won't appear in output. It won't
# have this <variable> replaced.
variable = 'Mitch'

! This is an IDF comment, with my name <variable> showing up.
```

the output is

```bemp
! This is an IDF comment, with my name Mitch showing up.
```

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

- `load(expression)`{.bemp}: Load data from a file to a list of structures.

  The single input parameter can either be a string expression or
  structure.

  If the input parameter is a string, it is a relative file path
  location. `..` represents the parent folder.

  If a structure is passed as the parameter, the members are used as
  options.


### Loading from Excel

To load data from Excel, you can use a structure with the following options.

1. `type`: This must be set to `'Excel'`{.bemp}.
2. `sheet`: Optional. A string that has the name of the sheet to pull
   the data from. If omitted, the first worksheet is used by default.
3. `range`: Optional. Can be specified in different forms.
    - `'A1:B2'`{.bemp} style. This is a string that specifies the complete range
      in normal Excel range syntax.
    - `'A1'`{.bemp} style. A single cell reference. Neobem will use the input as
      the upper left hand corner of the data table. It will read headers
      to the right until it reaches a blank cell. It then read down the
      table until it reaches a row in which every column in the table is
      empty.

An example:

```bemp
load_options = {
    type: 'Excel',
    sheet: 'Data',
    range: 'C10'
}

print map(my_template, load(load_options))
```


## Functional Programming Functions

Several functions are staples of functional programming languages.
`bemp` has a few of the most important ones.

- `map(function, list)`{.bemp}: Returns a new list in which the `function` has
  been applied to each element.
    - EX: `map(\x { x + 2}, [1, 2, 3])`{.bemp} will equal `[3, 4, 5]`.

- `filter(function, list)`{.bemp}: Returns a new list in which each
  element is passed to the function provided, and only the ones in which
  the function result is `true`{.bemp} are returned.

    - EX: `filter(\x { x < 3 }, [1, 2, 3, 4])`{.bemp} will equal `[1, 2]`{.bemp}.

## Let Expressions

Oftentimes, it is useful to be able to name a sub-calculation as part of
evaluating an expression. This is especially the case when within if
expressions where you can't put a variable declaration.

This is where 'let expressions' come into play. The syntax for a let
expression is:

```bemp
let var1 = expression [, var2 = expression]... in <expression>
```

Here's an example of using a let expression in defining a `filter`
function.

```bemp
filter = λ predicate list {
    if length(list) == 0 then [] else
    let elem = head(list),
        remaining = filter(predicate, tail(list))
    in
    if predicate(elem) then [elem] + remaining else remaining
}

filtered_list = filter(λ x { x < 2 }, [1, 2, 3, 4])
```

## Importing and Exporting

neobem has a fairly straightforward method of importing and exporting.
The syntax for importing from another file is:

```bemp
import 'uri' [as 'prefix'] [only (identifier1, identifier2, ...)]
```

The only portion that is required is the string `'uri'`. By default, this
string is a relative path to a file on local machine. For example,

```bemp
import 'utilities/my_utilities.bemp'
```

will import a 'my_utilities.bemp' file in the 'utilities' folder that is
in the same directory as the executing script.

Important note: Paths are *relative from the script location, not from
the current working directory of execution*. If this weren't the case,
running `neobem` from different locations would affect the outcome.

```sh
bemp in.bemp
```

would be different from:

```sh
bemp sub_folder/in.bemp
```

The string can also be a normal Internet URL. For example, you can test
an example file from GitHub.

```bemp
import 'example'
```

Important: *By default, identifiers that are imported are imported
directly into the same namespace as the calling script, with the
imported identifier overwriting any existing identifier*.

So for example:

```bemp
my_template = \ value {
Schedule:Constant,
  Const <value>, ! Name RefList: [ScheduleNames], REQ, #1
  ,              ! Schedule Type Limits Name [ScheduleTypeLimitsNames], #2
  <value>;       ! Hourly Value Def: 0, #3
}

import 'importfile.bemp'

print my_template(10)
```

where **importfile.bemp** has the contents

```
my_template = \ value {
Material:AirGap,
  <value> Air Gap, ! Name RefList: [MaterialName], REQ, #1
  <value>;         ! Thermal Resistance {m2-K/W}, REQ, #2
}
```

results in

```bemp
Material:AirGap,
  10 Air Gap, ! Name
  10;         ! Thermal Resistance {m2-K/W}
```

To avoid Conflicts, you can make use of the `as`{.bemp} and `only`{.bemp} options of
importing.

The `as`{.bemp} option uses the specified string that follows as a
prefix, with an '`@`' character between.

So if our example above instead used (note the `as 'my_import'`{.bemp}
option):

```bemp
my_template = \ value {
Schedule:Constant,
  Const <value>, ! Name RefList: [ScheduleNames], REQ, #1
  ,              ! Schedule Type Limits Name [ScheduleTypeLimitsNames], #2
  <value>;       ! Hourly Value Def: 0, #3
}

import 'importfile.bemp' as 'my_import'

print my_template(10)
```

results in

```bemp
Schedule:Constant,
  Const 10, ! Name RefList: [ScheduleNames], REQ, #1
  ,         ! Schedule Type Limits Name [ScheduleTypeLimitsNames], #2
  10;       ! Hourly Value Def: 0, #3
```

If the imported function was desired, then it would be called like:

```bemp
my_template = \ value {
Schedule:Constant,
  Const <value>, ! Name RefList: [ScheduleNames], REQ, #1
  ,              ! Schedule Type Limits Name [ScheduleTypeLimitsNames], #2
  <value>;       ! Hourly Value Def: 0, #3
}

import 'importfile.bemp' as 'my_import'

print my_import@my_template(10)
```

If only certain identifiers are desired to be imported, the `only
(identifiers, ...)`{.bemp} Syntax can be used. It can be used in
combination with the `as`{.bemp} option as well.

### Controlling What Gets Imported

When an import statement is called, the compiler moves to the imported
file and serially executes all the statements contained within.

*Any object declaration in the imported file is passed on to the final
result*. This is how you can import a normal idf file and have it pass
along to the final output.

However, variable and function definitions are not passed along to the
calling environment *unless they are exported*.

The syntax for exporting identifiers is:

```bemp
export (identifier1, identifier2, ...)
```

For example, if the following file is imported:

```bemp
my_template = \ name {
Zone,
  <name>,        ! Name RefList: [ZoneNames, OutFaceEnvNames, ZoneAndZoneListNames, AirflowNetworkNodeAndZoneNames], REQ, #1
  0,             ! Direction of Relative North {deg}, Def: 0, #2
  0,             ! X Origin {m}, Def: 0, #3
  0,             ! Y Origin {m}, Def: 0, #4
  0,             ! Z Origin {m}, Def: 0, #5
  1,             ! Type Def: 1, #6
  1,             ! Multiplier Def: 1, #7
  autocalculate, ! Ceiling Height {m}, Def: autocalculate, AC, #8
  autocalculate, ! Volume {m3}, Def: autocalculate, AC, #9
  autocalculate, ! Floor Area {m2}, Def: autocalculate, AC, #10
  ,              ! Zone Inside Convection Algorithm [Simple, TARP, CeilingDiffuser, AdaptiveConvectionAlgorithm, TrombeWall], #11
  ,              ! Zone Outside Convection Algorithm [SimpleCombined, TARP, DOE-2, MoWiTT, AdaptiveConvectionAlgorithm], #12
  Yes;           ! Part of Total Floor Area Def: Yes, [Yes, No], #13
}

const_schedule = \ value {
Schedule:Constant,
  Const <value>, ! Name RefList: [ScheduleNames], REQ, #1
  ,              ! Schedule Type Limits Name [ScheduleTypeLimitsNames], #2
  <value>;       ! Hourly Value Def: 0, #3
}

Version,
    9.4;

export(const_schedule)
```

`Version, 9.4`{.bemp} will be printed to the output, but only
`const_schedule` will be available for use from the script that has
imported this file.

