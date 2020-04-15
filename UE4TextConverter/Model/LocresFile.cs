using System.Collections.Generic;

namespace UE4TextConverter.Model
{
    public enum EngineVersion
    {
        Version3,
        Version4
    }

    public class LocresFile
    {
        public static byte[] Version4Signature = { 0x0E, 0x14, 0x74, 0x75, 0x67, 0x4A, 0x03, 0xFC, 0x4A, 0x15, 0x90, 0x9D, 0xC3, 0x37, 0x7F, 0x1B };

        public List<LocSection> LocSections { get; }
        public EngineVersion Version { get; set; }
        public byte Flags { get; set; }
        public long DataSectionStart { get; set; }

        public LocresFile()
        {
            LocSections = new List<LocSection>();
        }
    }
}
