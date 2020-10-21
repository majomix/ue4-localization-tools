using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UE4TextConverter.Model
{
    internal class TextConverter
    {
        public LocresFile Locres { get; private set; }

        public void LoadLocresFile(string path)
        {
            Locres = new LocresFile();

            using (var fileStream = File.Open(path, FileMode.Open))
            {
                using (var reader = new LocBinaryReader(fileStream))
                {
                    var signature = reader.ReadBytes(16);
                    if (LocresFile.Version4Signature.SequenceEqual(signature))
                    {
                        Locres.Flags = reader.ReadByte();
                        Locres.Version = Locres.Flags > 1 ? EngineVersion.Version4_2 : EngineVersion.Version4;
                        Locres.DataSectionStart = reader.ReadInt64();
                    }
                    else
                    {
                        Locres.Version = EngineVersion.Version3;
                        fileStream.Seek(0, SeekOrigin.Begin);
                    }

                    if (Locres.Version == EngineVersion.Version4_2)
                    {
                        var numberOfEntries = reader.ReadInt32();
                    }
                    var numberOfSections = reader.ReadUInt32();

                    for (var i = 0; i < numberOfSections; i++)
                    {
                        var currentSection = new LocSection();
                        if (Locres.Version == EngineVersion.Version4_2)
                        {
                            currentSection.NamespaceHash = reader.ReadUInt32();
                        }
                        currentSection.Name = reader.ReadString();
                        var numberOfEntries = reader.ReadInt32();

                        for (var y = 0; y < numberOfEntries; y++)
                        {
                            uint hash2 = 0;
                            if (Locres.Version == EngineVersion.Version4_2)
                            {
                                hash2 = reader.ReadUInt32();
                            }
                            var entryId = reader.ReadString();
                            var hash = reader.ReadUInt32();
                            string text = null;
                            int numberOfLine = -1;

                            if (Locres.Version == EngineVersion.Version3)
                            {
                                text = reader.ReadString();
                            }
                            else
                            {
                                numberOfLine = reader.ReadInt32();
                            }

                            var entry = new TextEntry
                            {
                                EntryId = entryId,
                                Hash = hash,
                                Hash2 = hash2,
                                Entry = text,
                                NumberOfLine = numberOfLine
                            };
                            currentSection.Entries.Add(entry);
                        }
                        Locres.LocSections.Add(currentSection);
                    }

                    if (Locres.Version == EngineVersion.Version4 || Locres.Version == EngineVersion.Version4_2)
                    {
                        var numberOfTextEntries = reader.ReadInt32();
                        var entries = Locres.LocSections.SelectMany(section => section.Entries).ToList();
                        
                        for (var i = 0; i < numberOfTextEntries; i++)
                        {
                            var localClosureI = i;
                            var entriesToModify = entries.Where(entry => entry.NumberOfLine == localClosureI);

                            var text = reader.ReadString();

                            if (Locres.Version == EngineVersion.Version4_2)
                            {
                                var numberOfReferences = reader.ReadInt32();
                            }

                            foreach (var entryToModify in entriesToModify)
                            {
                                entryToModify.Entry = text;
                            }
                        }
                    }
                }
            }
        }

        public void LoadTextFile(string path)
        {
            Locres = new LocresFile();

            using (var fileStream = File.Open(path, FileMode.Open))
            {
                var reader = new StreamReader(fileStream, Encoding.Unicode);
                LocSection currentSection = null;
                string line;
                var isFirstLine = true;
                var listOfStrings = new List<string>();

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (isFirstLine)
                    {
                        isFirstLine = false;

                        if (line.Equals("v4"))
                        {
                            Locres.Version = EngineVersion.Version4;
                            Locres.Flags = 1;
                            continue;
                        }

                        if (line.Equals("v4.2"))
                        {
                            Locres.Version = EngineVersion.Version4_2;
                            Locres.Flags = 2;
                            continue;
                        }
                    }

                    if (Locres.Version != EngineVersion.Version4_2)
                    {
                        // new section
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            currentSection = new LocSection
                                {Name = line.Equals("[]") ? string.Empty : line.Split('[', ']')[1]};
                            Locres.LocSections.Add(currentSection);
                        }
                        else if (currentSection != null)
                        {
                            string[] tokens = line.Split(new[] { '\t', '=' }, 3);

                            if (tokens.Length == 3)
                            {
                                var text = tokens[2];
                                var lineIndex = listOfStrings.IndexOf(text);

                                if (lineIndex == -1)
                                {
                                    lineIndex = listOfStrings.Count;
                                    listOfStrings.Add(text);
                                }

                                var entryIds = tokens[1].Split(',');
                                foreach (var entryId in entryIds)
                                {
                                    var entry = new TextEntry
                                    {
                                        Hash = Convert.ToUInt32(tokens[0], 16),
                                        EntryId = entryId,
                                        Entry = tokens[2],
                                        NumberOfLine = lineIndex
                                    };
                                    currentSection.Entries.Add(entry);
                                }
                            }
                        }
                    }
                    else
                    {
                        // new section
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            var insideBrackets = line.Split('[', ']')[1];
                            var split = insideBrackets.Split('|');
                            currentSection = new LocSection { Name = split[0], NamespaceHash = Convert.ToUInt32(split[1], 16) };
                            Locres.LocSections.Add(currentSection);
                        }
                        else if (currentSection != null)
                        {
                            string[] tokens = line.Split(new[] { '\t' }, 4);

                            if (tokens.Length == 4)
                            {
                                var text = tokens[3];
                                var lineIndex = listOfStrings.IndexOf(text);

                                if (lineIndex == -1)
                                {
                                    lineIndex = listOfStrings.Count;
                                    listOfStrings.Add(text);
                                }

                                var entryIds = tokens[2].Split(',');
                                foreach (var entryId in entryIds)
                                {
                                    var entry = new TextEntry
                                    {
                                        Hash = Convert.ToUInt32(tokens[0], 16),
                                        Hash2 = Convert.ToUInt32(tokens[1], 16),
                                        EntryId = entryId,
                                        Entry = text,
                                        NumberOfLine = lineIndex
                                    };
                                    currentSection.Entries.Add(entry);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void WriteLocresFile(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Create))
            {
                using (var writer = new LocBinaryWriter(fileStream, Encoding.Unicode))
                {
                    if (Locres.Version == EngineVersion.Version4 || Locres.Version == EngineVersion.Version4_2)
                    {
                        writer.Write(LocresFile.Version4Signature);
                        writer.Write(Locres.Flags);
                        writer.Write(Locres.DataSectionStart);
                    }
                    
                    if (Locres.Version == EngineVersion.Version4_2)
                    {
                        writer.Write(Locres.LocSections.Sum(s => s.Entries.Count));
                    }
                    
                    writer.Write(Locres.LocSections.Count);
                    foreach (var locSection in Locres.LocSections)
                    {
                        if (Locres.Version == EngineVersion.Version4_2)
                        {
                            writer.Write(locSection.NamespaceHash);
                        }

                        writer.Write(locSection.Name, Locres.Version);
                        writer.Write(locSection.Entries.Count);
                        foreach (var textEntry in locSection.Entries)
                        {
                            if (Locres.Version == EngineVersion.Version4_2)
                            {
                                writer.Write(textEntry.Hash2);
                            }

                            writer.Write(textEntry.EntryId.Replace("\\n", "\n").Replace("\\r", "\r"), Locres.Version);
                            writer.Write(textEntry.Hash);

                            if (Locres.Version == EngineVersion.Version3)
                            {
                                writer.Write(textEntry.Entry.Replace("\\n", "\n").Replace("\\r", "\r"), Locres.Version);
                            }
                            else if (Locres.Version == EngineVersion.Version4 || Locres.Version == EngineVersion.Version4_2)
                            {
                                writer.Write(textEntry.NumberOfLine);
                            }
                        }
                    }

                    if (Locres.Version == EngineVersion.Version4 || Locres.Version == EngineVersion.Version4_2)
                    {
                        Locres.DataSectionStart = writer.BaseStream.Position;
                        var distinctTextLines = Locres.LocSections.SelectMany(section => section.Entries).GroupBy(g => g.Entry).ToList();

                        writer.Write(distinctTextLines.Count);

                        for (var i = 0; i < distinctTextLines.Count; i++)
                        {
                            var line = distinctTextLines[i];

                            writer.Write(line.First().Entry.Replace("\\n", "\n").Replace("\\r", "\r"), Locres.Version);

                            if (Locres.Version == EngineVersion.Version4_2)
                            {
                                writer.Write(line.Count());
                            }
                        }

                        writer.BaseStream.Seek(17, SeekOrigin.Begin);
                        writer.Write(Locres.DataSectionStart);
                    }
                }
            }
        }

        public void WriteTextFile(string path, bool compact, bool sortById)
        {
            using (var fileStream = File.Open(path, FileMode.Create))
            {
                using (var writer = new StreamWriter(fileStream, Encoding.Unicode))
                {
                    switch (Locres.Version)
                    {
                        case EngineVersion.Version4:
                            writer.WriteLine("v4");
                            break;
                        case EngineVersion.Version4_2:
                            writer.WriteLine("v4.2");
                            break;
                    }

                    foreach (var locSection in Locres.LocSections)
                    {
                        switch (Locres.Version)
                        {
                            case EngineVersion.Version4:
                                writer.WriteLine($"[{locSection.Name}]");
                                break;
                            case EngineVersion.Version4_2:
                                writer.WriteLine($"[{locSection.Name}|{locSection.NamespaceHash:X8}]");
                                break;
                        }

                        if (!compact)
                        {
                            var entries = locSection.Entries;

                            if (sortById)
                            {
                                entries = entries.OrderBy(entry => entry.EntryId).ToList();
                            }

                            foreach (var textEntry in entries)
                            {
                                if (Locres.Version != EngineVersion.Version4_2)
                                {
                                    writer.WriteLine("{0:X8}\t{1}={2}", textEntry.Hash, textEntry.EntryId, textEntry.Entry.Replace("\n", "\\n").Replace("\r", "\\r"));
                                }
                                else
                                {
                                    writer.WriteLine("{0:X8}\t{1:X8}\t{2}\t{3}", textEntry.Hash, textEntry.Hash2, textEntry.EntryId.Replace("\n", "\\n").Replace("\r", "\\r"), textEntry.Entry.Replace("\n", "\\n").Replace("\r", "\\r"));
                                }
                            }
                        }
                        else
                        {
                            foreach (var grouping in locSection.Entries.GroupBy(entry => entry.Hash))
                            {
                                var lineIds = string.Join(",", grouping.Select(group => group.EntryId));
                                var secondaryHashes = string.Join(",", grouping.Select(group => group.Hash2.ToString("X8")));

                                if (Locres.Version != EngineVersion.Version4_2)
                                {
                                    writer.WriteLine("{0:X8}\t{1}={2}", grouping.Key, lineIds,
                                        grouping.First().Entry.Replace("\n", "\\n").Replace("\r", "\\r"));
                                }
                                else
                                {
                                    writer.WriteLine("{0:X8}\t{1}\t{2}\t{3}", grouping.Key, secondaryHashes, lineIds,
                                        grouping.First().Entry.Replace("\n", "\\n").Replace("\r", "\\r"));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
