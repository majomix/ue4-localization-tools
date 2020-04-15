using System.Collections.Generic;

namespace UE4TextConverter.Model
{
    public class LocSection
    {
        public string Name { get; set; }
        public List<TextEntry> Entries { get; private set; }

        public LocSection()
        {
            Entries = new List<TextEntry>();
        }
    }
}
