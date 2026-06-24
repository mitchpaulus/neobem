# Neobem for Visual Studio Code

Syntax highlighting and language server support for
[Neobem](https://neobem.io), a domain specific language that targets
[EnergyPlus](https://energyplus.net/) IDF files as its output. Neobem is a
superset of the IDF file syntax.

## Features

- **Syntax highlighting** for `.bemp` and `.nbem` files, covering:
  - EnergyPlus IDF object types
  - Neobem language keywords (`import`, `export`, `let`, `if`/`then`/`else`, …)
  - Strings, comments, function application, OpenStudio handles, BCL UUIDs, and
    DOE-2 constructs.
- **Language server** features powered by the Neobem executable (`nbem --lsp`):
  - Hover documentation
  - Go to definition
  - Find references
  - Field/key completion

## Requirements

The language-server features require the Neobem executable (`nbem`) to be
installed and available on your `PATH`. You can download a release from the
[Neobem repository](https://github.com/mitchpaulus/neobem) or build it from
source.

If `nbem` is not on your `PATH`, set its location explicitly:

```jsonc
{
  "neobem.server.path": "/absolute/path/to/nbem"
}
```

Syntax highlighting works without the executable.

## Extension settings

| Setting | Default | Description |
| --- | --- | --- |
| `neobem.server.enabled` | `true` | Enable the language server. Set to `false` for syntax highlighting only. |
| `neobem.server.path` | `nbem` | Path to the Neobem executable used to run the language server. |
| `neobem.trace.server` | `off` | Trace communication with the language server (`off`, `messages`, `verbose`). |

## Commands

- **Neobem: Restart Language Server** — restarts the `nbem --lsp` process.

## Development

This extension lives in the `code/` directory of the Neobem repository.

```sh
cd code
npm install
npm run compile      # build the TypeScript client
npm run generate-grammar   # regenerate the TextMate grammar from the Vim syntax file
```

Press <kbd>F5</kbd> in VS Code to launch an Extension Development Host.

The TextMate grammar in `syntaxes/neobem.tmLanguage.json` is generated from the
Vim syntax file at `../../neobem-vim/syntax/neobem.vim` (the source of truth for
the keyword lists) via `scripts/generate-grammar.py`.

### Packaging

```sh
npm install -g @vscode/vsce
vsce package          # produces neobem-<version>.vsix
vsce publish          # publish to the Marketplace (requires a publisher token)
```

## License

GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later). See the
[LICENSE](LICENSE) file.
