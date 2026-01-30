# Architecture Overview

PckTool is structured as a layered architecture designed for maintainability, testability, and clear separation of concerns.

## Project Structure

```
SoundsUnpack/
├── src/
│   └── PckTool.Abstractions/   # Interfaces & contracts (zero dependencies)
├── PckTool.Core/               # Core library (Wwise format implementations)
├── PckTool/                    # CLI application
└── tests/
    └── PckTool.Core.Tests/     # Unit tests
```

## Layers

### 1. PckTool.Abstractions

**Purpose**: Define contracts and interfaces with zero external dependencies.

**Key Interfaces**:

- `IPckFile` - Core PCK file operations (load, save, query, modify)
- `IPckFileFactory` - Factory for creating/loading PCK files
- `ISoundBank` - Wwise soundbank operations
- `ISoundBankBuilder` - Fluent builder for creating soundbanks from scratch
- `IMediaCollection` - Embedded media collection
- `IStreamingFileCollection` - Streaming file collection
- `IHircItem` - HIRC hierarchy items

**Patterns**:

- `Result<T>` / `Result` - Error handling without exceptions
- `WemReplacementResult` - Result type for WEM replacement operations

### 2. PckTool.Core

**Purpose**: Implement Wwise file format parsing, modification, and serialization.

**Key Components**:

#### WWise/Pck/ - Package File Handling

- `PckFile` - Main PCK file class (implements `IPckFile`)
- `PckFileFactory` - Factory implementation (implements `IPckFileFactory`)
- `SoundBankLut` - Sound bank lookup table
- `StreamingFileLut` - Streaming file lookup table
- `StringMap` - Language name mapping

#### WWise/Bnk/ - Sound Bank Handling

- `SoundBank` - BNK file parsing and serialization (implements `ISoundBank`)
- `SoundBankBuilder` - Fluent builder (implements `ISoundBankBuilder`)
- `MediaCollection` - Embedded WEM audio
- `HircCollection` - HIRC items collection

#### WWise/Bnk/Chunks/ - BNK Chunk Types

- `BkhdChunk` - Bank header
- `DidxChunk` - Data index
- `DataChunk` - Audio data
- `HircChunk` - Hierarchy data
- And more...

### 3. PckTool (CLI)

**Purpose**: Command-line interface for end users.

**Commands**:

- `dump` - Extract sound banks and WEM files
- `replace` - Replace a sound bank
- `replace-wem` - Replace a WEM file
- `list` - List sound banks
- `browse` - Interactive browser
- `sounds` - List sounds in a specific bank
- `find` - Search for WEM IDs or cue names
- `info` - Show configuration info

**Batch Commands**:

- `batch create` - Create a new batch project file
- `batch run` - Execute a batch project
- `batch info` - Show batch project information
- `batch add-action` - Add an action to a batch project
- `batch rm-action` - Remove an action from a batch project
- `batch validate` - Validate a batch project configuration
- `batch schema` - Generate JSON schema for batch projects

**Services**:

- `ServiceProvider` - Lightweight service locator for testability

## Design Patterns

### Factory Pattern

```csharp
// Create empty PCK file
var pck = ServiceProvider.PckFileFactory.Create();

// Load existing PCK file
var pck = ServiceProvider.PckFileFactory.Load("path/to/sounds.pck");
```

### Builder Pattern

```csharp
var soundBank = ServiceProvider.CreateSoundBankBuilder()
    .WithId(0x12345678)
    .WithVersion(0x71)
    .WithLanguage(0)
    .AddMedia(sourceId, wemData)
    .Build();
```

### Result Pattern

```csharp
var result = pckFile.ReplaceWem(sourceId, newData);
if (result.IsSuccess)
    Console.WriteLine($"Replaced {result.ReplacementCount} instances");
```

## Data Flow

```
User Input → CLI → Core → File System
                ↓
         Abstractions (contracts)
```

1. CLI receives user commands
2. CLI uses factories from ServiceProvider
3. Core implements the abstractions
4. Core reads/writes Wwise format files

## Wwise File Format

### PCK Package Structure

```
AKPK Header
├── Languages (StringMap)
├── Sound Banks (SoundBankLut)
│   └── Bank entries → BNK data
└── Streaming Files (StreamingFileLut)
    └── WEM entries → Audio data
```

### BNK SoundBank Structure

```
BNK File
├── BKHD (Bank Header)
├── DIDX (Data Index)
├── DATA (Audio Data)
├── HIRC (Hierarchy)
├── STID (String IDs)
└── Other chunks...
```

## Testing

Tests are located in `tests/PckTool.Core.Tests/` and cover:

- Serialization round-trips
- Equality comparisons
- WEM replacement functionality
- HIRC item parsing
