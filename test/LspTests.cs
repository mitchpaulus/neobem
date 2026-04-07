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

    [Test]
    public void CompletionLookupReturnsKnownKeysForExistingFieldValue()
    {
        var document = new LanguageServer.DocumentState(CreateSimulationControlSample("Y", "No"), FileType.Idf);

        var completions = document.FindCompletions(
            1,
            FindCharacter(document.Text, 1, "Y"));

        CollectionAssert.AreEquivalent(new[] { "Yes", "No" }, completions.Select(item => item.Label).ToArray());
        Assert.AreEqual("Do Zone Sizing Calculation", completions.Single(item => item.Label == "Yes").Detail);
    }

    [Test]
    public void CompletionLookupUsesPhysicalFieldOrderForBuildingTerrain()
    {
        var document = new LanguageServer.DocumentState(CreateBuildingCompletionSample(), FileType.Idf);

        var completions = document.FindCompletions(
            3,
            FindCharacter(document.Text, 3, "FullExterior"));

        CollectionAssert.Contains(completions.Select(item => item.Label).ToArray(), "Country");
        CollectionAssert.Contains(completions.Select(item => item.Label).ToArray(), "City");
        CollectionAssert.DoesNotContain(completions.Select(item => item.Label).ToArray(), "MinimalShadowing");
    }

    [Test]
    public void CompletionLookupReturnsKnownKeysForEmptyFieldWhenCursorIsOnComma()
    {
        var document = new LanguageServer.DocumentState(CreateSimulationControlSample("No", ""), FileType.Idf);

        var completions = document.FindCompletions(2, 2);

        CollectionAssert.AreEquivalent(new[] { "Yes", "No" }, completions.Select(item => item.Label).ToArray());
    }

    [Test]
    public void CompletionLookupReturnsKnownKeysForPopulatedFieldWhenCursorIsBeforeComma()
    {
        var document = new LanguageServer.DocumentState(CreateBuildingTerrainPrefixSample(), FileType.Idf);

        var completions = document.FindCompletions(3, 5);

        CollectionAssert.Contains(completions.Select(item => item.Label).ToArray(), "Ocean");
        CollectionAssert.Contains(completions.Select(item => item.Label).ToArray(), "Country");
        CollectionAssert.DoesNotContain(completions.Select(item => item.Label).ToArray(), "MinimalShadowing");
    }

    [Test]
    public void CompletionLookupReturnsKnownKeysForEmptyFieldWhenCursorIsAfterComma()
    {
        var document = new LanguageServer.DocumentState(CreateBuildingEmptyTerrainSample(), FileType.Idf);

        var completions = document.FindCompletions(3, 3);

        CollectionAssert.Contains(completions.Select(item => item.Label).ToArray(), "Country");
        CollectionAssert.Contains(completions.Select(item => item.Label).ToArray(), "Ocean");
        CollectionAssert.DoesNotContain(completions.Select(item => item.Label).ToArray(), "MinimalShadowing");
    }

    [Test]
    public void CompletionLookupPreservesFieldIndentationAndTrailingWhitespace()
    {
        var document = new LanguageServer.DocumentState(CreateBuildingCompletionSample(), FileType.Idf);

        var oceanCompletion = document.FindCompletions(
                3,
                FindCharacter(document.Text, 3, "FullExterior"))
            .Single(item => item.Label == "Ocean");

        Assert.NotNull(oceanCompletion.TextEdit);
        Assert.AreEqual(3, oceanCompletion.TextEdit.Range.Start.Line);
        Assert.AreEqual(2, oceanCompletion.TextEdit.Range.Start.Character);
        Assert.AreEqual(3, oceanCompletion.TextEdit.Range.End.Line);
        Assert.AreEqual(14, oceanCompletion.TextEdit.Range.End.Character);
    }

    [Test]
    public void CompletionLookupRefreshesAfterIncrementalObjectTypeUpdate()
    {
        var document = new LanguageServer.DocumentState(CreateSimulationControlSample("No", "No"), FileType.Idf);

        document.ApplyContentChanges(new[]
        {
            new TextDocumentContentChangeEvent
            {
                Range = new LspRange
                {
                    Start = new Position { Line = 0, Character = 0 },
                    End = new Position { Line = 0, Character = "SimulationControl".Length }
                },
                Text = "PerformancePrecisionTradeoffs"
            },
            new TextDocumentContentChangeEvent
            {
                Range = new LspRange
                {
                    Start = new Position { Line = 2, Character = 2 },
                    End = new Position { Line = 2, Character = 4 }
                },
                Text = "C"
            }
        });

        var completions = document.FindCompletions(
            2,
            FindCharacter(document.Text, 2, "C"));

        CollectionAssert.Contains(completions.Select(item => item.Label).ToArray(), "CarrollMRT");
        CollectionAssert.Contains(completions.Select(item => item.Label).ToArray(), "ScriptF");
        CollectionAssert.DoesNotContain(completions.Select(item => item.Label).ToArray(), "Yes");
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

    private static string CreateSimulationControlSample(string firstFieldValue, string secondFieldValue) => string.Join("\n", new[]
    {
        "SimulationControl,",
        $"  {firstFieldValue},",
        $"  {secondFieldValue},",
        "  No,",
        "  No,",
        "  No,",
        "  No;"
    }) + "\n";

    private static string CreateBuildingCompletionSample() => string.Join("\n", new[]
    {
        "Building,",
        "  NONE,",
        "  0.0,",
        "  FullExterior,",
        "  .04,",
        "  .4,",
        "  FullExterior,",
        "  25,",
        "  1;"
    }) + "\n";

    private static string CreateBuildingEmptyTerrainSample() => string.Join("\n", new[]
    {
        "Building,",
        "  NONE,",
        "  0.0,",
        "  ,",
        "  .04,",
        "  .4,",
        "  FullExterior,",
        "  25,",
        "  1;"
    }) + "\n";

    private static string CreateBuildingTerrainPrefixSample() => string.Join("\n", new[]
    {
        "Building,",
        "  NONE,",
        "  0.0,",
        "  Oce,",
        "  .04,",
        "  .4,",
        "  FullExterior,",
        "  25,",
        "  1;"
    }) + "\n";

    private static int FindCharacter(string text, int zeroBasedLine, string fragment)
    {
        string line = text.Split('\n')[zeroBasedLine].TrimEnd('\r');
        return line.IndexOf(fragment, StringComparison.Ordinal);
    }
}
