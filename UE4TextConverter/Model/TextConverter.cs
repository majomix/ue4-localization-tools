using System;
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
                        Locres.Version = EngineVersion.Version4;
                        Locres.Flags = reader.ReadByte();
                        Locres.DataSectionStart = reader.ReadInt64();
                    }
                    else
                    {
                        Locres.Version = EngineVersion.Version3;
                        fileStream.Seek(0, SeekOrigin.Begin);
                    }

                    var numberOfSections = reader.ReadUInt32();

                    for (var i = 0; i < numberOfSections; i++)
                    {
                        var currentSection = new LocSection { Name = reader.ReadString() };
                        var numberOfEntries = reader.ReadInt32();

                        for (var y = 0; y < numberOfEntries; y++)
                        {
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
                                Entry = text,
                                NumberOfLine = numberOfLine
                            };
                            currentSection.Entries.Add(entry);
                        }
                        Locres.LocSections.Add(currentSection);
                    }

                    if (Locres.Version == EngineVersion.Version4)
                    {
                        var numberOfTextEntries = reader.ReadInt32();
                        var entries = Locres.LocSections.SelectMany(section => section.Entries).ToList();
                        
                        for (var i = 0; i < numberOfTextEntries; i++)
                        {
                            var localClosureI = i;
                            var entriesToModify = entries.Where(entry => entry.NumberOfLine == localClosureI);
                            var text = reader.ReadString();

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
                    if (isFirstLine)
                    {
                        isFirstLine = false;

                        if (line.Equals("v4"))
                        {
                            Locres.Version = EngineVersion.Version4;
                            Locres.Flags = 1;
                            continue;
                        }
                    }

                    // new section
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = new LocSection { Name = line.Equals("[]") ? string.Empty : line.Split('[', ']')[1] };
                        Locres.LocSections.Add(currentSection);
                    }
                    else if (currentSection != null)
                    {
                        string[] tokens = line.Split(new[] {'\t', '='}, 3);

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
            }
        }

        public void WriteLocresFile(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Create))
            {
                using (var writer = new LocBinaryWriter(fileStream, Encoding.Unicode))
                {
                    if (Locres.Version == EngineVersion.Version4)
                    {
                        writer.Write(LocresFile.Version4Signature);
                        writer.Write(Locres.Flags);
                        writer.Write(Locres.DataSectionStart);
                    }

                    writer.Write(Locres.LocSections.Count);
                    foreach (var locSection in Locres.LocSections)
                    {
                        writer.Write(locSection.Name);
                        writer.Write(locSection.Entries.Count);
                        foreach (var textEntry in locSection.Entries)
                        {
                            writer.Write(textEntry.EntryId);
                            writer.Write(textEntry.Hash);

                            if (Locres.Version == EngineVersion.Version3)
                            {
                                writer.Write(textEntry.Entry.Replace("\\n", "\n").Replace("\\r", "\r"));
                            }
                            else if (Locres.Version == EngineVersion.Version4)
                            {
                                writer.Write(textEntry.NumberOfLine);
                            }
                        }
                    }

                    if (Locres.Version == EngineVersion.Version4)
                    {
                        Locres.DataSectionStart = writer.BaseStream.Position;
                        var distinctTextLines = Locres.LocSections.SelectMany(section => section.Entries.Select(entry => entry.Entry)).Distinct().ToList();

                        writer.Write(distinctTextLines.Count);

                        foreach (var line in distinctTextLines)
                        {
                            writer.Write(line.Replace("\\n", "\n").Replace("\\r", "\r"));
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
                    if (Locres.Version == EngineVersion.Version4)
                    {
                        writer.WriteLine("v4");
                    }

                    foreach (var locSection in Locres.LocSections)
                    {
                        writer.WriteLine("[{0}]", locSection.Name);

                        if (!compact)
                        {
                            var entries = locSection.Entries;

                            if (sortById)
                            {
                                entries = entries.OrderBy(entry => entry.EntryId).ToList();
                            }

                            foreach (var textEntry in entries)
                            {
                                writer.WriteLine("{0:X8}\t{1}={2}", textEntry.Hash, textEntry.EntryId, textEntry.Entry.Replace("\n", "\\n").Replace("\r", "\\r"));
                            }
                        }
                        else
                        {
                            foreach (var grouping in locSection.Entries.GroupBy(entry => entry.Hash))
                            {
                                var lineIds = string.Join(",", grouping.Select(group => group.EntryId));
                                writer.WriteLine("{0:X8}\t{1}={2}", grouping.Key, lineIds, grouping.First().Entry.Replace("\n", "\\n").Replace("\r", "\\r"));
                            }
                        }
                    }
                }
            }
        }
    }
}
