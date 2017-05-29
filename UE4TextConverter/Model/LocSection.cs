using System.Collections.Generic;

namespace UE4TextConverter.Model
{
    internal class LocSection
    {
        public string Name { get; set; }
        public List<TextEntry> Entries { get; private set; }

        public LocSection()
        {
            Entries = new List<TextEntry>();
        }
    }
}
