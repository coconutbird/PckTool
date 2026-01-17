using System.Text.Json;
using System.Text.Json.Serialization;

namespace PckTool.Core.Package;

/// <summary>
///     Represents a PckTool project configuration.
///     Contains paths to files being edited and project settings.
/// </summary>
public class ProjectFile
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    ///     The project file path (set when loaded/saved).
    /// </summary>
    [JsonIgnore] public string? FilePath { get; private set; }

    /// <summary>
    ///     Project name.
    /// </summary>
    public string Name { get; set; } = "Untitled Project";

    /// <summary>
    ///     Path to the PCK file being edited.
    /// </summary>
    public string? PackagePath { get; set; }

    /// <summary>
    ///     Path to the sound table XML file.
    /// </summary>
    public string? SoundTablePath { get; set; }

    /// <summary>
    ///     Path to the game directory (for auto-discovery).
    /// </summary>
    public string? GameDirectory { get; set; }

    /// <summary>
    ///     Output directory for exports.
    /// </summary>
    public string? OutputDirectory { get; set; }

    /// <summary>
    ///     List of bank IDs that are being actively edited.
    /// </summary>
    public List<uint> EditingBanks { get; set; } = new();

    /// <summary>
    ///     List of sound IDs that are being actively edited.
    /// </summary>
    public List<uint> EditingSounds { get; set; } = new();

    /// <summary>
    ///     Custom notes for the project.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    ///     When the project was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     When the project was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Returns true if the project has unsaved changes.
    /// </summary>
    [JsonIgnore] public bool IsDirty { get; private set; }

    /// <summary>
    ///     Creates a new empty project.
    /// </summary>
    public static ProjectFile Create(string name = "Untitled Project")
    {
        return new ProjectFile
        {
            Name = name, CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, IsDirty = true
        };
    }

    /// <summary>
    ///     Loads a project from a JSON file.
    /// </summary>
    public static ProjectFile? Load(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            var project = Load(stream);

            if (project is not null)
            {
                project.FilePath = path;
            }

            return project;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Saves the project to a JSON file.
    /// </summary>
    public bool Save(string? path = null)
    {
        path ??= FilePath;

        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            using var stream = File.Create(path);
            Save(stream);
            FilePath = path;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Saves the project to a stream.
    /// </summary>
    public void Save(Stream stream)
    {
        ModifiedAt = DateTime.UtcNow;
        JsonSerializer.Serialize(stream, this, JsonOptions);
        IsDirty = false;
    }

    /// <summary>
    ///     Loads a project from a stream.
    /// </summary>
    public static ProjectFile? Load(Stream stream)
    {
        try
        {
            var project = JsonSerializer.Deserialize<ProjectFile>(stream, JsonOptions);

            if (project is not null)
            {
                project.IsDirty = false;
            }

            return project;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Marks the project as modified.
    /// </summary>
    public void MarkDirty()
    {
        IsDirty = true;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Adds a bank to the editing list.
    /// </summary>
    public void AddEditingBank(uint bankId)
    {
        if (!EditingBanks.Contains(bankId))
        {
            EditingBanks.Add(bankId);
            MarkDirty();
        }
    }

    /// <summary>
    ///     Removes a bank from the editing list.
    /// </summary>
    public void RemoveEditingBank(uint bankId)
    {
        if (EditingBanks.Remove(bankId))
        {
            MarkDirty();
        }
    }

    /// <summary>
    ///     Adds a sound to the editing list.
    /// </summary>
    public void AddEditingSound(uint soundId)
    {
        if (!EditingSounds.Contains(soundId))
        {
            EditingSounds.Add(soundId);
            MarkDirty();
        }
    }

    /// <summary>
    ///     Removes a sound from the editing list.
    /// </summary>
    public void RemoveEditingSound(uint soundId)
    {
        if (EditingSounds.Remove(soundId))
        {
            MarkDirty();
        }
    }
}
