using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UE4PakEditor.Model;

namespace UE4PakEditor.ViewModel
{
    internal abstract class BaseViewModel : INotifyPropertyChanged
    {
        private int myCurrentProgress;
        private string myLoadedFilePath;
        private string myCurrentFile;

        public bool Padding { get; set; }

        public PakEditor Model { get; protected set; }
        public string LoadedFilePath
        {
            get { return myLoadedFilePath; }
            set
            {
                if (myLoadedFilePath != value)
                {
                    myLoadedFilePath = value;
                    OnPropertyChanged("LoadedFilePath");
                }
            }
        }

        public string MountPoint { get; set; }

        public string CurrentFile
        {
            get { return myCurrentFile; }
            protected set
            {
                if (myCurrentFile != value)
                {
                    myCurrentFile = value;
                    OnPropertyChanged("CurrentFile");
                }
            }
        }
        public int CurrentProgress
        {
            get { return myCurrentProgress; }
            protected set
            {
                if (myCurrentProgress != value)
                {
                    myCurrentProgress = value;
                    OnPropertyChanged("CurrentProgress");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler RequestClose;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void OnRequestClose(EventArgs e)
        {
            RequestClose(this, e);
        }

        public void ExtractWithPredicate(string directory, Func<PakEntry, bool> function)
        {
            using (var fileStream = File.Open(LoadedFilePath, FileMode.Open))
            {
                var reader = new PakBinaryReader(fileStream);
                var pakEntryCollection = Model.Archive.Directory.Entries.Where(function).ToList();
                long currentSize = 0;
                long totalSize = pakEntryCollection.Sum(_ => _.UncompressedSize);

                foreach (var pakEntry in pakEntryCollection)
                {
                    if (pakEntry.Offset == -1)
                        continue;

                    if (reader.BaseStream.Position != pakEntry.Offset) reader.BaseStream.Seek(pakEntry.Offset, SeekOrigin.Begin);
                    Model.ExtractFile(directory, pakEntry, reader);
                    CurrentProgress = (int)(currentSize * 100.0 / totalSize);
                    CurrentFile = pakEntry.Name;
                    currentSize += pakEntry.UncompressedSize;
                }
            }
        }

        public void SavePakFile(string path)
        {
            using (var inputFileStream = File.Open(LoadedFilePath, FileMode.Open))
            {
                using (var reader = new PakBinaryReader(inputFileStream))
                {
                    using (var outputFileStream = File.Open(path, FileMode.Create))
                    {
                        using (var writer = new PakBinaryWriter(outputFileStream))
                        {
                            long currentSize = 0;
                            long totalSize = Model.Archive.Directory.Entries.Sum(_ => _.UncompressedSize);

                            for (var i = 0; i < Model.Archive.Directory.Entries.Count; i++)
                            {
                                var entry = Model.Archive.Directory.Entries[i];
                                
                                Model.SaveDataEntry(reader, writer, entry, Padding && i + 1 != Model.Archive.Directory.Entries.Count);
                                CurrentProgress = (int) (currentSize * 100.0 / totalSize);
                                CurrentFile = entry.Name;
                                currentSize += entry.UncompressedSize;
                            }

                            Model.SavePakFileStructure(writer);
                        }
                    }
                }
            }

            OnPropertyChanged("Model");
        }

        public void LoadPakFile()
        {
            using (FileStream fileStream = File.Open(LoadedFilePath, FileMode.Open))
            {
                PakBinaryReader reader = new PakBinaryReader(fileStream);
                Model.LoadPakFileStructure(reader);
                OnPropertyChanged("Model");
            }
        }

        public void ResolveNewFiles(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                string[] tokens = file.Split(new string[] { directory + @"\" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        string result = token.Replace(@"\", @"/");
                        var currentEntry = Model.Archive.Directory.Entries.SingleOrDefault(_ => _.Name == result);

                        if (currentEntry != null)
                        {
                            currentEntry.Import = file;
                            currentEntry.UncompressedSize = new FileInfo(file).Length;
                            currentEntry.CompressedSize = currentEntry.UncompressedSize;
                            currentEntry.Chunks.Clear();
                            currentEntry.CompressionType = 0;
                            currentEntry.EncryptionType = 0;
                        }
                    }
                }
            }
        }

        public void CreateNewPak(string directory, string targetPath)
        {
            var entries = new List<PakEntry>();

            foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                string[] tokens = file.Split(new string[] { directory + @"\" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        string result = token.Replace(@"\", @"/");
                        var currentEntry = new PakEntry
                        {
                            Name = result,
                            Import = file,
                            UncompressedSize = new FileInfo(file).Length,
                            CompressedSize = new FileInfo(file).Length,
                            CompressionType = 0,
                            EncryptionType = 0,
                        };

                        entries.Add(currentEntry);
                    }
                }
            }

            Model.CreateNewPakFile(MountPoint, entries);

            using (var outputFileStream = File.Open(targetPath, FileMode.Create))
            {
                using (var writer = new PakBinaryWriter(outputFileStream))
                {
                    long currentSize = 0;
                    long totalSize = Model.Archive.Directory.Entries.Sum(_ => _.UncompressedSize);

                    for (var i = 0; i < Model.Archive.Directory.Entries.Count; i++)
                    {
                        var entry = Model.Archive.Directory.Entries[i];

                        Model.CreateDataEntry(writer, entry, Padding && i + 1 != Model.Archive.Directory.Entries.Count);
                        CurrentProgress = (int)(currentSize * 100.0 / totalSize);
                        CurrentFile = entry.Name;
                        currentSize += entry.UncompressedSize;
                    }

                    Model.SavePakFileStructure(writer);
                }
            }
        }

        public string GenerateRandomName()
        {
            Random generator = new Random();
            return Path.ChangeExtension(LoadedFilePath, @".tmp_" + generator.Next().ToString());
        }

        public void SavePakFileWithRandomNameAndReplace()
        {
            string randomName = GenerateRandomName();
            SavePakFile(randomName);

            File.Delete(LoadedFilePath);
            File.Move(randomName, LoadedFilePath);
        }
    }
}
