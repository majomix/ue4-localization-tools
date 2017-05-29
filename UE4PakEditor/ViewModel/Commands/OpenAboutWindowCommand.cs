using System;
using System.Windows;
using System.Windows.Input;
using UE4PakEditor.View;

namespace UE4PakEditor.ViewModel
{
    internal class OpenAboutWindowCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            AboutWindow aboutWindow = new AboutWindow() { Owner = Application.Current.MainWindow };
            aboutWindow.ShowDialog();
        }
    }
}
