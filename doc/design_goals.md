- Be a superset of the idf input file format.

  Current building energy simulation programs should be valid. This lets
  users start small with existing input files.

- Expose simple programming concepts for those unfamiliar with
  programming.

  The core of the language is simple, variable assignments, functions,
  and logic.

  And while I love statically typed languages, it doesn't make sense for
  this domain of problems. `bemp` is not meant to replace general
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

- Once initial stability is reached, backwards compatibility should be
  taken *extremely* seriously.

  Projects in building construction last *years* and the buildings
  themselves last *decades*. You should expect to be able to run your
  simulation model years later and it should work.

- There must always be a sole commander-in-chief in charge of the
  language.

  This language shall not be designed by committee. This does not
  mean feedback, feature requests, criticism are not welcome!
  Contributions of all kinds are wanted.

  This statement simply means that it is ultimately one persons decision
  on the future direction of the language.
