# Reference

## General

Whitespace is generally ignored, expect within the object definitions and
comments.

When parsing the main file, there are 6 possibilities of what is being
parsed:

1. An idf comment (ex: `! comment`{.nbem})
2. An idf object (ex: `Version,9.4;`{.neobem})
3. A variable declaration (ex: `name = <expression>`)
4. An `import` statement (ex: `import 'filename.nbem'`{.nbem})
5. Print statement (ex: `print <expression>`{.nbem})
6. A Neobem specific comment (ex: `# Neobem comment`{.nbem})
7. A log statement (ex: `log myvar`{.nbem})

idf comments and idf objects result in text being output to the final
target.

The purpose of the `print` statement is to be able to evaluate an
expression without naming it^[The other main purpose of having a unique
token before the expression is easier parsing of the file. Having lone
expressions everywhere makes things tricky.].

## Data Types

There are two primitive expression types, the same as EnergyPlus, along
with the boolean True/False data type.

1. String expressions (or alpha in idf terminology)
2. Numeric expressions
3. Boolean expressions

There are 3 higher order expression types.

1. List expressions
2. Function expressions
3. Dictionary expressions

### Strings

Strings are entered using single quotes "`'`". You can escape the
following characters with a backslash:

- `'\n'`{.nbem} for newline
- `'\r'`{.nbem} for carriage return
- `'\t'`{.nbem} for tab
- `'\''`{.nbem} for single quote
- `'\\'`{.nbem} for the backslash character itself

### Boolean Literals

You can enter in a boolean literal in two ways each for True or False.

For true you can use `true`{.nbem} or `✓`{.nbem} (Unicode U+2713, Check Mark)

For false you can use `false`{.nbem} or `✗`{.nbem} (Unicode U+2717, Ballot X)

## Comments

There are two different types of comments in Neobem. One is the comment
syntax used by the idf files, beginning with an exclamation point
character (`!`). These comments are available to have portions replaced
by expressions and they are always passed along on to the output.

The second comment type is internal to Neobem itself. These are like
traditional comments in that they are parsed an then completely ignored.
They are only there to aid the reader of the code.

So for example, given:

```neobem
# This is an internal comment that won't appear in output. It won't
# have this <variable> replaced.
variable = 'Mitch'

! This is an IDF comment, with my name <variable> showing up.
```

the output is

```neobem
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

- *Dictionary Expression*: Returns the string representation of each
  expression for each member of the dictionary, separated by a comma.
  Note that there is currently no way to specify the order here, unlike
  the list expression, so this is rarely something that you would want
  to do.

- *Boolean Expression*: Returns either `True` or `False`, as one would
  reasonably expect.


The most common types that are expected to be used in replacements are
the string expression, numeric expression, and list expression. The
others are there for consistency, but the usage should be rare.


## Operators

### Plus '`+`'
- Addition for numeric types
- String concatenation for string types
    - `'Chiller ' + '1'`{.nbem} equals `'Chiller 1'`{.nbem}
    - If using `'+'` operator with a string and a numeric, the
      numeric is coerced to a string, and then the two strings are
      concatenated. So `'Chiller ' + 1`{.nbem} (notice no quotes
      around the number 1) becomes the string `'Chiller 1'`{.nbem} as you
      would expect.
- List concatenation for list types
    - `[1, 2, 3] + [4, 5]` equals `[1, 2, 3, 4, 5]`
- Dictionaries - concatenate/update
    - When two dictionaries are used with the `+` operator, the keys from
      both dictionaries are combined into a new dictionary. If both
      dictionaries have the same key, then the value from the *second*
      dictionary is used. This is how dictionaries can be "updated" or
      modified. An example is shown below.

```neobem
INCLUDE code_samples/dictionary_addition_reference.nbem
```

Compiling the above code results in:

```neobem
INCLUDE code_samples/dictionary_addition_reference.output.idf
```


### Other Algebraic Operators

The following operators are only valid for numeric types.

- Exponentiation: '`^`'
- Minus '`-`'
- Multiplication '`*`'
- Division '`/`'

### Range Operator

The range operator can be used to quickly make a list of integer values.
The syntax looks like:

```neobem
list_normal = [1, 2, 3, 4, 5]
list_with_range_operator = 1..5
```

It is a numeric expression, followed by '`..`', then by a second numeric
expression.

### Map and Filter Operator

Two important concepts in Neobem and functional programming in general
are *filtering* and *mapping*. They are important enough that a
dedicated operator in the syntax was created.

A filter takes a list and a function that returns true or false. What is
returned is a list with only the elements that return true when put
through the function. The filter operator is `'|>'`, or a pipe followed by
a greater than sign. You might remember it by thinking of the greater
than sign as constricting a list, making it smaller from left to right.
Or like a typical physical filter symbol, rotated counter-clockwise by
90°.

A map takes a list and a function and returns a new list of the same
length in which each element has been transformed by the function. The
operator  is '`|=`' or a pipe followed by an equals sign. You can think
of the horizontal lines in the equals sign being the individual elements
being transformed some way 1:1.

Examples:

```neobem
INCLUDE code_samples/map_operator.nbem
```

### Pipe Operator

The pipe operator allows you to write functional code from left to
right, like water flowing in a pipe. Many operations can be broken up
into a series of transformations that can then be composed. The pipe
operator (`'->'`) is essentially syntax sugar for the following:

```neobem
# Normal function call
var = function(parameter)

