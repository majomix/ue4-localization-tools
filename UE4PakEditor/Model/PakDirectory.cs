using System.Collections.Generic;

namespace UE4PakEditor.Model
{
    internal class PakDirectory
    {
        public string Name { get; set; }
        public int NumberOfEntries { get; set; }
        public List<PakEntry> Entries { get; set; }

        public PakDirectory()
        {
            Entries = new List<PakEntry>();
        }
    }
}
