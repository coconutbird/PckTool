using System.Numerics;
using System.Text;

namespace PckTool.WWise;

public class FilePackageLut<TKey> where TKey : INumber<TKey>
{
    public List<FileEntry> Entries { get; } = [];

    public bool Read(BinaryReader reader, uint size)
    {
        if (typeof(TKey) != typeof(uint)
            && typeof(TKey) != typeof(int)
            && typeof(TKey) != typeof(ulong)
            && typeof(TKey) != typeof(long))
        {
            throw new NotSupportedException("Only uint keys are supported in this example.");
        }

        Entries.Clear();

        var baseOffset = reader.BaseStream.Position;

        var fileCount = reader.ReadUInt32();

        for (var i = 0; i < fileCount; ++i)
        {
            TKey fileId;

            if (typeof(TKey) == typeof(uint))
            {
                fileId = (TKey) (object) reader.ReadUInt32();
            }
            else if (typeof(TKey) == typeof(int))
            {
                fileId = (TKey) (object) reader.ReadInt32();
            }
            else if (typeof(TKey) == typeof(ulong))
            {
                fileId = (TKey) (object) reader.ReadUInt64();
            }
            else
            {
                fileId = (TKey) (object) reader.ReadInt64();
            }

            var blockSize = reader.ReadUInt32();
            var fileSize = reader.ReadInt32();
            var startBlock = reader.ReadUInt32();
            var languageId = reader.ReadUInt32();

            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(startBlock, SeekOrigin.Begin);

            var buffer = reader.ReadBytes(fileSize);

            reader.BaseStream.Position = position;

            var entry = new FileEntry
            {
                FileId = fileId,
                BlockSize = blockSize,
                StartBlock = startBlock,
                LanguageId = languageId,
                Data = buffer
            };

            if (!entry.IsValid)
            {
                return false;
            }

            Entries.Add(entry);
        }

        reader.BaseStream.Position = baseOffset + size;

        return true;
    }

    /// <summary>
    ///     Writes the LUT header entries to a BinaryWriter and returns the total size written.
    ///     Note: This only writes the LUT metadata, not the actual file data.
    ///     The StartBlock values in entries should be updated before calling this.
    /// </summary>
    public uint Write(BinaryWriter writer)
    {
        var startPosition = writer.BaseStream.Position;

        // Write count
        writer.Write((uint) Entries.Count);

        foreach (var entry in Entries)
        {
            // Write file ID based on type
            if (typeof(TKey) == typeof(uint))
            {
                writer.Write((uint) (object) entry.FileId);
            }
            else if (typeof(TKey) == typeof(int))
            {
                writer.Write((int) (object) entry.FileId);
            }
            else if (typeof(TKey) == typeof(ulong))
            {
                writer.Write((ulong) (object) entry.FileId);
            }
            else
            {
                writer.Write((long) (object) entry.FileId);
            }

            writer.Write(entry.BlockSize);
            writer.Write(entry.Data.Length);
            writer.Write(entry.StartBlock);
            writer.Write(entry.LanguageId);
        }

        return (uint) (writer.BaseStream.Position - startPosition);
    }

    /// <summary>
    ///     Calculates the size in bytes that the LUT header will occupy when written.
    /// </summary>
    public uint CalculateHeaderSize()
    {
        // 4 bytes for count
        // Per entry: FileId (4 or 8 bytes) + BlockSize (4) + FileSize (4) + StartBlock (4) + LanguageId (4)
        var fileIdSize = typeof(TKey) == typeof(ulong) || typeof(TKey) == typeof(long) ? 8u : 4u;
        var entrySize = fileIdSize + 4 + 4 + 4 + 4;

        return 4 + (uint) (Entries.Count * entrySize);
    }

    /// <summary>
    ///     Calculates the total size in bytes of all file data in this LUT.
    /// </summary>
    public uint CalculateTotalDataSize()
    {
        uint totalSize = 0;

        foreach (var entry in Entries)
        {
            // Align to block size if needed
            if (entry.BlockSize > 0 && totalSize % entry.BlockSize != 0)
            {
                totalSize += entry.BlockSize - totalSize % entry.BlockSize;
            }

            totalSize += (uint) entry.Data.Length;
        }

        return totalSize;
    }

    public class FileEntry
    {
        /// <summary>
        ///     Validates that the buffer and metadata are consistent.
        ///     Returns true if:
        ///     - Data is not null and has content
        ///     - Data.Length does not exceed FileSize (aligned size)
        ///     - StartBlock is aligned to BlockSize (if BlockSize > 0)
        /// </summary>
        public bool IsValid
        {
            get
            {
                // Data must exist and have content
                if (Data.Length == 0) return false;

                // Data length should not exceed the aligned file size
                if (Data.Length > FileSize) return false;

                // StartBlock should be aligned to BlockSize
                if (BlockSize > 0 && StartBlock % BlockSize != 0) return false;

                return true;
            }
        }

        /// <summary>
        ///     The unique identifier of the file. This is an FNV1A-32 or FNV1A-64 hash of the file name.
        /// </summary>
        public required TKey FileId { get; set; }

        /// <summary>
        ///     The block size of the file in bytes.
        ///     This is the alignment requirement for the file data in the package.
        /// </summary>
        public required uint BlockSize { get; set; }

        /// <summary>
        ///     The size of the file in bytes, aligned to BlockSize.
        /// </summary>
        public long FileSize
        {
            get
            {
                var size = Data.Length;

                if (BlockSize > 0 && size % BlockSize != 0)
                {
                    size += (int) (BlockSize - size % BlockSize);
                }

                return size;
            }
        }

        /// <summary>
        ///     The starting file offset (in bytes) of the file data in the package.
        /// </summary>
        public required uint StartBlock { get; set; }

        /// <summary>
        ///     The language ID of the file.
        /// </summary>
        public required uint LanguageId { get; set; }

        /// <summary>
        ///     The raw file buffer.
        /// </summary>
        public required byte[] Data { get; set; }

        /// <summary>
        ///     Returns the magic number of the file (first 4 bytes).
        /// </summary>
        public uint Magic => BitConverter.ToUInt32(Data, 0);

        /// <summary>
        ///     Returns the magic string of the file (first 4 bytes as ASCII).
        /// </summary>
        public string MagicString => Encoding.ASCII.GetString(Data, 0, 4);
    }
}
