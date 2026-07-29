using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using gui.Parsing;
using gui.ViewModels;

namespace gui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Hover and the "at caret" pane follow the caret, since a plain TextBox
        // gives us no way to hit-test a character under the mouse.
        SourceBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.CaretIndexProperty) ViewModel?.UpdateCaret(SourceBox.CaretIndex);
        };
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private async void OpenClicked(object? sender, RoutedEventArgs e)
    {
        IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Neobem file",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Neobem files") { Patterns = new[] { "*.nbem" } },
                FilePickerFileTypes.All,
            },
        });

        string? path = files.FirstOrDefault()?.TryGetLocalPath();
        if (path is not null) ViewModel?.LoadFile(path);
    }

    private void ReloadClicked(object? sender, RoutedEventArgs e) => ViewModel?.Reload();

    private void SymbolTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SymbolTree.SelectedItem is not SymbolNode node || ViewModel is null) return;
        ViewModel.SelectedSymbol = node;
        HighlightSpan(node.StartIndex, node.StopIndex + 1);
    }

    private void DiagnosticDoubleTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as ListBox)?.SelectedItem is not Diagnostic diagnostic || ViewModel is null) return;
        int offset = OffsetOfLine(ViewModel.SourceText, diagnostic.Line) + diagnostic.Column;
        HighlightSpan(offset, offset);
    }

    // ---- Language features -------------------------------------------------

    private void WindowKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F12 when e.KeyModifiers.HasFlag(KeyModifiers.Shift):
                FindReferences();
                break;
            case Key.F12:
                GoToDefinition();
                break;
            case Key.Space when e.KeyModifiers.HasFlag(KeyModifiers.Control):
                ShowCompletions();
                break;
            case Key.Escape:
                ViewModel?.ClearResults();
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private void DefinitionClicked(object? sender, RoutedEventArgs e) => GoToDefinition();

    private void ReferencesClicked(object? sender, RoutedEventArgs e) => FindReferences();

    private void CompletionsClicked(object? sender, RoutedEventArgs e) => ShowCompletions();

    private void CloseResultsClicked(object? sender, RoutedEventArgs e) => ViewModel?.ClearResults();

    private void GoToDefinition()
    {
        SourceSpan? span = ViewModel?.GoToDefinition(SourceBox.CaretIndex);
        if (span is not null) HighlightSpan(span.Start, span.End);
    }

    private void FindReferences() => ViewModel?.FindReferences(SourceBox.CaretIndex);

    private void ShowCompletions() => ViewModel?.ShowCompletions(SourceBox.CaretIndex);

    private void ResultSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if ((sender as ListBox)?.SelectedItem is not LocationResult result) return;
        HighlightSpan(result.Span.Start, result.Span.End);
    }

    private void HighlightSpan(int start, int end)
    {
        string text = SourceBox.Text ?? "";
        start = Math.Clamp(start, 0, text.Length);
        end = Math.Clamp(end, start, text.Length);

        // Setting the caret scrolls it into view; the selection is applied
        // after so the span stays highlighted.
        SourceBox.CaretIndex = start;
        SourceBox.SelectionStart = start;
        SourceBox.SelectionEnd = end;
    }

    // Returns the 0-based char offset of the start of a 1-based line number.
    private static int OffsetOfLine(string text, int line)
    {
        int offset = 0;
        for (int current = 1; current < line; current++)
        {
            int next = text.IndexOf('\n', offset);
            if (next < 0) return offset;
            offset = next + 1;
        }
        return offset;
    }
}
