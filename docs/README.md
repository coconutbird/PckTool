# PckTool - Wwise PCK Audio Toolkit

A .NET library and CLI tool for manipulating Wwise PCK audio packages, specifically designed for Halo Wars: Definitive Edition audio modding.

## Features

- **Extract** sound banks and WEM audio files from PCK packages
- **Replace** WEM audio files with custom audio
- **Create** new PCK packages and sound banks from scratch
- **Query** package contents and sound bank information
- **Browse** audio interactively by language and bank
- **Search** for WEM IDs or cue names across all sound banks
- **Batch Projects** for reproducible multi-file audio replacement workflows

## Quick Start

### Installation

```bash
# Clone the repository
git clone https://github.com/coconutbird/SoundsUnpack.git
cd SoundsUnpack

# Build
dotnet build
```

### Basic Usage

```bash
# Extract all audio from the game
PckTool dump --output dumps/

# Replace a specific WEM file
PckTool replace-wem --target 970927665 --source replacement.wem

# List all sound banks
PckTool list
```

## Project Structure

```
SoundsUnpack/
├── src/PckTool.Abstractions/   # Interfaces (zero dependencies)
├── PckTool.Core/               # Core library
├── PckTool/                    # CLI application
└── tests/PckTool.Core.Tests/   # Unit tests
```

## Library Usage

### As a NuGet Package

Reference `PckTool.Core` in your project:

```csharp
using PckTool.Core.WWise.Pck;
using PckTool.Core.WWise.Bnk;

// Load a PCK file
var pck = PckFile.Load("sounds.pck");

// Replace a WEM file
var result = pck.ReplaceWem(sourceId, newWemData);
if (result.WasReplaced)
{
    pck.Save("sounds_modified.pck");
}
```

### Using the Factory Pattern

```csharp
using PckTool.Services;

// Load using factory
var pck = ServiceProvider.PckFileFactory.Load("sounds.pck");

// Create soundbank using builder
var bank = ServiceProvider.CreateSoundBankBuilder()
    .WithId(0x12345678)
    .AddMedia(sourceId, wemData)
    .Build();

pck.AddSoundBank(bank);
pck.Save("output.pck");
```

## CLI Commands

| Command       | Description                                      |
| ------------- | ------------------------------------------------ |
| `dump`        | Extract all sound banks and WEM files            |
| `replace`     | Replace a sound bank with a .bnk file            |
| `replace-wem` | Replace a specific WEM file                      |
| `list`        | List all sound banks in the package              |
| `browse`      | Interactive browser for audio content            |
| `info`        | Show configuration information                   |
| `sounds`      | List sounds in a specific bank                   |
| `find`        | Search for WEM IDs or cue names across all banks |

### Batch Commands

| Command            | Description                             |
| ------------------ | --------------------------------------- |
| `batch create`     | Create a new batch project file         |
| `batch run`        | Execute a batch project                 |
| `batch info`       | Show batch project information          |
| `batch add-action` | Add an action to a batch project        |
| `batch rm-action`  | Remove an action from a batch project   |
| `batch validate`   | Validate a batch project configuration  |
| `batch schema`     | Generate JSON schema for batch projects |

### Examples

```bash
# Extract only English audio
PckTool dump --language "English(US)" --output dumps/english/

# Replace WEM with hex ID
PckTool replace-wem --target 0x39E3B0F1 --source custom.wem

# Browse Japanese audio
PckTool browse --language Japanese

# Find which bank contains a WEM ID
PckTool find --wem 0x39E3B0F1

# Create and run a batch project
PckTool batch create mymod.json --name "My Mod" --game hwde
PckTool batch add-action mymod.json --id 0x39E3B0F1 --source custom.wem
PckTool batch run mymod.json
```

## Documentation

- [Architecture Overview](ARCHITECTURE.md) - Project structure and design patterns
- [API Reference](API.md) - Interface and usage documentation

## File Formats

### PCK Package Files

Wwise package files containing sound banks and streaming audio.

### BNK Sound Bank Files

Wwise sound bank files containing embedded audio and HIRC metadata.

### WEM Audio Files

Wwise Encoded Media - the actual audio data (Vorbis-encoded).

## Requirements

- .NET 10.0 or later
- Windows (for automatic game directory detection)

## License

MIT License

## Acknowledgments

- Audiokinetic for the Wwise audio middleware
- The Halo Wars modding community
