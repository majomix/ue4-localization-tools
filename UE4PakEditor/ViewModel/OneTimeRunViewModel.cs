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
        private string myTargetDirectory;
        private string myTargetPath { get; set; }

        public bool? Export { get; set; }
        public ICommand ExtractByParameterCommand { get; private set; }
        public ICommand ImportByParameterCommand { get; private set; }

        public OneTimeRunViewModel()
        {
            ParseCommandLine();
            Model = new PakEditor();

            ImportByParameterCommand = new ImportByParameterCommand();
            ExtractByParameterCommand = new ExtractByParameterCommand();
        }

        public void Extract()
        {
            if(myTargetDirectory != null && LoadedFilePath != null)
            {
                LoadPakFile();
                ExtractWithPredicate(myTargetDirectory, _ => true);
            }
        }

        public void Import()
        {
            if (myTargetDirectory != null && Directory.Exists(myTargetDirectory) && LoadedFilePath != null)
            {
                LoadPakFile();
                ResolveNewFiles(myTargetDirectory);

                if (myTargetPath != null) SavePakFile(myTargetPath);
                else SavePakFileWithRandomNameAndReplace();
            }
        }

        public void ParseCommandLine()
        {
            OptionSet options = new OptionSet()
                .Add("export", value => Export = true)
                .Add("import", value => Export = false)
                .Add("target=", value => myTargetPath = CreateFullPath(value, false))
                .Add("pak=", value => LoadedFilePath = CreateFullPath(value, true))
                .Add("dir=", value => myTargetDirectory = CreateFullPath(value, false));

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
