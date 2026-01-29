# PckTool

A .NET library and CLI tool for manipulating Wwise PCK audio packages, specifically designed for **Halo Wars: Definitive Edition** audio modding.

## Features

- **Extract** sound banks and WEM audio files from PCK packages
- **Replace** WEM audio files with custom audio
- **Browse** audio content interactively by language and sound bank
- **List** and inspect package contents and sound bank information
  <!-- - **Project Management** for organizing modding workflows -->

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
| `replace`     | Replace a sound bank with a .bnk file               |
| `replace-wem` | Replace a specific WEM audio file                   |
| `list`        | List all sound banks in the package                 |
| `browse`      | Interactive browser for audio content               |
| `sounds`      | List all sounds in a specific bank                  |
| `info`        | Show configuration and path information             |

<!-- Project commands (coming soon):
| `project create` | Create a new modding project file |
| `project info`   | Show project information          |
-->

## Usage Examples

```bash
# Extract all audio to a directory
PckTool dump --output dumps/

# Extract only English audio
PckTool dump --language "English(US)" --output dumps/english/

# Extract a specific sound bank (hex ID)
PckTool dump --soundbank 1A2B3C4D --output dumps/

# Replace a WEM file by ID
PckTool replace-wem --target 970927665 --source replacement.wem

# Replace using hex ID
PckTool replace-wem --target 0x39E3B0F1 --source custom.wem

# List all sound banks with details
PckTool list --verbose

# Browse audio interactively
PckTool browse
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
foreach (var entry in pck.SoundBanks.Entries)
{
    var soundbank = SoundBank.Parse(entry.GetData());
    Console.WriteLine($"Bank: {soundbank.Id:X8}, Media: {soundbank.Media.Count}");
}

// Replace a WEM file
var result = pck.ReplaceWem(targetWemId, newWemData);
if (result.Success)
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
