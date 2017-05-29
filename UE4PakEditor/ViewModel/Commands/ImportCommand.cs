using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UE4PakEditor.Model;

namespace UE4PakEditor.ViewModel
{
    internal class ImportCommand : AbstractWorkerCommand
    {
        private MainViewModel myMainViewModel;

        public override void Execute(object parameter)
        {
            object[] parameters = (object[])parameter;
            bool askForOutputName = (bool)parameters[1];
            myMainViewModel = (MainViewModel)parameters[0];

            string directory = myMainViewModel.FilePathProvider.GetDirectoryPath();
            string outputPath = askForOutputName ? myMainViewModel.FilePathProvider.GetSaveFilePath() : null;

            myMainViewModel.ResolveNewFiles(directory);
            Worker.RunWorkerAsync(outputPath);
        }

        protected override void DoWork(object sender, DoWorkEventArgs e)
        {
            string file = e.Argument as string;
            if (!String.IsNullOrEmpty(file))
            {
                myMainViewModel.SavePakFile(file);
            }
            else
            {
                myMainViewModel.SavePakFileWithRandomNameAndReplace();
            }
        }
    }
}
