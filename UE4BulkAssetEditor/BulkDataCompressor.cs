using System.IO.Compression;

namespace UE4BulkAssetEditor;

public class BulkDataCompressor
{
    private const uint PACKAGE_FILE_TAG = 0x9E2A83C1;
    private const int MAX_BLOCK_SIZE = 131072; // 128 KB

    public byte[] Compress(byte[] uncompressedData)
    {
        int blockCount = (int)((uncompressedData.Length + MAX_BLOCK_SIZE - 1) / MAX_BLOCK_SIZE);

        // Compress each block first to know sizes
        var compressedBlocks = new byte[blockCount][];
        for (int i = 0; i < blockCount; i++)
        {
            int offset = i * MAX_BLOCK_SIZE;
            int length = Math.Min(MAX_BLOCK_SIZE, uncompressedData.Length - offset);

            using var ms = new MemoryStream();
            using (var zlib = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
            {
                zlib.Write(uncompressedData, offset, length);
            }
            compressedBlocks[i] = ms.ToArray();
        }

        // Calculate total compressed size (just the zlib data, no header)
        long totalCompressed = 0;
        foreach (var block in compressedBlocks)
            totalCompressed += block.Length;

        // Write header + compressed blocks
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output);

        // Header
        writer.Write(PACKAGE_FILE_TAG);
        writer.Write(0); // padding
        writer.Write((long)MAX_BLOCK_SIZE);
        writer.Write(totalCompressed);
        writer.Write((long)uncompressedData.Length);

        // Per-block table
        for (int i = 0; i < blockCount; i++)
        {
            int uncompLength = Math.Min(MAX_BLOCK_SIZE, uncompressedData.Length - i * MAX_BLOCK_SIZE);
            writer.Write((long)compressedBlocks[i].Length);
            writer.Write((long)uncompLength);
        }

        // Compressed data
        for (int i = 0; i < blockCount; i++)
        {
            writer.Write(compressedBlocks[i]);
        }

        // Trailing tag (footer sentinel)
        writer.Write(PACKAGE_FILE_TAG);

        return output.ToArray();
    }
}
