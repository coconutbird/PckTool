namespace SoundsUnpack.WWise;

public class FilePackageLut<TKey> where TKey : notnull
{
    public bool Read(BinaryReader reader, uint size)
    {
        if (typeof(TKey) != typeof(uint) && typeof(TKey) != typeof(int) && typeof(TKey) != typeof(ulong) &&
            typeof(TKey) != typeof(long))
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
                fileId = (TKey)(object)reader.ReadUInt32();
            }
            else if (typeof(TKey) == typeof(int))
            {
                fileId = (TKey)(object)reader.ReadInt32();
            }
            else if (typeof(TKey) == typeof(ulong))
            {
                fileId = (TKey)(object)reader.ReadUInt64();
            }
            else
            {
                fileId = (TKey)(object)reader.ReadInt64();
            }

            var blockSize = reader.ReadUInt32();
            var fileSize = reader.ReadInt32();
            var startBlock = reader.ReadUInt32();
            var languageId = reader.ReadUInt32();

            var position = reader.BaseStream.Position;

            reader.BaseStream.Seek(startBlock, SeekOrigin.Begin);

            var buffer = reader.ReadBytes(fileSize);

            reader.BaseStream.Position = position;

            Entries.Add(new FileEntry
            {
                FileId = fileId,
                BlockSize = blockSize,
                FileSize = fileSize,
                StartBlock = startBlock,
                LanguageId = languageId,
                Data = buffer
            });
        }

        reader.BaseStream.Position = baseOffset + size;

        return true;
    }

    public List<FileEntry> Entries { get; } = [];

    public class FileEntry
    {
        /// <summary>
        /// The unique identifier of the file. This is an FNV1A-32 or FNV1A-64 hash of the file name.
        /// </summary>
        public required TKey FileId { get; init; }

        /// <summary>
        /// The block size of the file in bytes.
        /// This is the alignment requirement for the file data in the package.
        /// </summary>
        public required uint BlockSize { get; init; }

        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        public required long FileSize { get; init; }

        /// <summary>
        /// The starting file offset (in bytes) of the file data in the package.
        /// </summary>
        public required uint StartBlock { get; init; }

        /// <summary>
        /// The language ID of the file.
        /// </summary>
        public required uint LanguageId { get; init; }

        // public required SubChunk[] Chunks { get; init; }
        /// <summary>
        /// The raw file buffer.
        /// </summary>
        public required byte[] Data { get; init; }

        /// <summary>
        /// Returns the magic number of the file (first 4 bytes).
        /// </summary>
        public uint Magic => BitConverter.ToUInt32(Data, 0);

        /// <summary>
        /// Returns the magic string of the file (first 4 bytes as ASCII).
        /// </summary>
        public string MagicString => System.Text.Encoding.ASCII.GetString(Data, 0, 4);
    }
}