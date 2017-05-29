﻿using System;
using System.IO;

namespace UE4PakEditor.Model
{
    internal class PakBinaryReader : BinaryReader
    {
        public PakBinaryReader(FileStream fileStream)
            : base(fileStream) { }

        public override string ReadString()
        {
            int length = ReadInt32();
            return new string(ReadChars(length)).TrimEnd('\0');
        }

        public PakFileStructure ReadPakInfo()
        {
            PakFileStructure pakFile = new PakFileStructure();

            pakFile.Signature = ReadInt32();
            pakFile.Version = ReadInt32();
            pakFile.DataSize = ReadInt64();
            pakFile.FileTreeSize = ReadInt64();
            pakFile.Hash = ReadBytes(20);

            return pakFile;
        }

        public PakDirectory ReadPakDirectory()
        {
            PakDirectory directory = new PakDirectory();

            directory.Name = ReadString();
            directory.NumberOfEntries = ReadInt32();

            return directory;
        }

        public PakEntry ReadDirectoryLevelPakEntry()
        {
            return ReadPakEntry(ReadString());
        }

        public PakEntry ReadFileLevelPakEntry()
        {
            return ReadPakEntry(String.Empty);
        }

        private PakEntry ReadPakEntry(string name)
        {
            PakEntry pakEntry = new PakEntry();

            pakEntry.Name = name;
            pakEntry.Offset = ReadInt64();
            pakEntry.CompressedSize = ReadInt64();
            pakEntry.UncompressedSize = ReadInt64();
            pakEntry.CompressionType = ReadInt32();
            pakEntry.Hash = ReadBytes(20);
            pakEntry.EncryptionType = ReadByte();
            pakEntry.ChunkSize = ReadInt32();

            return pakEntry;
        }
    }
}