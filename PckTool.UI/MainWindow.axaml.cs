using System;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using PckTool.Core.WWise;
using PckTool.Core.WWise.Pck;

namespace PckTool.UI;

public partial class MainWindow : Window
{
    private PckFile? _currentPackage;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnOpenPckClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select PCK File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("PCK Files") { Patterns = ["*.pck"] },
                new FilePickerFileType("All Files") { Patterns = ["*.*"] }
            ]
        });

        if (files.Count == 0) return;

        var file = files[0];
        var path = file.Path.LocalPath;

        LoadPackage(path);
    }

    private void LoadPackage(string path)
    {
        try
        {
            // Dispose previous package
            _currentPackage?.Dispose();

            _currentPackage = PckFile.Load(path);

            if (_currentPackage is null)
            {
                StatusText.Text = "Failed to load PCK file";
                return;
            }

            FilePathText.Text = path;
            StatusText.Text = $"Loaded: {System.IO.Path.GetFileName(path)}";

            PopulateTree();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
        }
    }

    private void PopulateTree()
    {
        if (_currentPackage is null) return;

        PackageTree.Items.Clear();

        // Languages node
        var languagesNode = new TreeViewItem { Header = $"Languages ({_currentPackage.Languages.Count})" };
        foreach (var (id, name) in _currentPackage.Languages.OrderBy(x => x.Value))
        {
            languagesNode.Items.Add(new TreeViewItem { Header = $"{name} (ID: {id})" });
        }
        PackageTree.Items.Add(languagesNode);

        // SoundBanks node - grouped by language
        var soundBanksNode = new TreeViewItem { Header = $"SoundBanks ({_currentPackage.SoundBanks.Count})" };
        var banksByLanguage = _currentPackage.SoundBanks.Entries
            .GroupBy(e => e.LanguageId)
            .OrderBy(g => g.Key);

        foreach (var group in banksByLanguage)
        {
            var langName = group.First().Language ?? $"Unknown ({group.Key})";
            var langNode = new TreeViewItem { Header = $"{langName} ({group.Count()})" };

            foreach (var entry in group.OrderBy(e => e.Id))
            {
                var size = FormatSize(entry.Size);
                langNode.Items.Add(new TreeViewItem
                {
                    Header = $"{entry.Id:X8} ({size})",
                    Tag = entry
                });
            }

            soundBanksNode.Items.Add(langNode);
        }
        PackageTree.Items.Add(soundBanksNode);

        // StreamingFiles node
        if (_currentPackage.StreamingFiles.Count > 0)
        {
            var stmNode = new TreeViewItem { Header = $"Streaming Files ({_currentPackage.StreamingFiles.Count})" };
            foreach (var entry in _currentPackage.StreamingFiles.Entries.Take(100)) // Limit for performance
            {
                var size = FormatSize(entry.Size);
                stmNode.Items.Add(new TreeViewItem
                {
                    Header = $"{entry.Id:X8} ({size})",
                    Tag = entry
                });
            }
            if (_currentPackage.StreamingFiles.Count > 100)
            {
                stmNode.Items.Add(new TreeViewItem { Header = $"... and {_currentPackage.StreamingFiles.Count - 100} more" });
            }
            PackageTree.Items.Add(stmNode);
        }

        // External files node
        if (_currentPackage.ExternalFiles.Count > 0)
        {
            var extNode = new TreeViewItem { Header = $"External Files ({_currentPackage.ExternalFiles.Count})" };
            foreach (var entry in _currentPackage.ExternalFiles.Entries.Take(100))
            {
                var size = FormatSize(entry.Size);
                extNode.Items.Add(new TreeViewItem
                {
                    Header = $"{entry.Id:X16} ({size})",
                    Tag = entry
                });
            }
            if (_currentPackage.ExternalFiles.Count > 100)
            {
                extNode.Items.Add(new TreeViewItem { Header = $"... and {_currentPackage.ExternalFiles.Count - 100} more" });
            }
            PackageTree.Items.Add(extNode);
        }
    }

    private static string FormatSize(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            _ => $"{bytes / (1024.0 * 1024.0):F1} MB"
        };
    }
}