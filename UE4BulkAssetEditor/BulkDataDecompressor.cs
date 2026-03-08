using System.IO.Compression;

namespace UE4BulkAssetEditor;

public class BulkDataDecompressor
{
    private const uint PACKAGE_FILE_TAG = 0x9E2A83C1;

    public long MaxBlockSize { get; private set; }
    public long TotalCompressedSize { get; private set; }
    public long TotalUncompressedSize { get; private set; }
    public int BlockCount { get; private set; }

    public byte[] Decompress(byte[] bulkData)
    {
        using var reader = new BinaryReader(new MemoryStream(bulkData));

        uint tag = reader.ReadUInt32();
        if (tag != PACKAGE_FILE_TAG)
            throw new InvalidDataException($"Bad tag: 0x{tag:X8}, expected 0x{PACKAGE_FILE_TAG:X8}");

        reader.ReadUInt32(); // padding
        MaxBlockSize = reader.ReadInt64();
        TotalCompressedSize = reader.ReadInt64();
        TotalUncompressedSize = reader.ReadInt64();

        BlockCount = (int)((TotalUncompressedSize + MaxBlockSize - 1) / MaxBlockSize);
        var blocks = new (long Compressed, long Uncompressed)[BlockCount];
        for (int i = 0; i < BlockCount; i++)
        {
            blocks[i].Compressed = reader.ReadInt64();
            blocks[i].Uncompressed = reader.ReadInt64();
        }

        long dataOffset = reader.BaseStream.Position;
        var output = new byte[TotalUncompressedSize];
        int outputOffset = 0;

        for (int i = 0; i < BlockCount; i++)
        {
            // ZLibStream handles the 2-byte header and 4-byte adler32 natively
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
}
