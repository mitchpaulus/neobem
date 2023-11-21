# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Simple Windows GUI for compiling.

## 0.8.0 - 2023-11-06

### Added

- Added modulo operation `mod`
- Added `fold` function
- Added literal `* *` parsing for DOE-2 files
- Added ability to pass boolean flags through CLI
- Added `exists` function for checking for boolean flags

### Changed

- Added short circuit logic evaluation for `and` and `or` operators

## 0.7.1 - 2022-12-28

### Changed

- Now targets .NET 7.

## 0.7.0 - 2022-02-20

### Added

- DOE-2 support added. Can now parse input file in the Building Description Language (BDL) format.

### Changed

- Release binaries are not trimmed, due to compiler warnings about possible behavior changes.

## 0.6.1 - 2022-02-13

### Changed

- References to 'structure' for associative arrays has been replaced with 'dictionary'.

## 0.6.0 - 2022-01-29

### Added

- Added new syntax for integrating with the NREL Building Component Library.

### Changed

- Now releasing executable as a single file.
- Numbers are now printing with 4 significant figures on output.


## 0.5.0 - 2021-09-17

### Added

- Added new dedicated operators for mapping, filtering, and piping. The
  operators are `|=`, `|>`, and `->`.

- Added new possible terminator for idf objects, `$`. This is important
  when building objects that can have arbitrary lengths, such as
  `Wall:Detailed`. There will likely be a different allowed character
  for aesthetic purposes in future version.

- Added range syntax. Can quickly make lists with integers like `0..10`.

- New log statement. Allows for simple debugging. Prints value of an
  expression to standard error.

- Documentation is now available in HTML form. See it live at neobem.io.

### Changed

- **BREAKING**: The order of the parameters for the `map` and `filter`
  function have swapped. Old programs will have to swap the order or
  move to the more concise operators `|=` and `|>`.

- **BREAKING**: The order of the parameters for the `join` function has
  swapped. Old programs will also have to swap this.

### Fixed

- Vast improvements to the formatter.
- The documentation for the 'has' function was previously incorrect on
  the order of the parameters.
- Documentation that attempted to use a logarithm now uses the actual
  logarithm function.

## 0.4.0 - 2021-07-22

### Added

- Ability to load in JSON data
- Ability to load in XML data
- A `print` statement is now allowed within a function. Previously it
  was only allowed at the root scope of a file.
- Added three new built in functions: `type`, `guid`, and `replace`.
- Added formatting option `-f` or `--fmt`, analogous to `gofmt`.
- When loading a delimited text file, there is an option to skip a
  number of header rows.

### Changed

- **BREAKING**: The grammar for imports was updated. The `as 'name'`
  token has been now forced to be an identifier, not a string. All
  strings will need to have the quotations removed, and the `name` made
  into a valid identifier.

- The URI portion of the `import` statement now is generalized to be an
  expression, instead of a string. This means that the URI for an import
  can now be dynamic. This is not a breaking change.

- Box characters can now be used for inline data tables. Generally, the
  formatter will be used to redraw the inline data tables with these
  box characters.

### Fixed

- The program now exits immediately on any errors in lexing or parsing.
  Previously would still try to walk the broken syntax tree.


## 0.3.0

### Changed

- **BREAKING**: Structures now act as true associative arrays. You can
  use any string expression as the key.

- **BREAKING**: Because the structures can now use any string as the
  key, the sanitation of column headers when loading from Excel has been
  removed.

- **BREAKING**: Inline data table headers are now strings, not
  identifiers.

### Added

- String escaping is now supported for the following characters:
    - `'\n'` for newline
    - `'\r'` for carriage return
    - `'\t'` for tab
    - `'\''` for single quote
    - `'\\'` for the backslash character itself

- New built in functions:
    - `keys`: returns all the current keys in a structure
    - `has`: check whether a structure has a given key initialized
    - `init`: return all but the last element in a list
    - `last`: return the last element in a list
    - `join`: join a list of strings together with a separator string

- Pretty print the IDF objects by default.
- Added ability to create empty structure.
- Added equality comparisons between booleans, empty lists (like `list
  == []`), and comparison of lists with different lengths.
- Trailing commas are now allowed in both lists and structures ([1, 2,
  3,] is valid).

- '+' operator is now defined for 'string + number'
- '+' operator is now defined for two structures.

### Fixed

- Comments called from within functions now have replacements made.

## 0.2.0

### Changed

- **BREAKING**: Executable is now called `nbem` instead of `bemp`.

### Added

- Vast documentation additions, including 'Getting Started' section.
- Boolean literal syntax, (i.e. `true`, `false`, `✓`, `✗`).
- Let binding syntax.
- Replacement ability in comments.
- Loading from Excel functionality.
- Grammar and functionality for exports.
- Ability to import from web location.
- Ability to read input from standard input.


### Fixed

- Printing of lists of lists now flattens output as expected.
- lambda expressions can now properly return values.
- Variable assignment with nested structures now works as expected.

### Removed

- `data` syntax. Replaced with `load` function.

## 0.1.0 - 2021-02-06

### Added

- Everything! First working prototype of the language.
