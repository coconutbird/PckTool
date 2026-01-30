# PckTool

A .NET library and CLI tool for manipulating Wwise PCK audio packages, specifically designed for **Halo Wars: Definitive Edition** audio modding.

## Features

- **Extract** sound banks and WEM audio files from PCK packages
- **Replace** WEM audio files with custom audio
- **Browse** audio content interactively by language and sound bank
- **Search** for WEM IDs or cue names across all sound banks
- **List** and inspect package contents and sound bank information
- **Batch Projects** for reproducible multi-file audio replacement workflows

## Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Halo Wars: Definitive Edition (for audio extraction/modding)

## Installation

```bash
# Clone the repository
git clone https://github.com/coconutbird/SoundsUnpack.git
cd SoundsUnpack

# Build the project
dotnet build

# Run the CLI
dotnet run --project PckTool -- --help
```

## CLI Commands

| Command       | Description                                         |
| ------------- | --------------------------------------------------- |
| `dump`        | Extract all sound banks and WEM files from the game |
| `replace`     | Replace one or more sound banks with .bnk files     |
| `replace-wem` | Replace one or more WEM audio files                 |
| `list`        | List all sound banks in the package                 |
| `browse`      | Interactive browser for audio content               |
| `sounds`      | List all sounds in a specific bank                  |
| `find`        | Search for WEM IDs or cue names across all banks    |
| `info`        | Show configuration and path information             |

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

## Usage Examples

```bash
# Extract all audio to a directory
PckTool dump --game hwde --output dumps/

# Extract only English audio
PckTool dump --game hwde --language "English(US)" --output dumps/english/

# Extract a specific sound bank (hex ID)
PckTool dump --game hwde --bank 0x1A2B3C4D --output dumps/

# Replace a WEM file by ID
PckTool replace-wem --game hwde --target 970927665 --source replacement.wem --output ./

# Replace using hex ID
PckTool replace-wem --game hwde --target 0x39E3B0F1 --source custom.wem --output ./

# Replace multiple WEMs in one command
PckTool replace-wem --game hwde --target 0x39E3B0F1 --source sound1.wem --target 0x12345678 --source sound2.wem --output ./

# Replace multiple sound banks in one command
PckTool replace --game hwde --target 0x1A2B3C4D --source bank1.bnk --target 0x5E6F7A8B --source bank2.bnk --output ./

# Or use --game-dir to override the auto-detected game path
PckTool replace-wem --game hwde --game-dir "C:\Games\HaloWars" --target 0x39E3B0F1 --source custom.wem --output ./

# List all sound banks with details
PckTool list --game hwde --verbose

# Browse audio interactively
PckTool browse --game hwde

# Find which bank contains a WEM ID
PckTool find --game hwde --wem 970927665

# Search for sounds by cue name
PckTool find --game hwde --name "explosion"
```

### Batch Project Examples

```bash
# Create a new batch project
PckTool batch create mymod.json --name "My Audio Mod" --game hwde

# Add a WEM replacement action
PckTool batch add-action mymod.json --action replace --target-type wem --id 0x39E3B0F1 --source custom.wem

# Validate the project
PckTool batch validate mymod.json

# Execute the batch project
PckTool batch run mymod.json

# Dry run to see what would be modified
PckTool batch run mymod.json --dry-run
```

## Project Structure

```
SoundsUnpack/
├── PckTool/                    # CLI application (Spectre.Console)
├── PckTool.Abstractions/       # Interfaces (zero dependencies)
├── PckTool.Core/               # Core library
│   ├── WWise/
│   │   ├── Pck/                # PCK package file parsing
│   │   ├── Bnk/                # BNK sound bank parsing
│   │   │   ├── Chunks/         # BKHD, HIRC, DATA, DIDX chunks
│   │   │   └── Structs/        # HIRC items, actions, etc.
│   │   └── Util/               # Hash utilities (FNV1A)
│   └── Games/HaloWars/         # Game-specific integration
├── PckTool.UI/                 # UI components (optional)
├── tests/                      # Unit tests
└── docs/                       # Additional documentation
```

## Library Usage

Reference `PckTool.Core` to use the library in your own projects:

```csharp
using PckTool.Core.WWise.Pck;
using PckTool.Core.WWise.Bnk;

// Load a PCK file
var pck = PckFile.Load("Sounds.pck");

// Iterate sound banks
foreach (var entry in pck.SoundBanks)
{
    var soundbank = entry.Parse();
    Console.WriteLine($"Bank: {soundbank.Id:X8}, Media: {soundbank.Media.Count}");
}

// Replace a WEM file
var result = pck.ReplaceWem(targetWemId, newWemData);
if (result.WasReplaced)
{
    pck.Save("Sounds_modified.pck");
}
```

## Documentation

- [API Reference](docs/API.md) - Detailed library API documentation
- [Architecture](docs/ARCHITECTURE.md) - System design and internals

## Supported Formats

| Format | Extension | Description             |
| ------ | --------- | ----------------------- |
| PCK    | `.pck`    | Wwise package container |
| BNK    | `.bnk`    | Wwise sound bank        |
| WEM    | `.wem`    | Wwise encoded audio     |

## License

This project is for educational and modding purposes. Halo Wars is a trademark of Microsoft Corporation.

## Acknowledgments

- Wwise audio middleware by Audiokinetic
- [vgmstream](https://github.com/vgmstream/vgmstream) for audio playback tools