# Pipe function call
var = parameter -> function
```

When the function has more than one parameter, the *first* parameter can
be piped. For example:

```neobem
# Normal function call with multiple parameters
var = function(parameter1, parameter2)

# Using pipe
var = parameter1 -> function(parameter2)
```

Technically, what has happened is that Neobem allows something called
*partial application* of a function. You are allowed to invoke a
function specifying *1* less parameter than originally specified. This
returns a new function that is a function of a single variable, the
first parameter.

In this trivial example, not much has been gained. The real usefulness
comes when multiple functions are chained. For example:

```neobem
# Imagine fix_units, to_uppercase, and template are all functions
print obj -> fix_units -> to_uppercase -> template
# is more understandable than
print template(to_uppercase(fix_units(obj)))
# at least in my opinion
```

## Inline Data

Grammar for inline data tables in ANTLR form:

```antlr
INCLUDE text_snippets/inline_data_table_antlr.txt
```

In words, it's marked by 3 or more underscores, hyphens, or box drawing
characters, then identifiers separated by pipes (`|`), a header
separator row that has sections of 3 or more hyphens separated by pipes,
then data in the form of expressions separated by pipes, finished by 3
or more underscores, hyphens, or box drawing characters.

So these are technically parsed the same:

```neobem
zones =
_________________
'name' | 'origin'
-------|---------
'Z1'   | 0
'Z2'   | 1
_________________

zones = ___ 'name'|'origin'---'Z1'|0|'Z2'|1___
```

But I think it's obvious which one is easier for a *human* to parse.


## Mathematical Functions

A number of mathematical functions are built in.

- `abs(x)`{.nbem}: absolute value
- `acos(x)`{.nbem}: inverse cosine - return value is in radians
- `asin(x)`{.nbem}: inverse sine - return value is in radians
- `atan2(x, y)`{.nbem}: inverse tangent, returns the angle whose tangent is the quotient of two specified numbers.
- `ceiling(x)`{.nbem}: Returns the smallest integral value that is greater than or equal to the specified number.
- `cos(x)`{.nbem}: Returns the cosine of the angle specified in radians
- `floor(x)`{.nbem}: Returns the smallest integral value that is less than or equal to the specified number.
- `ln(x)`{.nbem}: Natural logarithm, logarithm with base $e$
- `log10(x)`{.nbem}: logarithm with base 10
- `log2(x)`{.nbem}: logarithm with base 2
- `mod(a, n)`{.nbem}: Returns the modulus of `a` divided by `n`. Uses *truncated* division (See [Wikipedia](https://en.wikipedia.org/wiki/Modulo_operation)) for details.
- `sin(x)`{.nbem}: Returns the sine of the angle specified in radians
- `sqrt(x)`{.nbem}: Returns the positive root of the value (always remember there are 2 roots!)
- `tan(x)`{.nbem}: Returns the tangent of the angle specified in radians

## Functions on Lists

- `length(list)`{.nbem}: number of elements in the list
- `head(list)`{.nbem}: Returns the first element of the list
- `tail(list)`{.nbem}: Returns all elements *except* the first element in the list
- `init(list)`{.nbem}: Returns all elements *except* the last element in
  the list
- `last(list)`{.nbem}: Returns the last element in the list
- `index(list, integer)`{.nbem}: Returns the element the specified index. The
  index is 0-based, so getting the first element of the list would be
  `index(list, 0)`{.nbem}. The index can also be negative to index from the
  end. `index(list, -1)`{.nbem} returns the last element of the list and
  `index(list, -2)`{.nbem} returns the second to last element.

## String Functions

- `join(list, seperator)`{.nbem}: Joins a list of strings together with a
  separator string.
    - Ex: `join(['a', 'b', 'c'], ', ')`{.nbem} results in `'a, b, c'`{.nbem}

## Functions for Dictionaries

- `keys(dictionary)`{.nbem}: Returns a list of strings that are the keys
  to the dictionary.
- `has(dictionary, key)`{.nbem}: Returns a boolean representing whether
  the given dictionary has the string key as a member.

## Loading Data

- `load(expression)`{.nbem}: Load data from a file to a list of dictionaries.

  The single input parameter can either be a string expression or
  dictionary.

  If the input parameter is a string, it is a relative file path
  location. `..` represents the parent folder.

  If a dictionary is passed as the parameter, the members are used as
  options.


### Loading Delimited Text Data

The most straightforward file type to load is the delimited text file.
To load, pass a dictionary with the following options:

1. `type`: This must be set to `'text'`{.nbem}.
2. `path`: Required. A string that is the relative path to the text file.
3. `has header`: Optional. A boolean that specifies whether the file has
   a header record or not. By default, this is `true`{.nbem}.
4. `delimiter`: Optional. A string that specifies the delimiter between
   fields. By default, this is a tab (`'\t'`) character.
5. `skip`. Optional. An integer number of lines to skip.

For example, with a text file like:

```
Non useful info to start:

