using System;
using System.Linq;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using NUnit.Framework;
using src;
using LspRange = Microsoft.VisualStudio.LanguageServer.Protocol.Range;

namespace test;

[TestFixture]
public class LspTests
{
    [SetUp]
    public void SetUp()
    {
        LspReferableIdfObjectTypes.Values.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        LspReferableIdfObjectTypes.Values.Clear();
    }

    [Test]
    public void DefinitionLookupUsesSecondFieldOfAllowlistedObjects()
    {
        LspReferableIdfObjectTypes.Values.Add("Schedule:Compact");

        var document = new LanguageServer.DocumentState(CreateDefinitionSample(), FileType.Idf);
        var definitions = document.FindDefinitions(
            new Uri("file:///definition-sample.nbem"),
            6,
            FindCharacter(document.Text, 6, "Occupied"));

        Assert.AreEqual(1, definitions.Count);
        Assert.AreEqual(1, definitions[0].Range.Start.Line);
        Assert.AreEqual(2, definitions[0].Range.Start.Character);
    }

    [Test]
    public void DefinitionLookupRefreshesAfterIncrementalRename()
    {
        LspReferableIdfObjectTypes.Values.Add("Schedule:Compact");

        var document = new LanguageServer.DocumentState(CreateDefinitionSample(), FileType.Idf);

        document.ApplyContentChanges(new[]
        {
            new TextDocumentContentChangeEvent
            {
                Range = new LspRange
                {
                    Start = new Position { Line = 1, Character = 2 },
                    End = new Position { Line = 1, Character = 17 }
                },
                Text = "Office Revised"
            }
        });

        var staleDefinitions = document.FindDefinitions(
            new Uri("file:///definition-sample.nbem"),
            6,
            FindCharacter(document.Text, 6, "Occupied"));
        Assert.AreEqual(0, staleDefinitions.Count);

        document.ApplyContentChanges(new[]
        {
            new TextDocumentContentChangeEvent
            {
                Range = new LspRange
                {
                    Start = new Position { Line = 6, Character = 2 },
                    End = new Position { Line = 6, Character = 17 }
                },
                Text = "Office Revised"
            }
        });

        var updatedDefinitions = document.FindDefinitions(
            new Uri("file:///definition-sample.nbem"),
            6,
            FindCharacter(document.Text, 6, "Revised"));

        Assert.AreEqual(1, updatedDefinitions.Count);
        Assert.AreEqual(1, updatedDefinitions[0].Range.Start.Line);
        Assert.AreEqual(2, updatedDefinitions[0].Range.Start.Character);
    }

    [Test]
    public void VariableDefinitionLookupWorksInDeclarationPrintAndLogExpressions()
    {
        var document = new LanguageServer.DocumentState(CreateVariableDefinitionSample(), FileType.Idf);
        Uri uri = new("file:///variable-definition-sample.nbem");

        var declarationDefinitions = document.FindDefinitions(
            uri,
            1,
            FindCharacter(document.Text, 1, "baseLoad"));
        Assert.AreEqual(1, declarationDefinitions.Count);
        Assert.AreEqual(0, declarationDefinitions[0].Range.Start.Line);
        Assert.AreEqual(0, declarationDefinitions[0].Range.Start.Character);

        var printDefinitions = document.FindDefinitions(
            uri,
            2,
            FindCharacter(document.Text, 2, "adjustedLoad"));
        Assert.AreEqual(1, printDefinitions.Count);
        Assert.AreEqual(1, printDefinitions[0].Range.Start.Line);
        Assert.AreEqual(0, printDefinitions[0].Range.Start.Character);

        var logDefinitions = document.FindDefinitions(
            uri,
            3,
            FindCharacter(document.Text, 3, "baseLoad"));
        Assert.AreEqual(1, logDefinitions.Count);
        Assert.AreEqual(0, logDefinitions[0].Range.Start.Line);
        Assert.AreEqual(0, logDefinitions[0].Range.Start.Character);
    }

    [Test]
    public void VariableDefinitionLookupRefreshesAfterIncrementalRename()
    {
        var document = new LanguageServer.DocumentState(CreateVariableDefinitionSample(), FileType.Idf);
        Uri uri = new("file:///variable-definition-sample.nbem");

        document.ApplyContentChanges(new[]
        {
            new TextDocumentContentChangeEvent
            {
                Range = new LspRange
                {
                    Start = new Position { Line = 0, Character = 0 },
                    End = new Position { Line = 0, Character = 8 }
                },
                Text = "peakLoad"
            }
        });

        var staleDefinitions = document.FindDefinitions(
            uri,
            1,
            FindCharacter(document.Text, 1, "baseLoad"));
        Assert.AreEqual(0, staleDefinitions.Count);

        document.ApplyContentChanges(new[]
        {
            new TextDocumentContentChangeEvent
            {
                Range = new LspRange
                {
                    Start = new Position { Line = 1, Character = 15 },
                    End = new Position { Line = 1, Character = 23 }
                },
                Text = "peakLoad"
            }
        });

        var updatedDefinitions = document.FindDefinitions(
            uri,
            1,
            FindCharacter(document.Text, 1, "peakLoad"));
        Assert.AreEqual(1, updatedDefinitions.Count);
        Assert.AreEqual(0, updatedDefinitions[0].Range.Start.Line);
        Assert.AreEqual(0, updatedDefinitions[0].Range.Start.Character);
    }

    [Test]
    public void VariableDefinitionLookupIgnoresForwardReferences()
    {
        var document = new LanguageServer.DocumentState(string.Join("\n", new[]
        {
            "adjustedLoad = baseLoad + 2",
            "baseLoad = 4"
        }) + "\n", FileType.Idf);

        var definitions = document.FindDefinitions(
            new Uri("file:///forward-reference-sample.nbem"),
            0,
            FindCharacter(document.Text, 0, "baseLoad"));

        Assert.AreEqual(0, definitions.Count);
    }

    private static string CreateDefinitionSample() => string.Join("\n", new[]
    {
        "Schedule:Compact,",
        "  Office Occupied,",
        "  Through: 12/31;",
        "",
        "Lights,",
        "  My Lights,",
        "  Office Occupied;"
    }) + "\n";

    private static string CreateVariableDefinitionSample() => string.Join("\n", new[]
    {
        "baseLoad = 10",
        "adjustedLoad = baseLoad + 2",
        "print adjustedLoad",
        "log baseLoad"
    }) + "\n";

    private static int FindCharacter(string text, int zeroBasedLine, string fragment)
    {
        string line = text.Split('\n')[zeroBasedLine].TrimEnd('\r');
        return line.IndexOf(fragment, StringComparison.Ordinal);
    }
}
