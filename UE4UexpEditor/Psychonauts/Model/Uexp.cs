using System.Collections.Generic;

namespace UE4UexpEditor.Psychonauts.Model
{
    public class Uexp
    {
        public string InputPath;
        public string ExportPath;
        public string RebuildPath;
        public List<UexpDescriptor> Descriptors = new List<UexpDescriptor>();
        public List<Identifier> Identifiers = new List<Identifier>();
        public List<string> Strings = new List<string>();
        public List<Lookup> Lookup = new List<Lookup>();
        public byte[] Metadata1;
        public byte[] Metadata2;
        public Uasset Parent;
    }
}
