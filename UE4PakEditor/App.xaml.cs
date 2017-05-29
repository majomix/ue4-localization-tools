using System;
using System.Windows;
using UE4PakEditor.View;

namespace UE4PakEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if(Environment.GetCommandLineArgs().Length > 1)
            {
                OneTimeRunWindow oneTimeRunWindow = new OneTimeRunWindow();
                MainWindow = oneTimeRunWindow;
                MainWindow.Show();
            }
            else
            {
                MainEditorWindow mainEditorWindow = new MainEditorWindow();
                MainWindow = mainEditorWindow;
                MainWindow.Show();
            }
        }
    }
}
