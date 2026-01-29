namespace PckTool.Abstractions.Batch;

/// <summary>
///     Represents a batch project that defines operations to perform on Wwise audio files.
/// </summary>
public interface IBatchProject
{
    /// <summary>
    ///     Gets or sets the project name.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///     Gets or sets an optional description for the project.
    /// </summary>
    string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the schema version for forward compatibility.
    /// </summary>
    int SchemaVersion { get; set; }

    /// <summary>
    ///     Gets or sets the list of input file paths (.pck or .bnk files).
    /// </summary>
    IList<string> InputFiles { get; set; }

    /// <summary>
    ///     Gets or sets the output directory for modified files.
    ///     If null, files may be modified in place or use a default location.
    /// </summary>
    string? OutputDirectory { get; set; }

    /// <summary>
    ///     Gets the list of actions to perform.
    /// </summary>
    IList<IProjectAction> Actions { get; }

    /// <summary>
    ///     Gets or sets when the project was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Gets or sets when the project was last modified.
    /// </summary>
    DateTime ModifiedAt { get; set; }

    /// <summary>
    ///     Validates the entire project configuration.
    /// </summary>
    /// <returns>A result indicating whether the project is valid.</returns>
    BatchProjectValidationResult Validate();
}
