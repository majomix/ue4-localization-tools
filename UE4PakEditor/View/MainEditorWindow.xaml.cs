using System;
using System.Windows;
using UE4PakEditor.ViewModel;

namespace UE4PakEditor.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainEditorWindow : Window
    {
        public MainEditorWindow()
        {
            InitializeComponent();
            MainViewModel mainViewModel = new MainViewModel(new FilePathProvider());
            mainViewModel.RequestClose += (s, e) => this.Dispatcher.Invoke(new Action(() => Close())); // violates MVVM
            DataContext = mainViewModel;
        }
    }
}
