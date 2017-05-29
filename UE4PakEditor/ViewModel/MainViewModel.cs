using System.Windows.Input;
using UE4PakEditor.Model;

namespace UE4PakEditor.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        public IFilePathProvider FilePathProvider { get; private set; }

        public ICommand OpenFileCommand { get; private set; }
        public ICommand ExtractAllCommand { get; private set; }
        public ICommand ExtractSelectedCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }
        public ICommand CloseApplicationCommand { get; private set; }
        public ICommand OpenAboutWindowCommand { get; private set; }

        public MainViewModel(IFilePathProvider filePathProvider)
        {
            FilePathProvider = filePathProvider;
            Model = new PakEditor();

            OpenFileCommand = new OpenFileCommand();
            ExtractAllCommand = new ExtractAllCommand();
            ExtractSelectedCommand = new ExtractSelectedCommand();
            ImportCommand = new ImportCommand();
            CloseApplicationCommand = new CloseApplicationCommand();
            OpenAboutWindowCommand = new OpenAboutWindowCommand();
        }
    }
}
