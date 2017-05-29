using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UE4PakEditor.Model
{
    internal class PakEditor
    {
        public PakFileStructure Archive { get; private set; }
        
        public void LoadPakFileStructure(PakBinaryReader reader)
        {
            reader.BaseStream.Seek(-44, SeekOrigin.End);
            Archive = reader.ReadPakInfo();

            reader.BaseStream.Seek(Archive.DataSize, SeekOrigin.Begin);
            Archive.Directory = reader.ReadPakDirectory();

            for (int i = 0; i < Archive.Directory.NumberOfEntries; i++)
            {
                Archive.Directory.Entries.Add(reader.ReadDirectoryLevelPakEntry());
            }
        }

        public void ExtractFile(string directory, PakEntry pakEntry, PakBinaryReader reader)
        {
            PakEntry currentPakEntry = reader.ReadFileLevelPakEntry();
            if (!currentPakEntry.Equals(pakEntry)) return;

            string compoundName = directory + @"\" + pakEntry.Name.Replace(@"/", @"\");
            if(!compoundName.Contains(@"\")) return;

            Directory.CreateDirectory(Path.GetDirectoryName(compoundName));

            using (FileStream fileStream = File.Open(compoundName, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    writer.Write(reader.ReadBytes((int)currentPakEntry.UncompressedSize));
                }
            }
        }

        public void SaveDataEntry(PakBinaryReader reader, PakBinaryWriter writer, PakEntry pakEntry)
        {
            long originalOffset = pakEntry.Offset;
            pakEntry.Offset = writer.BaseStream.Position;
            writer.Write(pakEntry, true);

            if (pakEntry.Import != null)
            {
                using (FileStream importFileStream = File.Open(pakEntry.Import, FileMode.Open))
                {
                    PakBinaryReader importReader = new PakBinaryReader(importFileStream);
                    writer.Write(importReader.ReadBytes((int)pakEntry.UncompressedSize));
                }
            }
            else
            {
                if (reader.BaseStream.Position != originalOffset) reader.BaseStream.Seek(originalOffset, SeekOrigin.Begin);
                PakEntry importFilePakEntry = reader.ReadFileLevelPakEntry();
                if (!importFilePakEntry.Equals(pakEntry)) return;
                writer.Write(reader.ReadBytes((int)pakEntry.UncompressedSize));
            }

            
        }

        public void SavePakFileStructure(PakBinaryWriter writer)
        {
            byte[] streamBytes;
            Archive.DataSize = writer.BaseStream.Position;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (PakBinaryWriter memoryWriter = new PakBinaryWriter(memoryStream))
                {
                    memoryWriter.Write(Archive.Directory);

                    foreach (PakEntry entry in Archive.Directory.Entries)
                    {
                        memoryWriter.Write(entry, false);
                    }
                }

                streamBytes = memoryStream.ToArray();
            }

            writer.Write(streamBytes);
            Archive.Hash = HashBytes(streamBytes);
            Archive.FileTreeSize = streamBytes.Length;

            writer.Write(Archive);
        }

        public byte[] HashBytes(byte[] input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(input);
            }
        }
    }
}
