# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Simple Windows GUI for compiling.

## 0.2.0

### Changed

- Executable is now called `nbem` instead of `bemp`.

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

### Removed

- `data` syntax. Replaced with `load` function.

## 0.1.0 - 2021-02-06

### Added

- Everything! First working prototype of the language.
