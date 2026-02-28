using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UE4TextConverter.Model;
using UE4UexpEditor.Psychonauts.Model;

namespace UE4UexpEditor.Psychonauts
{
    public class PsychonautsEditor : UexpEditor
    {
        private readonly string _inputDirectory;
        private readonly string _outputDirectory;
        private readonly string _rebuildDirectory;
        private readonly IPsychonautsDescriptorFactory _factory;

        public PsychonautsEditor(string inputDirectory, string outputDirectory, string rebuildDirectory, string version)
        {
            var strategies = new Dictionary<string, IPsychonautsDescriptorFactory>
            {
                { "1.0", new PsychonautsDescriptorFactory()}
            };

            _inputDirectory = inputDirectory;
            _outputDirectory = outputDirectory;
            _rebuildDirectory = rebuildDirectory;
            _factory = strategies[version];
        }

        public override void ExtractText()
        {
            var uexps = new List<Uexp>
            {
                _factory.CreateUexpSystemEn(_inputDirectory, _outputDirectory, _rebuildDirectory),
                _factory.CreateUexpSystemDe(_inputDirectory, _outputDirectory, _rebuildDirectory),
                _factory.CreateUexpSystemEs(_inputDirectory, _outputDirectory, _rebuildDirectory),
                _factory.CreateUexpMainEn(_inputDirectory, _outputDirectory, _rebuildDirectory),
                _factory.CreateUexpMainDe(_inputDirectory, _outputDirectory, _rebuildDirectory),
                _factory.CreateUexpMainEs(_inputDirectory, _outputDirectory, _rebuildDirectory),
                //_factory.CreateUexpMainSk(_inputDirectory, _outputDirectory, _rebuildDirectory)
            };

            PopulateUexps(uexps);

            foreach (var uexp in uexps)
            {
                var output = new List<string>();
                for (var i = 0; i < uexp.Lookup.Count; i++)
                {
                    var lookup = uexp.Lookup[i];

                    var id = uexp.Identifiers[i];
                    var value = uexp.Strings[lookup.First];

                    output.Add($"{id.Id}\t{value.Replace("\n", "\\n")}");
                }

                File.WriteAllLines(uexp.ExportPath, output.OrderBy(r => r));
            }
        }

        public override void CreateUexp()
        {
            var uexps = new List<Uexp>
            {
                _factory.CreateUexpSystemEn(_inputDirectory, _outputDirectory, _rebuildDirectory),
                _factory.CreateUexpMainEn(_inputDirectory, _outputDirectory, _rebuildDirectory),
            };

            PopulateUexps(uexps);

            foreach (var uexp in uexps)
            {
                var lines = File.ReadAllLines(uexp.ExportPath);

                var dict = new Dictionary<string, string>();
                foreach (var line in lines)
                {
                    var split = line.Split('\t');
                    dict[split[0]] = split[1].Replace("\\n", "\n");
                }

                long totalUexpLength;

                using (var outputStream = File.Open(uexp.RebuildPath, FileMode.Create))
                using (var writer = new LocBinaryWriter(outputStream, Encoding.Unicode))
                {
                    writer.Write(uexp.Metadata1);

                    var binaryStringsLengthOffset = writer.BaseStream.Position;

                    writer.Write(dict.Count);

                    foreach (var id in uexp.Identifiers)
                    {
                        writer.Write(dict[id.Id], EngineVersion.Version4);
                    }

                    var binaryStringsLength = writer.BaseStream.Position - binaryStringsLengthOffset;

                    writer.Write(uexp.Metadata2);
                    
                    writer.Write(dict.Count);

                    for (var i = 0; i < uexp.Lookup.Count; i++)
                    {
                        var lookup = uexp.Lookup[i];
                        writer.Write(i);
                        writer.Write(lookup.Second);
                        writer.Write(lookup.Third);
                        writer.Write(lookup.Fourth);
                        writer.Write(lookup.Fifth);
                    }

                    totalUexpLength = writer.BaseStream.Position;

                    writer.Write(2653586369);

                    writer.BaseStream.Seek(binaryStringsLengthOffset - 17, SeekOrigin.Begin);
                    writer.Write(binaryStringsLength);
                }

                var uasset = File.ReadAllBytes(uexp.Parent.InputPath);

                using (var memoryStream = new MemoryStream(uasset))
                using (var writer = new BinaryWriter(memoryStream))
                {
                    writer.Seek(uexp.Parent.Offset, uexp.Parent.SeekOrigin);
                    writer.Write(totalUexpLength);

                    File.WriteAllBytes(uexp.Parent.RebuildPath, memoryStream.ToArray());
                }
            }
        }

        private void PopulateUexps(List<Uexp> uexps)
        {
            foreach (var uexp in uexps)
            {
                using (var fileStream = File.Open(uexp.InputPath, FileMode.Open))
                using (var reader = new LocBinaryReader(fileStream))
                {
                    foreach (var descriptor in uexp.Descriptors)
                    {
                        reader.BaseStream.Seek(descriptor.StartOffset, SeekOrigin.Begin);

                        switch (descriptor.Type)
                        {
                            case SectionType.IdArray:
                                var idCount = reader.ReadInt32();
                                for (var i = 0; i < idCount; i++)
                                {
                                    var identifier = new Identifier
                                    {
                                        Id = Encoding.ASCII.GetString(reader.ReadBytes(13)),
                                        Number = reader.ReadInt32()
                                    };
                                    uexp.Identifiers.Add(identifier);
                                }

                                break;
                            case SectionType.StringArray:
                                var stringCount = reader.ReadInt32();
                                for (var i = 0; i < stringCount; i++)
                                {
                                    uexp.Strings.Add(reader.ReadString());
                                }

                                descriptor.EndOffset = (int)reader.BaseStream.Position;
                                break;
                            case SectionType.StringLookup:
                                var lookupCount = reader.ReadInt32();
                                for (var i = 0; i < lookupCount; i++)
                                {
                                    var lookup = new Lookup
                                    {
                                        First = reader.ReadInt32(),
                                        Second = reader.ReadInt32(),
                                        Third = reader.ReadInt32(),
                                        Fourth = reader.ReadInt32(),
                                        Fifth = reader.ReadByte()
                                    };
                                    uexp.Lookup.Add(lookup);

                                }

                                break;
                        }

                        reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        uexp.Metadata1 = reader.ReadBytes(uexp.Descriptors[1].StartOffset);

                        reader.BaseStream.Seek(uexp.Descriptors[1].EndOffset, SeekOrigin.Begin);
                        uexp.Metadata2 = reader.ReadBytes(uexp.Descriptors[2].StartOffset - uexp.Descriptors[1].EndOffset);
                    }
                }
            }
        }
    }
}
