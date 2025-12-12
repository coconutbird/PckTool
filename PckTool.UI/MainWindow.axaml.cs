using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using PckTool.Core.HaloWars;
using PckTool.Core.WWise.Bnk;
using PckTool.Core.WWise.Bnk.Structs;
using PckTool.Core.WWise.Pck;

namespace PckTool.UI;

public partial class MainWindow : Window
{
    private PckFile? _currentPackage;
    private SoundTable? _soundTable;
    private bool _isUpdatingFilters;

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

    private async void OnLoadSoundTableClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Sound Table XML",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("XML Files") { Patterns = ["*.xml"] },
                new FilePickerFileType("All Files") { Patterns = ["*.*"] }
            ]
        });

        if (files.Count == 0) return;

        var file = files[0];
        var path = file.Path.LocalPath;

        try
        {
            _soundTable = new SoundTable();
            if (_soundTable.Load(path))
            {
                StatusText.Text = $"Sound table loaded: {System.IO.Path.GetFileName(path)}";

                // Re-resolve file IDs if we have a package loaded
                if (_currentPackage is not null)
                {
                    ResolveSoundTableFileIds();
                    PopulateTree();
                }
            }
            else
            {
                _soundTable = null;
                StatusText.Text = "Failed to load sound table";
            }
        }
        catch (Exception ex)
        {
            _soundTable = null;
            StatusText.Text = $"Error loading sound table: {ex.Message}";
        }
    }

    private void ResolveSoundTableFileIds()
    {
        if (_soundTable is null || _currentPackage is null) return;

        // Build a lookup for all parsed banks
        var parsedBanks = new Dictionary<uint, SoundBank>();
        foreach (var entry in _currentPackage.SoundBanks.Entries)
        {
            var parsed = entry.Parse();
            if (parsed is not null)
            {
                parsedBanks[entry.Id] = parsed;
            }
        }

        // Resolve file IDs for each bank
        foreach (var bank in parsedBanks.Values)
        {
            _soundTable.ResolveFileIds(bank, id => parsedBanks.GetValueOrDefault(id));
        }
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
                FilterPanel.IsVisible = false;
                return;
            }

            FilePathText.Text = path;
            StatusText.Text = $"Loaded: {System.IO.Path.GetFileName(path)}";

            PopulateLanguageFilter();
            FilterPanel.IsVisible = true;
            PopulateTree();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
            FilterPanel.IsVisible = false;
        }
    }

    private void PopulateLanguageFilter()
    {
        if (_currentPackage is null) return;

        _isUpdatingFilters = true;
        LanguageFilter.Items.Clear();
        LanguageFilter.Items.Add(new ComboBoxItem { Content = "All Languages", Tag = (uint?)null });

        foreach (var (id, name) in _currentPackage.Languages.OrderBy(x => x.Value))
        {
            LanguageFilter.Items.Add(new ComboBoxItem { Content = name, Tag = (uint?)id });
        }

        LanguageFilter.SelectedIndex = 0;
        IdFilter.Text = string.Empty;
        NameFilter.Text = string.Empty;
        _isUpdatingFilters = false;
    }

    private void OnLanguageFilterChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingFilters) return;
        PopulateTree();
    }

    private void OnSearchFilterChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFilters) return;
        PopulateTree();
    }

    private void OnClearFiltersClick(object? sender, RoutedEventArgs e)
    {
        _isUpdatingFilters = true;
        LanguageFilter.SelectedIndex = 0;
        IdFilter.Text = string.Empty;
        NameFilter.Text = string.Empty;
        _isUpdatingFilters = false;
        PopulateTree();
    }

    private uint? GetSelectedLanguageId()
    {
        if (LanguageFilter.SelectedItem is ComboBoxItem item)
            return item.Tag as uint?;
        return null;
    }

    private string GetIdFilter()
    {
        return IdFilter.Text?.Trim() ?? string.Empty;
    }

    private string GetNameFilter()
    {
        return NameFilter.Text?.Trim() ?? string.Empty;
    }

    private void PopulateTree()
    {
        if (_currentPackage is null) return;

        PackageTree.Items.Clear();

        var selectedLanguageId = GetSelectedLanguageId();
        var idFilter = GetIdFilter();
        var nameFilter = GetNameFilter();
        var hasFilters = selectedLanguageId.HasValue || !string.IsNullOrEmpty(idFilter) || !string.IsNullOrEmpty(nameFilter);

        // Languages node (only show when no filters)
        if (!hasFilters)
        {
            var languagesNode = new TreeViewItem { Header = $"Languages ({_currentPackage.Languages.Count})" };
            foreach (var (id, name) in _currentPackage.Languages.OrderBy(x => x.Value))
            {
                languagesNode.Items.Add(new TreeViewItem { Header = $"{name} (ID: {id})" });
            }
            PackageTree.Items.Add(languagesNode);
        }

        // SoundBanks node - grouped by language, with filtering
        var filteredBanks = _currentPackage.SoundBanks.Entries.AsEnumerable();

        if (selectedLanguageId.HasValue)
            filteredBanks = filteredBanks.Where(e => e.LanguageId == selectedLanguageId.Value);

        if (!string.IsNullOrEmpty(idFilter))
            filteredBanks = filteredBanks.Where(e => e.Id.ToString("X8").Contains(idFilter, StringComparison.OrdinalIgnoreCase));

        var filteredBanksList = filteredBanks.ToList();

        // If name filter is active, we need to filter banks that contain matching sounds
        var banksWithMatchingSounds = new Dictionary<SoundBankEntry, List<(SoundItem sound, string? name)>>();
        if (!string.IsNullOrEmpty(nameFilter))
        {
            foreach (var entry in filteredBanksList)
            {
                var parsed = entry.Parse();
                if (parsed is null) continue;

                var matchingSounds = new List<(SoundItem sound, string? name)>();
                foreach (var sound in parsed.Sounds)
                {
                    var sourceId = sound.Values.BankSourceData.MediaInformation.SourceId;
                    var soundName = _soundTable?.GetCueNameByFileId(sourceId);

                    // Check if name matches filter
                    if (soundName?.Contains(nameFilter, StringComparison.OrdinalIgnoreCase) == true ||
                        sourceId.ToString("X8").Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingSounds.Add((sound, soundName));
                    }
                }

                if (matchingSounds.Count > 0)
                {
                    banksWithMatchingSounds[entry] = matchingSounds;
                }
            }
            filteredBanksList = banksWithMatchingSounds.Keys.ToList();
        }

        var soundBanksNode = new TreeViewItem { Header = $"SoundBanks ({filteredBanksList.Count})" };

        var banksByLanguage = filteredBanksList
            .GroupBy(e => e.LanguageId)
            .OrderBy(g => g.Key);

        foreach (var group in banksByLanguage)
        {
            var langName = group.First().Language ?? $"Unknown ({group.Key})";
            var langNode = new TreeViewItem { Header = $"{langName} ({group.Count()})" };

            foreach (var entry in group.OrderBy(e => e.Id))
            {
                var size = FormatSize(entry.Size);
                var bankNode = new TreeViewItem
                {
                    Header = $"{entry.Id:X8} ({size})",
                    Tag = entry
                };

                // Add sounds as children
                if (!string.IsNullOrEmpty(nameFilter) && banksWithMatchingSounds.TryGetValue(entry, out var matchingSounds))
                {
                    // Show only matching sounds when filtering by name
                    foreach (var (sound, soundName) in matchingSounds.OrderBy(s => s.name ?? s.sound.Id.ToString("X8")))
                    {
                        var sourceId = sound.Values.BankSourceData.MediaInformation.SourceId;
                        var displayName = soundName ?? $"{sourceId:X8}";
                        bankNode.Items.Add(new TreeViewItem
                        {
                            Header = $"🔊 {displayName} ({sourceId:X8})",
                            Tag = sound
                        });
                    }
                    bankNode.IsExpanded = true;
                }
                else
                {
                    // Eagerly load sounds for expansion
                    AddSoundsToBank(bankNode, entry);
                }

                langNode.Items.Add(bankNode);
            }

            soundBanksNode.Items.Add(langNode);
        }
        PackageTree.Items.Add(soundBanksNode);

        // StreamingFiles node (with ID filter only, no language filter)
        var filteredStreaming = _currentPackage.StreamingFiles.Entries.AsEnumerable();
        if (!string.IsNullOrEmpty(idFilter))
            filteredStreaming = filteredStreaming.Where(e => e.Id.ToString("X8").Contains(idFilter, StringComparison.OrdinalIgnoreCase));

        var filteredStreamingList = filteredStreaming.ToList();
        if (filteredStreamingList.Count > 0)
        {
            var stmNode = new TreeViewItem { Header = $"Streaming Files ({filteredStreamingList.Count})" };
            foreach (var entry in filteredStreamingList.Take(100))
            {
                var size = FormatSize(entry.Size);
                stmNode.Items.Add(new TreeViewItem
                {
                    Header = $"{entry.Id:X8} ({size})",
                    Tag = entry
                });
            }
            if (filteredStreamingList.Count > 100)
            {
                stmNode.Items.Add(new TreeViewItem { Header = $"... and {filteredStreamingList.Count - 100} more" });
            }
            PackageTree.Items.Add(stmNode);
        }

        // External files node (with ID filter only)
        var filteredExternal = _currentPackage.ExternalFiles.Entries.AsEnumerable();
        if (!string.IsNullOrEmpty(idFilter))
            filteredExternal = filteredExternal.Where(e => e.Id.ToString("X16").Contains(idFilter, StringComparison.OrdinalIgnoreCase));

        var filteredExternalList = filteredExternal.ToList();
        if (filteredExternalList.Count > 0)
        {
            var extNode = new TreeViewItem { Header = $"External Files ({filteredExternalList.Count})" };
            foreach (var entry in filteredExternalList.Take(100))
            {
                var size = FormatSize(entry.Size);
                extNode.Items.Add(new TreeViewItem
                {
                    Header = $"{entry.Id:X16} ({size})",
                    Tag = entry
                });
            }
            if (filteredExternalList.Count > 100)
            {
                extNode.Items.Add(new TreeViewItem { Header = $"... and {filteredExternalList.Count - 100} more" });
            }
            PackageTree.Items.Add(extNode);
        }

        // Update status with filter info
        if (hasFilters)
        {
            var filterInfo = new List<string>();
            if (selectedLanguageId.HasValue && LanguageFilter.SelectedItem is ComboBoxItem item)
                filterInfo.Add($"Language: {item.Content}");
            if (!string.IsNullOrEmpty(idFilter))
                filterInfo.Add($"ID: \"{idFilter}\"");
            if (!string.IsNullOrEmpty(nameFilter))
                filterInfo.Add($"Name: \"{nameFilter}\"");
            StatusText.Text = $"Filtered: {string.Join(", ", filterInfo)} - {filteredBanksList.Count} banks";
        }
        else
        {
            StatusText.Text = $"Loaded: {_currentPackage.SoundBanks.Count} banks, {_currentPackage.StreamingFiles.Count} streaming files";
        }
    }

    private void AddSoundsToBank(TreeViewItem bankNode, SoundBankEntry entry)
    {
        var parsed = entry.Parse();
        if (parsed is null)
        {
            bankNode.Items.Add(new TreeViewItem { Header = "Failed to parse soundbank" });
            return;
        }

        var sounds = parsed.Sounds.ToList();
        if (sounds.Count == 0)
        {
            // No sounds to show - bank might only have events/actions
            return;
        }

        foreach (var sound in sounds.OrderBy(s => s.Id))
        {
            var sourceId = sound.Values.BankSourceData.MediaInformation.SourceId;
            var soundName = _soundTable?.GetCueNameByFileId(sourceId);
            var displayName = soundName ?? $"{sourceId:X8}";

            bankNode.Items.Add(new TreeViewItem
            {
                Header = $"🔊 {displayName} ({sourceId:X8})",
                Tag = sound
            });
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