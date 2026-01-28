using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PckTool.Core.WWise.Pck;

/// <summary>
///     Base class for all file entries in a package LUT.
///     Provides common functionality for data storage, replacement, and change tracking.
/// </summary>
/// <typeparam name="TKey">The type of the file ID (uint or ulong).</typeparam>
public abstract class FileEntry<TKey> : INotifyPropertyChanged, IEquatable<FileEntry<TKey>>
    where TKey : struct, INumber<TKey>
{
    private byte[]? _originalData;
    private byte[]? _replacementData;
    private string? _replacementPath;

    /// <summary>
    ///     The unique identifier of the file (FNV1A hash of the file name).
    /// </summary>
    public required TKey Id { get; init; }

    /// <summary>
    ///     The language ID of the file.
    /// </summary>
    public required uint LanguageId { get; init; }

    /// <summary>
    ///     The block size alignment requirement for this file.
    /// </summary>
    public required uint BlockSize { get; init; }

    /// <summary>
    ///     The original starting offset in the package file.
    /// </summary>
    public uint StartBlock { get; set; }

    /// <summary>
    ///     Whether this entry has been modified from its original state.
    /// </summary>
    public bool IsModified => _replacementData is not null || _replacementPath is not null;

    /// <summary>
    ///     The size of the file data in bytes.
    /// </summary>
    public long Size => GetData().Length;

    /// <summary>
    ///     The aligned size of the file data (rounded up to BlockSize).
    /// </summary>
    public long AlignedSize
    {
        get
        {
            var size = Size;

            if (BlockSize > 0 && size % BlockSize != 0)
            {
                size += BlockSize - size % BlockSize;
            }

            return size;
        }
    }

    /// <summary>
    ///     Returns true if the data appears valid (has content).
    /// </summary>
    public bool IsValid => GetData().Length > 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Gets the file data, loading from replacement source if modified.
    /// </summary>
    public byte[] GetData()
    {
        if (_replacementData is not null)
        {
            return _replacementData;
        }

        if (_replacementPath is not null)
        {
            _replacementData = File.ReadAllBytes(_replacementPath);

            return _replacementData;
        }

        return _originalData ?? [];
    }

    /// <summary>
    ///     Replaces the file data with new bytes.
    /// </summary>
    public void ReplaceWith(byte[] data)
    {
        _replacementData = data;
        _replacementPath = null;
        OnPropertyChanged(nameof(IsModified));
        OnPropertyChanged(nameof(Size));
    }

    /// <summary>
    ///     Replaces the file data with contents from a file path.
    ///     The file is read lazily when GetData() is called.
    /// </summary>
    public void ReplaceWith(string filePath)
    {
        _replacementPath = filePath;
        _replacementData = null;
        OnPropertyChanged(nameof(IsModified));
        OnPropertyChanged(nameof(Size));
    }

    /// <summary>
    ///     Reverts any modifications, restoring original data.
    /// </summary>
    public void Revert()
    {
        _replacementData = null;
        _replacementPath = null;
        OnPropertyChanged(nameof(IsModified));
        OnPropertyChanged(nameof(Size));
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///     Sets the original data (used during loading).
    /// </summary>
    internal void SetOriginalData(byte[] data)
    {
        _originalData = data;
    }

    /// <summary>
    ///     Determines whether this entry is equal to another entry.
    ///     Compares Id, LanguageId, BlockSize, and data content.
    /// </summary>
    public bool Equals(FileEntry<TKey>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Id.Equals(other.Id)
               && LanguageId == other.LanguageId
               && BlockSize == other.BlockSize
               && GetData().AsSpan().SequenceEqual(other.GetData());
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as FileEntry<TKey>);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, LanguageId, BlockSize);
    }

    public static bool operator ==(FileEntry<TKey>? left, FileEntry<TKey>? right)
    {
        if (left is null) return right is null;

        return left.Equals(right);
    }

    public static bool operator !=(FileEntry<TKey>? left, FileEntry<TKey>? right)
    {
        return !(left == right);
    }
}
