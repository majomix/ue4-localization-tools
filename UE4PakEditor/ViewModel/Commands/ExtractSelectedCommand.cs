using System.ComponentModel;

namespace UE4PakEditor.ViewModel
{
    internal class ExtractSelectedCommand : AbstractWorkerCommand
    {
        private MainViewModel myMainViewModel;

        public override void Execute(object parameter)
        {
            myMainViewModel = (MainViewModel)parameter;
            string outputDirectory = myMainViewModel.FilePathProvider.GetDirectoryPath();
            Worker.RunWorkerAsync(outputDirectory);
        }

        protected override void DoWork(object sender, DoWorkEventArgs e)
        {
            string directory = e.Argument as string;
            if (directory != null) myMainViewModel.ExtractWithPredicate(directory, _ => _.Extract == true);
        }
    }
}
