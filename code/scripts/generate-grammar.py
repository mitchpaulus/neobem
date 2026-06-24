#!/usr/bin/env python3
"""Generate the Neobem TextMate grammar from the Vim syntax file.

The Vim syntax file in ../neobem-vim is the source of truth for the keyword
lists (IDF object types and OpenStudio object types). This script extracts them
and writes syntaxes/neobem.tmLanguage.json so the VS Code grammar stays in sync.

Run from the `code` directory:

    python3 scripts/generate-grammar.py
"""
import json
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
CODE_DIR = os.path.dirname(HERE)
VIM_SYNTAX = os.path.normpath(
    os.path.join(CODE_DIR, "..", "..", "neobem-vim", "syntax", "neobem.vim")
)
OUTPUT = os.path.join(CODE_DIR, "syntaxes", "neobem.tmLanguage.json")


def extract_keywords(text, group):
    """Pull every `syntax keyword <group> <kw>` entry, preserving file order."""
    pattern = re.compile(r"^\s*syntax keyword " + re.escape(group) + r"\s+(.+?)\s*$", re.M)
    keywords = []
    for match in pattern.finditer(text):
        # A line may list multiple keywords separated by spaces.
        keywords.extend(match.group(1).split())
    return keywords


def alternation(keywords):
    """Build a regex alternation, longest first so the longest match wins."""
    unique = sorted(set(keywords), key=lambda k: (-len(k), k))
    return "|".join(re.escape(k) for k in unique)


def main():
    with open(VIM_SYNTAX, "r", encoding="utf-8") as handle:
        text = handle.read()

    idf_keywords = extract_keywords(text, "idfKeywords")
    if not idf_keywords:
        raise SystemExit("Failed to extract keywords from the Vim syntax file")

    # Object types (EnergyPlus IDF). Keywords contain ':' so a normal word
    # boundary won't do; we guard with characters that can't be adjacent to an
    # object-type token.
    object_types = alternation(idf_keywords)

    grammar = {
        "$schema": "https://raw.githubusercontent.com/martinring/tmlanguage/master/tmlanguage.json",
        "name": "Neobem",
        "scopeName": "source.neobem",
        "patterns": [
            {"include": "#comments"},
            {"include": "#doe2-comment"},
            {"include": "#strings"},
            {"include": "#doe2-strings"},
            {"include": "#language-keywords"},
            {"include": "#conditionals"},
            {"include": "#booleans"},
            {"include": "#object-types"},
            {"include": "#osm-handle"},
            {"include": "#bcl-uuid"},
            {"include": "#numbers"},
            {"include": "#function-application"},
            {"include": "#doe2-terminator"},
        ],
        "repository": {
            "comments": {
                "name": "comment.line.number-sign.neobem",
                "match": "#.*$",
            },
            "doe2-comment": {
                "name": "comment.line.dollar.neobem",
                "match": "\\$.*$",
            },
            "strings": {
                "name": "string.quoted.single.neobem",
                "begin": "'",
                "end": "'",
            },
            "doe2-strings": {
                "name": "string.quoted.double.neobem",
                "begin": "\"",
                "end": "\"",
            },
            "language-keywords": {
                "name": "keyword.other.neobem",
                "match": "\\b(?:import|export|print|and|or|return|let|in|as|only|not|log)\\b",
            },
            "conditionals": {
                "name": "keyword.control.conditional.neobem",
                "match": "\\b(?:if|then|else)\\b",
            },
            "booleans": {
                "name": "constant.language.boolean.neobem",
                "match": "\\b(?:true|false)\\b",
            },
            "object-types": {
                "name": "support.type.object.neobem",
                "match": "(?<![\\w:])(?:" + object_types + ")(?![\\w:])",
            },
            "osm-handle": {
                "name": "variable.other.handle.neobem",
                "match": "\\{[0-9a-fA-F]{8}-(?:[0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}\\}",
            },
            "bcl-uuid": {
                "name": "constant.other.bcl-uuid.neobem",
                "match": "bcl:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
            },
            "numbers": {
                "name": "constant.numeric.neobem",
                "match": "(?<![\\w.])[-+]?(?:\\d+\\.?\\d*|\\.\\d+)(?:[eE][-+]?\\d+)?",
            },
            "function-application": {
                "match": "([a-z][A-Za-z0-9_]*)\\s*(?=\\()",
                "captures": {"1": {"name": "entity.name.function.neobem"}},
            },
            "doe2-terminator": {
                "name": "punctuation.terminator.doe2.neobem",
                "match": "\\.\\.",
            },
        },
    }

    os.makedirs(os.path.dirname(OUTPUT), exist_ok=True)
    with open(OUTPUT, "w", encoding="utf-8") as handle:
        json.dump(grammar, handle, indent=2)
        handle.write("\n")

    print(
        "Wrote {} ({} IDF object types)".format(
            os.path.relpath(OUTPUT, CODE_DIR), len(set(idf_keywords))
        )
    )


if __name__ == "__main__":
    main()
