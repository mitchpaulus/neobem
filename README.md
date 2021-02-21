# Neobem

Neobem is a preprocessor programming language primarily targeted for the idf input files for EnergyPlus.

While creating you building energy simulation input files, have you ever wanted to:

- Use variables?
- Break out sections into custom templates?
- Use arithmetic?
- Build objects directly from Excel data?
- Loop over lists?
- Easily incorporate work from others?
- Do all this without setting up a full Python or R working environment?

If so, Neobem is what you have always wanted.

At is core, Neobem is a command line application that follows the Unix
principle of doing one thing and doing it well. That thing is compiling
an expressive programming syntax into building energy simulation input.

It is a [Unix filter
program](https://en.wikipedia.org/wiki/Filter_(software)), taking input
via standard input or a file and writing the results to standard output.
It is designed to play one role in larger processing pipelines.

I hope you find it as useful as I do.

# Getting Started

## Installation

### Quick Instructions

1. Download program files.
2. Add folder location to `PATH`.
3. Execute `nbem` in shell or command interpreter.

See below for additional details on these steps.

### Download Program

The latest release of Neobem is on GitHub, at
[https://github.com/mitchpaulus/idfplus/releases](https://github.com/mitchpaulus/idfplus/releases).
There you will see 3 main assets:

1. `neobem_win-x64_x.x.x.zip`
2. `neobem_linux-x64_x.x.x.zip`
3. `neobem_osx-x64_x.x.x.zip`

Download the zip file that matches your operating system.^[If you are
running the Windows Subsystem for Linux within Windows, I would
recommend the Linux version, and installing like a Linux program]

Extract that zip file to a location that you will want the program to
live. It doesn't really matter where you put it, but recommended places
would be:

- `C:\Program Files\neobem` on Windows
- `/usr/local/neobem` or `~/.local/neobem` on Linux

Neobem is a console or command line application. It is meant to be run
from a shell environment, that could be anything like:

- `cmd.exe` or PowerShell on Windows
- Any terminal emulator, like Gnome Terminal, Alacritty, Konsole running
  a shell like `bash`, `zsh`, or `fish`.^[If none of this makes sense, take a look at [this link](https://www.unixsheikh.com/articles/the-terminal-the-console-and-the-shell-what-are-they.html) or other web searches for 'terminal vs. shell']

### Add Program Location to PATH Variable

Once the program files are installed in your preferred location, you
will want to add the folder to the `PATH` environment variable (if the
location you put it in isn't already there).

#### Windows

On Windows, you can get to the dialog box to change the `PATH` variable
by doing a search for 'Edit System Environment Variables'.


#### Linux/OSX

Setting the `PATH` variable is most often done in the initialization of
the particular shell that you are using. The default shell on many
systems is `bash`. To add a location to the `PATH` every time bash is
invoked, you follow the steps [here](https://unix.stackexchange.com/questions/26047/how-to-correctly-add-a-path-to-path). You add the location to
the existing `PATH` variable in the `.bash_profile` or `.bashrc`
initialization file,
making sure it is exported.

If you are using a different shell, you already likely know how to add
locations to the PATH, but for example the syntax for
[fish](https://fishshell.com/) (the interactive shell I personally use),
the syntax looks like:

```fish
set -gxp PATH "/path/to/directory"
```

## Creating Neobem Input Files


The input files are simple text files - you can use any editor of choice
to create them. Here's a list of popular text editors that you might
want to try. If you've never heard of a "text editor", I'd begin with
Sublime Text or Atom.

**Cross-Platform:**

1. [Sublime Text](https://www.sublimetext.com/)
2. [Atom](https://atom.io/)
3. [Visual Studio Code](https://code.visualstudio.com/)
4. [Vim](https://www.vim.org/)
5. [Neovim](https://neovim.io/)
6. [Kate](https://kate-editor.org/)
6. [Emacs](https://www.gnu.org/software/emacs/)

**Windows:**

1. [Notepad++](https://notepad-plus-plus.org/)
2. Notepad - yes, that Notepad built into Windows

**Linux:**

1. [gEdit](https://wiki.gnome.org/Apps/Gedit)
2. [Nano](https://www.nano-editor.org/)


## Execute the Program

On Windows, the program is called `nbem.exe`. On Linux and OSX, it is
just `nbem` with no extension.

From the shell, you can test that things are working by running the
command with the help argument like:

```console
mp@mp-computer:~$ nbem -h
```

on Windows:

```console
C:\Users\mpaulus> nbem.exe -h
```

If things are working correctly, you should see help text like:

INCLUDE help_output.md

In general, you will call the execute the program `nbem`, passing in your Neobem
input file as an argument.

To compile a Neobem file to an idf file, execute a command like

```console
mp@mp-computer:~$ nbem in.nbem
```

where `in.nbem` is the relative path to the file you want to compile. In
the example above, this would be the `in.nbem` file in my home directory
('`~`'). By default, the compiled output is printed to standard output,
which you will see on the screen. To put the output into a file, either
specify the file path as a option, or redirect the output in the shell.

**Using Option:**

```console
mp@mp-computer:~$ nbem -o output.idf in.nbem
```

**Using Redirection:**

```console
mp@mp-computer:~$ nbem in.nbem > output.idf
```

Please see this [screencast link](https://asciinema.org/a/392845) that
shows an example workflow from start to finish.

![Sample screenshot from demo at: [https://asciinema.org/a/392845](https://asciinema.org/a/392845).](img/demo.png)

<!-- vim:set ft=markdown: -->
