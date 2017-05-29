using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace UE4TextConverter.Model
{
    internal class TextConverter
    {
        public List<LocSection> LocSections { get; private set; }

        public void LoadLocresFile(string path)
        {
            LocSections = new List<LocSection>();

            using (FileStream fileStream = File.Open(path, FileMode.Open))
            {
                LocBinaryReader reader = new LocBinaryReader(fileStream, Encoding.Unicode);
                uint numberOfSections = reader.ReadUInt32();

                for (int i = 0; i < numberOfSections; i++)
                {
                    LocSection currentSection = new LocSection() { Name = reader.ReadString() };
                    int numberOfEntries = reader.ReadInt32();

                    for (int y = 0; y < numberOfEntries; y++)
                    {
                        TextEntry entry = new TextEntry()
                        {
                            EntryId = reader.ReadString(),
                            Hash = reader.ReadUInt32(),
                            Entry = reader.ReadString()
                        };
                        currentSection.Entries.Add(entry);
                    }
                    LocSections.Add(currentSection);
                }
            }
        }

        public void LoadTextFile(string path)
        {
            LocSections = new List<LocSection>();

            using (FileStream fileStream = File.Open(path, FileMode.Open))
            {
                StreamReader reader = new StreamReader(fileStream, Encoding.Unicode);
                LocSection currentSection = null;
                string line = null;

                while ((line = reader.ReadLine()) != null)
                {
                    // new section
                    if(line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = new LocSection() { Name = line.Split('[', ']')[1] };
                        LocSections.Add(currentSection);
                    }
                    else if(currentSection != null)
                    {
                        string[] tokens = line.Split(new Char[] {'\t', '='}, 3);

                        TextEntry entry = new TextEntry()
                        {
                            Hash = Convert.ToUInt32(tokens[0], 16),
                            EntryId = tokens[1],
                            Entry = tokens[2]
                        };
                        currentSection.Entries.Add(entry);
                    }
                }
            }
        }

        public void WriteLocresFile(string path)
        {
            using (FileStream fileStream = File.Open(path, FileMode.Create))
            {
                using (LocBinaryWriter writer = new LocBinaryWriter(fileStream, Encoding.Unicode))
                {
                    writer.Write(LocSections.Count);
                    foreach (LocSection locSection in LocSections)
                    {
                        writer.Write(locSection.Name);
                        writer.Write(locSection.Entries.Count);
                        foreach (TextEntry textEntry in locSection.Entries)
                        {
                            writer.Write(textEntry.EntryId);
                            writer.Write(textEntry.Hash);
                            writer.Write(textEntry.Entry);
                        }
                    }
                }
            }
        }

        public void WriteTextFile(string path)
        {
            using (FileStream fileStream = File.Open(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream, Encoding.Unicode))
                {
                    foreach (LocSection locSection in LocSections)
                    {
                        writer.WriteLine("[{0}]", locSection.Name);
                        foreach (TextEntry textEntry in locSection.Entries)
                        {
                            writer.WriteLine("{0:X8}\t{1}={2}", textEntry.Hash, textEntry.EntryId, textEntry.Entry);
                        }
                    }
                }
            }
        }
    }
}
