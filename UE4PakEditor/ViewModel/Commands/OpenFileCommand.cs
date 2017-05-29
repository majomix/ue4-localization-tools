using System;
using System.Windows.Input;

namespace UE4PakEditor.ViewModel
{
    internal class OpenFileCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return parameter is MainViewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            MainViewModel mainViewModel = (MainViewModel)parameter;
            string pakFilePath = mainViewModel.FilePathProvider.GetOpenFilePath();
            if (!String.IsNullOrWhiteSpace(pakFilePath))
            {
                mainViewModel.LoadedFilePath = pakFilePath;
                mainViewModel.LoadPakFile();
            }
        }
    }
}
