# Automatic Formatting

If you have read through the tutorial section, you will have seen some
unique characters allowed as part of the syntax. For example, you can
use the `'λ'` character in function definitions, or `'✓'` for a literal
true value.

But how do you easily enter this characters as you are initially
programming? Plugins for your current favorite editor may arrive in
the future. But there is a better solution.

The solution favored by Neobem is to use the Neobem transpiler itself
to *automatically* format the files, including using the more
aesthetically pleasing Unicode characters.

This is especially useful for formatting inline data tables. You can
quickly define a table with sloppy syntax like (along with a function
using the easy to type characters):

```neobem
INCLUDE code_samples/sloppy_table.nbem
```

and then run the command `nbem -f sloppy_table_file.nbem` on your file,
which will print out your freshly formatted file to standard output,
which will look like:

```neobem
INCLUDE code_samples/sloppy_table_beautified.nbem
```

If you like the output, you can overwrite your current file using a
syntax like (if using a POSIX-like shell like Bash):

```sh
nbem -f my_file.nbem > tmp && mv tmp my_file.nbem
```

This formatter will also take care of many other concerns, such as
whitespace between tokens, trimming extra lines at the beginning and end
of the file.

It is meant to be *opinionated*. There are many advantages to having a
de-facto standard for file formatting.

1. All programmers get used to seeing the same style no matter what
   person or group wrote code.
2. No reason to worry about style arguments when working with a team.
3. No useless version control commits that only affect style, not
   content.
4. Can prototype fast and sloppy, and not spend valuable mental effort
   on worrying about irrelevant formatting details.

It makes most sense for the formatter to be part of the Neobem
transpiler project since it can then use the exact same parsed AST
(Abstract Syntax Tree) as the transpiler, leading to high quality
output. As the currently sole author, it also makes sense that I define
what I believe the standard should be for file formatting.

You can see more arguments for this approach from its inspiration: The
[Go programming language](https://golang.org/) and `gofmt` ([Blog on
`gofmt`](https://blog.golang.org/gofmt)), and
[Prettier](https://prettier.io/), an opinionated JavaScript/TypeScript
formatter.

