> *If I have seen further it is by standing on the shoulders of Giants.*

A project such as this does not occur in a vacuum. I am indebted to many
tools and resources that have made this possible.

First and foremost, the most thanks goes to
[ANTLR](https://www.antlr.org/). This project would not exist without
it. It makes designing grammars so straightforward that a mechanical
engineer such as myself can do it. Anyone designing programming
languages should give ANTLR a try.

While my personal vision for the language should be evident, I've stolen
many good ideas from languages before me. Some inspirations include:

- The general purpose is analogous to [Sass](https://sass-lang.com/) for
  compiling to [CSS](https://www.w3.org/TR/CSS/).
- The functional programming style comes from my experience with
  Haskell. The lambda grammar is inspired in part from [Haskell](https://www.haskell.org/).
- The logic operators `and`{.nbem} and `or`{.nbem} come from
  [Python](https://www.python.org/). I think that they are clearer than
  the usual `&&` and `||` and only require one extra character. I also
  prefer the snake case convention for identifiers.
- The structure grammar matches closely to that of [JSON](https://www.json.org/json-en.html).

