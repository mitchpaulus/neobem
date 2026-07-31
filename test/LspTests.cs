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
    public void ReferenceLookupReturnsAllUsagesIncludingDefinitionWhenRequested()
    {
        LspReferableIdfObjectTypes.Values.Add("Schedule:Compact");

        var document = new LanguageServer.DocumentState(CreateDefinitionSample(), FileType.Idf);
        Uri uri = new("file:///definition-sample.nbem");

        var referencesWithDeclaration = document.FindReferences(
            uri,
            6,
            FindCharacter(document.Text, 6, "Occupied"),
            includeDeclaration: true);

        Assert.AreEqual(2, referencesWithDeclaration.Count);
        var orderedDecl = referencesWithDeclaration.OrderBy(loc => loc.Range.Start.Line).ToArray();
        Assert.AreEqual(1, orderedDecl[0].Range.Start.Line);
        Assert.AreEqual(2, orderedDecl[0].Range.Start.Character);
        Assert.AreEqual(6, orderedDecl[1].Range.Start.Line);
        Assert.AreEqual(2, orderedDecl[1].Range.Start.Character);
    }

    [Test]
    public void ReferenceLookupExcludesDeclarationWhenNotRequested()
    {
        LspReferableIdfObjectTypes.Values.Add("Schedule:Compact");

        var document = new LanguageServer.DocumentState(CreateDefinitionSample(), FileType.Idf);
        Uri uri = new("file:///definition-sample.nbem");

        var referencesWithoutDeclaration = document.FindReferences(
            uri,
            1,
            FindCharacter(document.Text, 1, "Occupied"),
            includeDeclaration: false);

        Assert.AreEqual(1, referencesWithoutDeclaration.Count);
        Assert.AreEqual(6, referencesWithoutDeclaration[0].Range.Start.Line);
        Assert.AreEqual(2, referencesWithoutDeclaration[0].Range.Start.Character);
    }

    [Test]
    public void ReferenceLookupTrimsTrailingWhitespaceInRange()
    {
        LspReferableIdfObjectTypes.Values.Add("Schedule:Compact");

        var document = new LanguageServer.DocumentState(CreateDefinitionSample(), FileType.Idf);
        Uri uri = new("file:///definition-sample.nbem");

        var references = document.FindReferences(
            uri,
            6,
            FindCharacter(document.Text, 6, "Occupied"),
            includeDeclaration: true);

        var lights = references.Single(loc => loc.Range.Start.Line == 6);
        Assert.AreEqual(6, lights.Range.End.Line);
        Assert.AreEqual(2 + "Office Occupied".Length, lights.Range.End.Character);
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
    public void CompletionLookupReturnsKnownKeysForSingleLineEmptyFieldBeforeTerminator()
    {
        var document = new LanguageServer.DocumentState("Building,None,0.0,;\n", FileType.Idf);

        var completions = document.FindCompletions(0, 18);

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

    [Test]
    public void DefinitionInsideReplacementResolvesTopLevelVariable()
    {
        var document = new LanguageServer.DocumentState(string.Join("\n", new[]
        {
            "versionNum = '9.4'",
            "Version,",
            "  <versionNum>;"
        }) + "\n", FileType.Idf);

        var definitions = document.FindDefinitions(
            new Uri("file:///replacement-sample.nbem"),
            2,
            FindCharacter(document.Text, 2, "versionNum"));

        Assert.AreEqual(1, definitions.Count);
        Assert.AreEqual(0, definitions[0].Range.Start.Line);
        Assert.AreEqual(0, definitions[0].Range.Start.Character);
    }

    [Test]
    public void DefinitionInsideReplacementResolvesLambdaParameter()
    {
        var document = new LanguageServer.DocumentState(string.Join("\n", new[]
        {
            "zone = \\ name {",
            "Zone,",
            "  <name>;",
            "}"
        }) + "\n", FileType.Idf);

        var definitions = document.FindDefinitions(
            new Uri("file:///lambda-replacement-sample.nbem"),
            2,
            FindCharacter(document.Text, 2, "name"));

        Assert.AreEqual(1, definitions.Count);
        Assert.AreEqual(0, definitions[0].Range.Start.Line);
        Assert.AreEqual(FindCharacter(document.Text, 0, "name"), definitions[0].Range.Start.Character);
        Assert.AreEqual(FindCharacter(document.Text, 0, "name") + "name".Length, definitions[0].Range.End.Character);
    }

    [Test]
    public void DefinitionInsideReplacementResolvesFunctionLocalVariable()
    {
        var document = new LanguageServer.DocumentState(string.Join("\n", new[]
        {
            "f = \\ x {",
            "  scaled = x * 2",
            "Zone,",
            "  <scaled>;",
            "}"
        }) + "\n", FileType.Idf);

        var definitions = document.FindDefinitions(
            new Uri("file:///function-local-sample.nbem"),
            3,
            FindCharacter(document.Text, 3, "scaled"));

        Assert.AreEqual(1, definitions.Count);
        Assert.AreEqual(1, definitions[0].Range.Start.Line);
        Assert.AreEqual(2, definitions[0].Range.Start.Character);
    }

    [Test]
    public void DefinitionInsideReplacementWorksAcrossFieldSeparators()
    {
        // The comma inside the replacement splits the surrounding FIELD token, so this
        // exercises the object-text reconstruction across FIELD/FIELD_SEP boundaries.
        var document = new LanguageServer.DocumentState(string.Join("\n", new[]
        {
            "first = 7",
            "second = 3",
            "Version,",
            "  <mod(first, second)>;"
        }) + "\n", FileType.Idf);

        Uri uri = new("file:///multi-field-sample.nbem");

        var firstDefinitions = document.FindDefinitions(
            uri,
            3,
            FindCharacter(document.Text, 3, "first"));
        Assert.AreEqual(1, firstDefinitions.Count);
        Assert.AreEqual(0, firstDefinitions[0].Range.Start.Line);

        var secondDefinitions = document.FindDefinitions(
            uri,
            3,
            FindCharacter(document.Text, 3, "second"));
        Assert.AreEqual(1, secondDefinitions.Count);
        Assert.AreEqual(1, secondDefinitions[0].Range.Start.Line);
    }

    [Test]
    public void DefinitionInsideLambdaBodyExpressionResolvesParameter()
    {
        var document = new LanguageServer.DocumentState("f = \\ x { return x }\n", FileType.Idf);

        int usageCharacter = document.Text.Split('\n')[0].LastIndexOf('x');
        var definitions = document.FindDefinitions(
            new Uri("file:///lambda-body-sample.nbem"),
            0,
            usageCharacter);

        Assert.AreEqual(1, definitions.Count);
        Assert.AreEqual(0, definitions[0].Range.Start.Line);
        Assert.AreEqual(document.Text.IndexOf('x', 0, usageCharacter), definitions[0].Range.Start.Character);
    }

    [Test]
    public void DefinitionInsideLetBindingBodyResolvesBoundName()
    {
        var document = new LanguageServer.DocumentState("y = let scale = 2 in scale + 1\n", FileType.Idf);

        var definitions = document.FindDefinitions(
            new Uri("file:///let-binding-sample.nbem"),
            0,
            FindCharacter(document.Text, 0, "scale + 1"));

        Assert.AreEqual(1, definitions.Count);
        Assert.AreEqual(0, definitions[0].Range.Start.Line);
        Assert.AreEqual(FindCharacter(document.Text, 0, "scale"), definitions[0].Range.Start.Character);
    }

    [Test]
    public void ReplacementIdentifierLookupSupportsBuiltInHover()
    {
        var document = new LanguageServer.DocumentState(string.Join("\n", new[]
        {
            "nums = [1, 2, 3]",
            "Version,",
            "  <join(nums, ' ')>;"
        }) + "\n", FileType.Idf);

        var identifier = document.FindReplacementIdentifierAt(2, FindCharacter(document.Text, 2, "join"));

        Assert.NotNull(identifier);
        Assert.AreEqual("join", identifier!.Name);
        Assert.IsNull(identifier.Definition);
        Assert.NotNull(LanguageServer.TryGetBuiltInHoverMarkdown(identifier.Name));

        var variableIdentifier = document.FindReplacementIdentifierAt(2, FindCharacter(document.Text, 2, "nums"));
        Assert.NotNull(variableIdentifier);
        Assert.NotNull(variableIdentifier!.Definition);
        Assert.AreEqual(0, variableIdentifier.Definition!.Range.Start.Line);
        Assert.AreEqual("nums = [1, 2, 3]", variableIdentifier.Definition.Detail);
    }

    [Test]
    public void DefinitionLookupMatchesDynamicObjectNameOutsideReplacement()
    {
        LspReferableIdfObjectTypes.Values.Add("Schedule:Compact");

        var document = new LanguageServer.DocumentState(CreateDynamicScheduleTemplateSample(), FileType.Idf);

        // Cursor on the static part of the referencing field, outside the <zone> replacement.
        var definitions = document.FindDefinitions(
            new Uri("file:///dynamic-name-sample.nbem"),
            7,
            FindCharacter(document.Text, 7, "Occupied"));

        Assert.AreEqual(1, definitions.Count);
        Assert.AreEqual(2, definitions[0].Range.Start.Line);
        Assert.AreEqual(2, definitions[0].Range.Start.Character);
    }

    [Test]
    public void DefinitionInsideReplacementOfDynamicObjectNameReturnsVariableAndObject()
    {
        LspReferableIdfObjectTypes.Values.Add("Schedule:Compact");

        var document = new LanguageServer.DocumentState(CreateDynamicScheduleTemplateSample(), FileType.Idf);

        // Cursor on 'zone' inside the <zone> replacement of the referencing field:
        // both the lambda parameter and the schedule defined with the complete
        // dynamic name should be offered.
        var definitions = document.FindDefinitions(
            new Uri("file:///dynamic-name-sample.nbem"),
            7,
            FindCharacter(document.Text, 7, "zone"));

        Assert.AreEqual(2, definitions.Count);
        Assert.AreEqual(0, definitions[0].Range.Start.Line);
        Assert.AreEqual(FindCharacter(document.Text, 0, "zone"), definitions[0].Range.Start.Character);
        Assert.AreEqual(2, definitions[1].Range.Start.Line);
        Assert.AreEqual(2, definitions[1].Range.Start.Character);
    }

    private static string CreateDynamicScheduleTemplateSample() => string.Join("\n", new[]
    {
        "template = \\ zone {",
        "Schedule:Compact,",
        "  <zone> Occupied,",
        "  Through: 12/31;",
        "",
        "Lights,",
        "  My Lights,",
        "  <zone> Occupied;",
        "}"
    }) + "\n";

    [Test]
    public void EscapedAngleBracketsAreNotTreatedAsReplacements()
    {
        var document = new LanguageServer.DocumentState(string.Join("\n", new[]
        {
            "note = 'hi'",
            "Version,",
            "  <<note>>;"
        }) + "\n", FileType.Idf);

        var identifier = document.FindReplacementIdentifierAt(2, FindCharacter(document.Text, 2, "note"));

        Assert.IsNull(identifier);
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
