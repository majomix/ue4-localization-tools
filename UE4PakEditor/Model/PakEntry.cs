using System;
using System.Diagnostics;
using System.Linq;

namespace UE4PakEditor.Model
{
    [DebuggerDisplay("Name:{Name}, Import:{Import}, Offset:{Offset}, Size:{UncompressedSize}")]
    internal class PakEntry
    {
        public string Name { get; set; }
        public Int64 Offset { get; set; }
        public Int64 CompressedSize { get; set; }
        public Int64 UncompressedSize { get; set; }
        public int CompressionType { get; set; }
        public byte[] Hash { get; set; }
        public byte EncryptionType { get; set; }
        public int ChunkSize { get; set; }
        public bool Extract { get; set; }
        public string Import { get; set; }

        public bool Equals(PakEntry entry)
        {
            if (entry == null) return false;
            if (CompressedSize != entry.CompressedSize) return false;
            if (UncompressedSize != entry.UncompressedSize) return false;
            if (CompressionType != entry.CompressionType) return false;
            if (!Hash.SequenceEqual(entry.Hash)) return false;
            if (EncryptionType != entry.EncryptionType) return false;
            if (ChunkSize != entry.ChunkSize) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            PakEntry comparing = obj as PakEntry;
            if (comparing == null) return false;
            return Equals(comparing);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
