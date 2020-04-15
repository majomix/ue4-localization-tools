using NDesk.Options;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using UE4PakEditor.Model;

namespace UE4PakEditor.ViewModel
{
    internal class OneTimeRunViewModel : BaseViewModel
    {
        private string _targetDirectory;

        private string TargetPath { get; set; }
        public bool? Export { get; set; }
        public ICommand ExtractByParameterCommand { get; }
        public ICommand ImportByParameterCommand { get; }

        public OneTimeRunViewModel()
        {
            ParseCommandLine();
            Model = new PakEditor();

            ImportByParameterCommand = new ImportByParameterCommand();
            ExtractByParameterCommand = new ExtractByParameterCommand();
        }

        public void Extract()
        {
            if (_targetDirectory != null && LoadedFilePath != null)
            {
                LoadPakFile();
                ExtractWithPredicate(_targetDirectory, _ => true);
            }
        }

        public void Import()
        {
            if (_targetDirectory != null && Directory.Exists(_targetDirectory) && LoadedFilePath != null)
            {
                LoadPakFile();
                ResolveNewFiles(_targetDirectory);

                if (TargetPath != null) SavePakFile(TargetPath);
                else SavePakFileWithRandomNameAndReplace();
            }
        }

        public void ParseCommandLine()
        {
            OptionSet options = new OptionSet()
                .Add("export", value => Export = true)
                .Add("import", value => Export = false)
                .Add("target=", value => TargetPath = CreateFullPath(value, false))
                .Add("pak=", value => LoadedFilePath = CreateFullPath(value, true))
                .Add("padding", value => Padding = true)
                .Add("dir=", value => _targetDirectory = CreateFullPath(value, false));

            options.Parse(Environment.GetCommandLineArgs());
        }

        private string CreateFullPath(string path, bool checkForFileExistence)
        {
            if (String.IsNullOrEmpty(path)) return null;

            if (path.Contains(':') && CheckForExistence(path, checkForFileExistence))
            {
                return path;
            }
            else
            {
                string resultPath = Directory.GetCurrentDirectory() + @"\" + path.Replace('/', '\\');
                if (CheckForExistence(resultPath, checkForFileExistence)) return resultPath;
                else return null;
            }
        }

        private bool CheckForExistence(string path, bool checkForFile)
        {
            if(checkForFile == true)
            {
                return File.Exists(path);
            }
            return true;
        }
    }
}
