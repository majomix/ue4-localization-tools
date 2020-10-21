using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using Ionic.Zlib;
using UE4PakEditor.Model.Compression;
using UE4PakEditor.Model.Encryption;

namespace UE4PakEditor.Model
{
    internal class PakEditor
    {
        public PakFileStructure Archive { get; private set; }
        
        public void LoadPakFileStructure(PakBinaryReader reader, int version, string aesKey)
        {
            var startOffset = version >= 8 ? -204 : -44;

            reader.BaseStream.Seek(startOffset, SeekOrigin.End);
            Archive = reader.ReadPakInfo();

            reader.BaseStream.Seek(Archive.DataSize, SeekOrigin.Begin);

            var readerToUse = reader;

            if (aesKey != null)
            {
                var soapHex = SoapHexBinary.Parse(aesKey);

                var fileTree = reader.ReadBytes((int)Archive.FileTreeSize);
                var decryptedFileTree = AesHandler.DecryptAes(fileTree, soapHex.Value);

                var memoryStream = new MemoryStream(decryptedFileTree);
                var memoryReader = new PakBinaryReader(memoryStream);

                readerToUse = memoryReader;
            }

            Archive.Directory = readerToUse.ReadPakDirectory();

            for (int i = 0; i < Archive.Directory.NumberOfEntries; i++)
            {
                var currentEntry = readerToUse.ReadDirectoryLevelPakEntry();
                Archive.Directory.Entries.Add(currentEntry);

                if (i > 0)
                {
                    var previousEntry = Archive.Directory.Entries[i - 1];
                    var realEntrySize = currentEntry.Offset - previousEntry.Offset;
                    previousEntry.Padded = currentEntry.Offset % 2048 == 0;
                    previousEntry.NextOffset = currentEntry.Offset;
                    previousEntry.RealSize = realEntrySize;
                }
            }

            if (aesKey != null)
            {
                readerToUse.Close();
            }

            var compressionTypes  = Archive.Directory.Entries.Select(entry => entry.CompressionType).Distinct();
            var encryptionTyoes = Archive.Directory.Entries.Select(entry => entry.EncryptionType).Distinct();
            var paddedUncompressed = Archive.Directory.Entries.Where(entry => entry.CompressionType == 0 && entry.Padded).OrderBy(entry => entry.UncompressedSize);
            var unpaddedUncompressed = Archive.Directory.Entries.Where(entry => entry.CompressionType == 0 && entry.Padded == false).OrderBy(entry => entry.UncompressedSize);
        }

        public void ExtractFile(string directory, PakEntry pakEntry, PakBinaryReader reader)
        {
            var currentPakEntry = reader.ReadFileLevelPakEntry();
            if (!currentPakEntry.Equals(pakEntry)) return;

            string compoundName = directory + @"\" + pakEntry.Name.Replace(@"/", @"\");
            if (!compoundName.Contains(@"\")) return;

            Directory.CreateDirectory(Path.GetDirectoryName(compoundName));

            using (var fileStream = File.Open(compoundName, FileMode.Create))
            {
                using (var writer = new BinaryWriter(fileStream))
                {
                    switch (pakEntry.CompressionType)
                    {
                        case 0:
                            writer.Write(reader.ReadBytes((int)currentPakEntry.UncompressedSize));
                            break;
                        case 1:
                            foreach (var chunk in pakEntry.Chunks)
                            {
                                var compressedData = reader.ReadBytes((int)(chunk.ChunkEnd - chunk.ChunkOffset));
                                var decompressed = ZlibStream.UncompressBuffer(compressedData);
                                writer.Write(decompressed);
                            }
                            break;
                        case 4:
                        case 16400:
                            var remainingSizeToUncompress = pakEntry.UncompressedSize;
                            foreach (var chunk in pakEntry.Chunks)
                            {
                                var decompressedSize = remainingSizeToUncompress > pakEntry.ChunkSize
                                    ? pakEntry.ChunkSize
                                    : remainingSizeToUncompress;
                                var compressedData = reader.ReadBytes((int)(chunk.ChunkEnd - chunk.ChunkOffset));
                                var decompressed = OodleHandler.Decompress(compressedData, decompressedSize);
                                writer.Write(decompressed);
                                remainingSizeToUncompress -= pakEntry.ChunkSize;
                            }
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }
        }

        public void SaveDataEntry(PakBinaryReader reader, PakBinaryWriter writer, PakEntry pakEntry, bool usePadding)
        {
            if (pakEntry.Offset == -1)
                return;

            long originalOffset = pakEntry.Offset;
            pakEntry.Offset = writer.BaseStream.Position;

            if (Archive.Version < 7)
            {
                var offsetDifference = pakEntry.Offset - originalOffset;
                foreach (var chunk in pakEntry.Chunks)
                { 
                    chunk.ChunkEnd += offsetDifference;
                    chunk.ChunkOffset += offsetDifference;
                }
            }

            writer.Write(pakEntry, true);

            if (pakEntry.Import != null)
            {
                using (var importFileStream = File.Open(pakEntry.Import, FileMode.Open))
                {
                    var importReader = new PakBinaryReader(importFileStream);
                    writer.Write(importReader.ReadBytes((int)pakEntry.UncompressedSize));
                }
            }
            else
            {
                if (reader.BaseStream.Position != originalOffset)
                {
                    reader.BaseStream.Seek(originalOffset, SeekOrigin.Begin);
                }

                var importFilePakEntry = reader.ReadFileLevelPakEntry();
                if (!importFilePakEntry.Equals(pakEntry)) return;

                switch (pakEntry.CompressionType)
                {
                    case 0:
                        writer.Write(reader.ReadBytes((int)pakEntry.UncompressedSize));
                        break;
                    case 1:
                    case 4:
                    case 16400:
                        foreach (var chunk in pakEntry.Chunks)
                        {
                            var compressedData = reader.ReadBytes((int)(chunk.ChunkEnd - chunk.ChunkOffset));
                            writer.Write(compressedData);
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            if (usePadding && pakEntry.Padded)
            {
                var toWrite = 2048 - writer.BaseStream.Position % 2048;
                if (toWrite != 2048)
                {
                    writer.Write(new byte[toWrite]);
                }
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

            if (Archive.Version >= 7)
            {
                writer.Write(new byte[17]);
            }

            writer.Write(Archive);

            if (Archive.Version == 8)
            {
                writer.Write(new byte[160]);
            }
        }

        public byte[] HashBytes(byte[] input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(input);
            }
        }

        public void CreateNewPakFile(string mountPoint, List<PakEntry> entries, int version)
        {
            Archive = new PakFileStructure
            {
                Signature = 1517228769,
                Version = version,
                Directory = new PakDirectory
                {
                    Name = mountPoint,
                    Entries = entries,
                    NumberOfEntries = entries.Count
                }
            };
        }

        public void CreateDataEntry(PakBinaryWriter writer, PakEntry pakEntry, bool usePadding)
        {
            pakEntry.Offset = writer.BaseStream.Position;

            var bytes = File.ReadAllBytes(pakEntry.Import);
            pakEntry.Hash = HashBytes(bytes);
            writer.Write(pakEntry, true);
            writer.Write(bytes);

            if (usePadding && pakEntry.Padded)
            {
                var toWrite = 2048 - writer.BaseStream.Position % 2048;
                if (toWrite != 2048)
                {
                    writer.Write(new byte[toWrite]);
                }
            }
        }
    }
}
