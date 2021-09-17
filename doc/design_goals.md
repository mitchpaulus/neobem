# Design Goals

- Be a superset of the idf input file format.

  Current building energy simulation programs should be valid. This lets
  users start small with existing input files.

- Expose simple programming concepts for those unfamiliar with
  programming.

  The core of the language is simple, variable assignments, functions,
  and logic.

  And while I love statically typed languages, it doesn't make sense for
  this domain of problems. Neobem is not meant to replace general
  purpose programming languages. It is a language designed specifically
  for generating comma separated data for input files. So there is no
  need to add verbosity to the language in the form of types.

- Be "Batteries" included.

  The user shouldn't expect to have to download additional packages for
  the program to be useful. Important functions should be built right
  into the language.

- Cross-platform.

  All users should be able to use tool regardless of OS. The Windows,
  Mac, and Linux experience should be first class.

- Once initial stability is reached, backwards compatibility should be
  taken *extremely* seriously.

  Projects in building construction last *years* and the buildings
  themselves last *decades*. You should expect to be able to run your
  simulation model years later and it should still work.

- Installation and getting started should feel easy. The same should be
  for the uninstall process.

  There should be efforts taken to make sure new users can get up and
  running quickly. This means offering convenient methods of
  installation - This may include:

    - Different flavors of package managers for Linux
    - Windows Store
    - `scoop`, `chocolately`, and the new Windows Package Manager
      `winget`
    - Standalone `.exe` installers
    - `brew` for MacOS
    - Documentation on building from source

  It may be difficult for me to accomplish this goal by myself, may
  need much help from other contributers!

- High quality documentation.

  Building energy modeling professionals shouldn't have to also be
  software engineers. The audience here may have never programmed
  before. Thorough, hand-crafted not generated, documentation is
  required.

- Provide surrounding tooling.

  For users who haven't spent years modifying their programming
  environment, it may not be a comfortable experience editing these text
  files in Notepad. We should:
     1. Provide syntax highlighting/autocompletion files for various
        text editors.
     2. Provide GUI wrappers around the program.
     3. Provide an implementation of the [Language Server Protocol](https://microsoft.github.io/language-server-protocol/)


And to be clear, these are the goals and driving principles, it doesn't
mean all this is finished already :)