Header 1|Header 2
Value1|Value2
Value3|Value4
```

It can be loaded like:

```neobem
load_options = {
  'type': 'text',
  'path': 'path/to/file.txt',
  'skip': 2,
  'delimiter': '|'
}

data = load(load_options)
# data is same as:
data = [
  { 'Header 1': 'Value1', 'Header  2': 'Value2' },
  { 'Header 1': 'Value3', 'Header  2': 'Value4' },
]

```


### Loading from Excel

To load data from Excel, you can use a dictionary with the following options.

1. `type`: This must be set to `'Excel'`{.nbem}.
2. `path`: Required. A string that is the relative path to the Excel file.
3. `sheet`: Optional. A string that has the name of the sheet to pull
   the data from. If omitted, the first worksheet is used by default.
4. `range`: Optional. Can be specified in different forms.
    - `'A1:B2'`{.nbem} style. This is a string that specifies the complete range
      in normal Excel range syntax.
    - `'A1'`{.nbem} style. A single cell reference. Neobem will use the input as
      the upper left hand corner of the data table. It will read headers
      to the right until it reaches a blank cell. It then read down the
      table until it reaches a row in which every column in the table is
      empty.

An example:

```neobem
load_options = {
    'type': 'Excel',
    'sheet': 'Data',
    'range': 'C10',
    'path': 'my_excel_data.xlsx'
}

print map(load(load_options), my_template)
```

### Loading JSON

To load JSON formatted data, you can use a dictionary with the following
options.

1. `type`: Required. This must be set to `'JSON'`{.nbem}.
2. `path`: Required. A string that is the relative path to the JSON file.

The mapping between the different value types in JSON to the built in
types for Neobem is straightforward, except for JSON's `null`. `null`
values in JSON are currently converted to a string expression with the
contents `'null'`{.nbem}.

JSON values | Neobem types
------------|-------------
object      | Dictionary
array       | List
string      | String
number      | Number
"true"      | Boolean True
"false"     | Boolean False
"null"      | String `'null'`{.nbem}

An example:

```neobem
load_options = {
    'type': 'JSON',
    'path': '../some folder/data.json'
}

print map(load(load_options), my_template)
```

### Loading XML

In a similar manner to JSON, you can load XML passing in a dictionary
with the following properties.

1. `type`: Required. This must be set to `'XML'`{.nbem}.
2. `path`: Required. A string that is the relative path to the XML file.

The mapping between XML and Neobem is only slightly lossy. An example
will show the mapping best.

```xml
<root>
  <element attribute1="attribute value" iselement="true">Some text</element>
  <numbers>
    <list>1.5</list>
    <list>2.5</list>
    <list>3.5</list>
  </numbers>
</root>
```

These statements are the same:

```neobem
root = load({ 'type': 'XML', 'path': 'path/to/example above.xml' })

