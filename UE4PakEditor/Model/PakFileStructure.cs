using System;

namespace UE4PakEditor.Model
{
    internal class PakFileStructure
    {
        public int Signature { get; set; }
        public int Version { get; set; }
        public Int64 DataSize { get; set; }
        public Int64 FileTreeSize { get; set; }
        public byte[] Hash { get; set; }
        public PakDirectory Directory { get; set; }
    }
}
