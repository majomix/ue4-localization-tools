using NDesk.Options;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using UE4PakEditor.Model;

namespace UE4PakEditor.ViewModel
{
    public enum EditorAction
    {
        None,
        Create,
        Export,
        Import
    }

    internal class OneTimeRunViewModel : BaseViewModel
    {
        private string _targetDirectory;

        private string TargetPath { get; set; }
        public EditorAction Action { get; set; }
        public ICommand ExtractByParameterCommand { get; }
        public ICommand ImportByParameterCommand { get; }
        public ICommand CreateByParameterCommand { get; }

        public OneTimeRunViewModel()
        {
            ParseCommandLine();
            Model = new PakEditor();

            ImportByParameterCommand = new ImportByParameterCommand();
            ExtractByParameterCommand = new ExtractByParameterCommand();
            CreateByParameterCommand = new CreateByParameterCommand();
        }

        public void Extract()
        {
            if (_targetDirectory != null && LoadedFilePath != null)
            {
                LoadPakFile();
                ExtractWithPredicate(_targetDirectory, _ => true);
                //ExtractWithPredicate(_targetDirectory, _ => _.Name.Contains("Font") || _.Name.Contains("locres"));
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

        public void Create()
        {
            if (MountPoint != null && Directory.Exists(_targetDirectory) && TargetPath != null)
            {
                CreateNewPak(_targetDirectory, TargetPath); 
            }
        }

        public void ParseCommandLine()
        {
            OptionSet options = new OptionSet()
                .Add("create", value => Action = EditorAction.Create)
                .Add("export", value => Action = EditorAction.Export)
                .Add("import", value => Action = EditorAction.Import)
                .Add("target=", value => TargetPath = CreateFullPath(value, false))
                .Add("pak=", value => LoadedFilePath = CreateFullPath(value, true))
                .Add("mountpoint=", value => MountPoint = value)
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
