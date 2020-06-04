using System;
using System.Windows;
using UE4PakEditor.ViewModel;

namespace UE4PakEditor.View
{
    /// <summary>
    /// Interaction logic for OneTimeRunWindow.xaml
    /// </summary>
    public partial class OneTimeRunWindow : Window
    {
        public OneTimeRunWindow()
        {
            InitializeComponent();
            OneTimeRunViewModel oneTimeRunViewModel = new OneTimeRunViewModel();
            oneTimeRunViewModel.RequestClose += (s, e) => this.Dispatcher.Invoke(new Action(() => Close())); // violates MVVM
            DataContext = oneTimeRunViewModel;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            OneTimeRunViewModel oneTimeRunViewModel = DataContext as OneTimeRunViewModel;
            if (oneTimeRunViewModel != null)
            {
                switch (oneTimeRunViewModel.Action)
                {
                    case EditorAction.None:
                        Close();
                        return;
                    case EditorAction.Export:
                        oneTimeRunViewModel.ExtractByParameterCommand.Execute(oneTimeRunViewModel);
                        break;
                    case EditorAction.Import:
                        oneTimeRunViewModel.ImportByParameterCommand.Execute(oneTimeRunViewModel);
                        break;
                    case EditorAction.Create:
                        oneTimeRunViewModel.CreateByParameterCommand.Execute(oneTimeRunViewModel);
                        break;
                }
            }
        }
    }
}
