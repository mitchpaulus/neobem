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

    private static int FindCharacter(string text, int zeroBasedLine, string fragment)
    {
        string line = text.Split('\n')[zeroBasedLine].TrimEnd('\r');
        return line.IndexOf(fragment, StringComparison.Ordinal);
    }
}