root = {
  'element': {
    'attribute1': 'attribute value'
    'iselement': true
    'value': 'Some text'
  }

  'numbers': {
    'list': [
      { 'value': 1.5 },
      { 'value': 2.5 },
      { 'value': 3.5 },
    ]
  }

  'value': ''
}
```

The basics are that

1. A dictionary is returned.
2. Each child element is made as a property
3. If there are more than one of the same child element, the value for
   that property is a list of dictionaries.
4. Each element has a `'value'`{.nbem} property, which holds the text inside of
   a given element.
5. Booleans and numeric values are attempted to be parsed for all
   dictionary values, with a string type as the fall back.

Some tricky edge cases:

1. It's expected that there is a single root element, Neobem currently
   only loads the first element.
2. If you have an element such as

   ```xml
   <element>   some    <br></br>     text    </element>
   ```

   the `'value'`{.nbem} that you get out will be `'some text'`{.nbem}. Each run
   of text is trimmed of white space on both the start and end, then
   concatenated with a space.

## Functional Programming Functions

Several functions are staples of functional programming languages.
Neobem has a few of the most important ones.

- `map(list, function)`{.nbem}: Returns a new list in which the `function` has
  been applied to each element.
    - EX: `map([1, 2, 3], \x { x + 2})`{.nbem} will equal `[3, 4, 5]`{.nbem}.

- `filter(list, function)`{.nbem}: Returns a new list in which each
  element is passed to the function provided, and only the ones in which
  the function result is `true`{.nbem} are returned.

    - EX: `filter([1, 2, 3, 4], \x { x < 3 })`{.nbem} will equal `[1, 2]`{.nbem}.

## Other Functions

- `type(anything)`{.nbem}: Returns a string representing the type of the
  input parameter. Possibilities include: `'function'`{.nbem}, `'string'`{.nbem},
  `'list'`{.nbem}, `'numeric'`{.nbem}, `'dictionary'`{.nbem}, and `'boolean'`{.nbem}.

## Let Expressions

Oftentimes, it is useful to be able to name a sub-calculation as part of
evaluating an expression. This is especially the case when within if
expressions where you can't put a variable declaration.

This is where 'let expressions' come into play. The syntax for a let
expression is:

```neobem
let var1 = expression [, var2 = expression]... in <expression>
```

Here's an example of using a let expression in defining a `filter`
function.

```neobem
INCLUDE code_samples/filter_let_expression.nbem
```

## Importing and Exporting

neobem has a fairly straightforward method of importing and exporting.
The syntax for importing from another file is:

```neobem
import <URI> [as prefix_identifier] [only (identifier1, identifier2, ...)]
```

The only portion that is required is the *expression* `URI`. Generally, this
string is a relative path to a file on local machine. For example,

```neobem
import 'utilities/my_utilities.nbem'
```

will import a 'my_utilities.nbem' file in the 'utilities' folder that is
in the same directory as the executing script.

Notice that this is not necessarily a string, but any arbitrary
*expression* that evaluates to a string. For example:

```neobem
INCLUDE code_samples/reference_import_expression.nbem
```

**Important note**: Paths are *relative from the script location, not from
the current working directory of execution*. If this weren't the case,
running `neobem` from different locations would affect the outcome.

```sh
nbem in.nbem
```

would be different from:

```sh
nbem sub_folder/in.nbem
```

The URI can also be a normal Internet URL. For example, you can test
an example file from GitHub.

```neobem
import 'example'
```

Important: *By default, identifiers that are imported are imported
directly into the same namespace as the calling script, with the
imported identifier overwriting any existing identifier*.

So for example:

```neobem
my_template = \ value {
Schedule:Constant,
  Const <value>, ! Name
  ,              ! Schedule Type Limits Name
  <value>;       ! Hourly Value
}

import 'importfile.nbem'

print my_template(10)
```

where **importfile.nbem** has the contents

```neobem
INCLUDE code_samples/importfile.nbem
```

results in

```neobem
Material:AirGap,
  10 Air Gap, ! Name
  10;         ! Thermal Resistance {m2-K/W}
```

To avoid conflicts, you can make use of the `as`{.nbem} and `only`{.nbem} options of
importing.

The `as`{.nbem} option uses the specified identifier that follows as a
prefix, with an '`@`' character between.

So if our example above instead used (note the `as my_import`{.nbem}
option):

```neobem
INCLUDE code_samples/reference_as_import_1.nbem
```

results in

```neobem
Schedule:Constant,
  Const 10, ! Name
  ,         ! Schedule Type Limits Name
  10;       ! Hourly Value
```

If the imported function was desired, then it would be called like:

```neobem
INCLUDE code_samples/reference_as_import_2.nbem
```

If only certain identifiers are desired to be imported, the `only
(identifiers, ...)`{.nbem} Syntax can be used. It can be used in
combination with the `as`{.nbem} option as well.

### Controlling What Gets Imported

When an import statement is called, the compiler moves to the imported
file and serially executes all the statements contained within.

*Any object declaration in the imported file is passed on to the final
result*. This is how you can import a normal idf file and have it pass
along to the final output.

However, variable and function definitions are not passed along to the
calling environment *unless they are exported*.

The syntax for exporting identifiers is:

```neobem
export (identifier1, identifier2, ...)
```

For example, if the following file is imported:

```neobem
INCLUDE code_samples/reference_controlling_what_gets_imported.nbem
```

`Version, 9.4`{.nbem} will be printed to the output, but only
`const_schedule` will be available for use from the script that has
imported this file.

## Debugging

Currently, debugging is somewhat crude. As of now, there is no step by
step debugger. What is available is a special keyword `log`. It can be
invoked similarly to the `print` statement.

```neobem
log expression
```

What the log statement does is evaluate the entire expression, and then
prints a detailed representation of the expression to the *standard
error* output stream. This allows for the debug information to normally
be printed to the terminal, without ruining the final compiled idf file.

