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
            using (FileStream fileStream = File.Open(LoadedFilePath, FileMode.Open))
            {
                PakBinaryReader reader = new PakBinaryReader(fileStream);
                IEnumerable<PakEntry> pakEntryCollection = Model.Archive.Directory.Entries.Where(function);
                long currentSize = 0;
                long totalSize = pakEntryCollection.Sum(_ => _.UncompressedSize);

                foreach (PakEntry pakEntry in pakEntryCollection)
                {
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
            using (FileStream inputFileStream = File.Open(LoadedFilePath, FileMode.Open))
            {
                PakBinaryReader reader = new PakBinaryReader(inputFileStream);

                using (FileStream outputFileStream = File.Open(path, FileMode.Create))
                {
                    using (PakBinaryWriter writer = new PakBinaryWriter(outputFileStream))
                    {
                        long currentSize = 0;
                        long totalSize = Model.Archive.Directory.Entries.Sum(_ => _.UncompressedSize);

                        foreach (PakEntry entry in Model.Archive.Directory.Entries)
                        {
                            Model.SaveDataEntry(reader, writer, entry);
                            CurrentProgress = (int)(currentSize * 100.0 / totalSize);
                            CurrentFile = entry.Name;
                            currentSize += entry.UncompressedSize;
                        }

                        Model.SavePakFileStructure(writer);
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
                    if (!String.IsNullOrWhiteSpace(token))
                    {
                        string result = token.Replace(@"\", @"/");
                        PakEntry currentEntry = Model.Archive.Directory.Entries.SingleOrDefault(_ => _.Name == result);

                        if (currentEntry != null)
                        {
                            currentEntry.Import = file;
                            currentEntry.UncompressedSize = new FileInfo(file).Length;
                            currentEntry.CompressedSize = currentEntry.UncompressedSize;
                        }
                    }
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
