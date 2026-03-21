using System.IO.Compression;

namespace UE4BulkAssetEditor;

/// <summary>
/// Handles zlib-compressed UE4 bulk data in 128 KB blocks.
/// Format: [Tag][Pad][BlockSize][TotalComp][TotalUncomp][BlockTable...][ZlibData...][SentinelTag]
/// </summary>
public static class BulkDataCodec
{
    private const uint PACKAGE_FILE_TAG = 0x9E2A83C1;
    private const int BLOCK_SIZE = 131072; // 128 KB

    public static byte[] Decompress(byte[] bulkData)
    {
        using var reader = new BinaryReader(new MemoryStream(bulkData));

        uint tag = reader.ReadUInt32();
        if (tag != PACKAGE_FILE_TAG)
            throw new InvalidDataException($"Bad tag: 0x{tag:X8}, expected 0x{PACKAGE_FILE_TAG:X8}");

        reader.ReadUInt32(); // padding
        long maxBlockSize = reader.ReadInt64();
        long totalCompressed = reader.ReadInt64();
        long totalUncompressed = reader.ReadInt64();

        int blockCount = (int)((totalUncompressed + maxBlockSize - 1) / maxBlockSize);
        var blocks = new (long Compressed, long Uncompressed)[blockCount];
        for (int i = 0; i < blockCount; i++)
        {
            blocks[i].Compressed = reader.ReadInt64();
            blocks[i].Uncompressed = reader.ReadInt64();
        }

        long dataOffset = reader.BaseStream.Position;
        var output = new byte[totalUncompressed];
        int outputOffset = 0;

        for (int i = 0; i < blockCount; i++)
        {
            using var compressedStream = new MemoryStream(bulkData, (int)dataOffset, (int)blocks[i].Compressed);
            using var zlib = new ZLibStream(compressedStream, CompressionMode.Decompress);

            int remaining = (int)blocks[i].Uncompressed;
            while (remaining > 0)
            {
                int read = zlib.Read(output, outputOffset, remaining);
                if (read == 0) break;
                outputOffset += read;
                remaining -= read;
            }

            if (remaining != 0)
                throw new InvalidDataException($"Block {i}: expected {blocks[i].Uncompressed} bytes, got {blocks[i].Uncompressed - remaining}");

            dataOffset += blocks[i].Compressed;
        }

        return output;
    }

    public static byte[] Compress(byte[] data)
    {
        int blockCount = (data.Length + BLOCK_SIZE - 1) / BLOCK_SIZE;

        var compressedBlocks = new byte[blockCount][];
        for (int i = 0; i < blockCount; i++)
        {
            int offset = i * BLOCK_SIZE;
            int length = Math.Min(BLOCK_SIZE, data.Length - offset);

            using var ms = new MemoryStream();
            using (var zlib = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
                zlib.Write(data, offset, length);
            compressedBlocks[i] = ms.ToArray();
        }

        long totalCompressed = compressedBlocks.Sum(b => (long)b.Length);

        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output);

        writer.Write(PACKAGE_FILE_TAG);
        writer.Write(0);                     // padding
        writer.Write((long)BLOCK_SIZE);
        writer.Write(totalCompressed);
        writer.Write((long)data.Length);

        for (int i = 0; i < blockCount; i++)
        {
            writer.Write((long)compressedBlocks[i].Length);
            writer.Write((long)Math.Min(BLOCK_SIZE, data.Length - i * BLOCK_SIZE));
        }

        for (int i = 0; i < blockCount; i++)
            writer.Write(compressedBlocks[i]);

        writer.Write(PACKAGE_FILE_TAG); // sentinel

        return output.ToArray();
    }
}
