using System.IO;
using System.Text;

namespace UE4PakEditor.Model
{
    internal class PakBinaryWriter : BinaryWriter
    {
        public PakBinaryWriter(Stream stream)
            : base(stream) { }

        public override void Write(string value)
        {
            value = value + '\0';
            Write(value.Length);
            byte[] buffer = Encoding.ASCII.GetBytes(value);
            Write(buffer);
        }

        public void Write(PakEntry entry, bool fileLevel)
        {
            if (!fileLevel) Write(entry.Name);

            if (fileLevel) Write(0L);
            else Write(entry.Offset);

            Write(entry.CompressedSize);
            Write(entry.UncompressedSize);
            Write(entry.CompressionType);
            Write(entry.Hash);

            if (entry.CompressionType != 0)
            {
                Write(entry.Chunks.Count);

                foreach (var chunk in entry.Chunks)
                {
                    Write(chunk.ChunkOffset);
                    Write(chunk.ChunkEnd);
                }
            }

            Write(entry.EncryptionType);
            Write(entry.ChunkSize);
        }

        public void Write(PakDirectory directory)
        {
            Write(directory.Name);
            Write(directory.Entries.Count);
        }

        public void Write(PakFileStructure archive)
        {
            Write(archive.Signature);
            Write(archive.Version);
            Write(archive.DataSize);
            Write(archive.FileTreeSize);
            Write(archive.Hash);
        }
    }
}
