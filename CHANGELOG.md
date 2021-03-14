# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Simple Windows GUI for compiling.

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
