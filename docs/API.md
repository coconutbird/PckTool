# API Reference

## Core Interfaces

### IPckFile

Main interface for PCK file operations.

```csharp
public interface IPckFile : IDisposable
{
    int SoundBankCount { get; }
    int StreamingFileCount { get; }
    
    byte[]? FindWem(uint sourceId);
    bool ContainsWem(uint sourceId);
    
    void AddSoundBank(ISoundBank soundBank);
    bool RemoveSoundBank(uint bankId);
    
    void AddStreamingFile(uint sourceId, byte[] data);
    bool RemoveStreamingFile(uint sourceId);
    
    WemReplacementResult ReplaceWem(uint sourceId, byte[] data, bool updateHircSizes = true);
    
    void Save(string path);
    void Save(Stream stream);
}
```

### IPckFileFactory

Factory for creating and loading PCK files.

```csharp
public interface IPckFileFactory
{
    IPckFile Create();
    IPckFile Load(string path);
    IPckFile Load(Stream stream);
}
```

### ISoundBank

Interface for Wwise soundbank operations.

```csharp
public interface ISoundBank
{
    uint Id { get; }
    uint Version { get; }
    uint LanguageId { get; }
    int MediaCount { get; }
    int HircItemCount { get; }
    
    bool ContainsMedia(uint sourceId);
    int ReplaceWem(uint sourceId, byte[] data, bool updateHircSizes = true);
    byte[] ToByteArray();
}
```

### ISoundBankBuilder

Fluent builder for creating soundbanks from scratch.

```csharp
public interface ISoundBankBuilder
{
    ISoundBankBuilder WithId(uint bankId);
    ISoundBankBuilder WithVersion(uint version);
    ISoundBankBuilder WithLanguage(uint languageId);
    ISoundBankBuilder AddMedia(uint sourceId, byte[] data);
    ISoundBankBuilder AddHircItem(IHircItem hircItem);
    ISoundBank Build();
    ISoundBankBuilder Reset();
}
```

## Usage Examples

### Loading and Querying a PCK File

```csharp
using PckTool.Services;

// Load PCK file
var pck = ServiceProvider.PckFileFactory.Load("sounds.pck");

Console.WriteLine($"Sound banks: {pck.SoundBankCount}");
Console.WriteLine($"Streaming files: {pck.StreamingFileCount}");

// Find a WEM file
var wemData = pck.FindWem(970927665);
if (wemData != null)
    File.WriteAllBytes("extracted.wem", wemData);

pck.Dispose();
```

### Replacing a WEM File

```csharp
using PckTool.Services;

var pck = ServiceProvider.PckFileFactory.Load("sounds.pck");

// Load replacement audio
var newWem = File.ReadAllBytes("replacement.wem");

// Replace WEM file (updates HIRC sizes automatically)
var result = pck.ReplaceWem(970927665, newWem);

Console.WriteLine($"Replaced {result.ReplacementCount} instances");
Console.WriteLine($"Streaming: {result.StreamingFilesModified}");
Console.WriteLine($"Embedded: {result.SoundBanksModified}");

// Save changes
pck.Save("sounds_modified.pck");
pck.Dispose();
```

### Creating a SoundBank from Scratch

```csharp
using PckTool.Services;

// Create a new soundbank using the builder
var soundBank = ServiceProvider.CreateSoundBankBuilder()
    .WithId(0xDEADBEEF)
    .WithVersion(0x71)  // Wwise version
    .WithLanguage(0)    // SFX (non-localized)
    .AddMedia(12345, File.ReadAllBytes("sound1.wem"))
    .AddMedia(67890, File.ReadAllBytes("sound2.wem"))
    .Build();

// Create a new PCK and add the soundbank
var pck = ServiceProvider.PckFileFactory.Create();
pck.AddSoundBank(soundBank);
pck.AddStreamingFile(11111, File.ReadAllBytes("music.wem"));

pck.Save("custom.pck");
pck.Dispose();
```

### Working with the Result Pattern

```csharp
var result = pck.ReplaceWem(sourceId, newData);

if (result.IsSuccess)
{
    Console.WriteLine($"Success! Modified {result.ReplacementCount} files");
}
else
{
    Console.WriteLine($"Failed: {result.Message}");
}
```

## CLI Commands

See `PckTool --help` for full command documentation.

```bash
# Extract all audio
PckTool dump --game-dir "C:\Games\HaloWars" --output dumps/

# Replace a WEM file
PckTool replace-wem --target 970927665 --source replacement.wem

# List sound banks
PckTool list --verbose

# Interactive browser
PckTool browse --language "English(US)"
```

