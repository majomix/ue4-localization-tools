using NDesk.Options;
using System;
using System.IO;
using UE4TextConverter.Model;

namespace UE4TextConverter.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        private string _locresFile;
        private string _textFile;
        private bool? _export;
        private bool _compact;
        private bool _sortById;

        public string LocresFile
        {
            get => _locresFile;
            set
            {
                if (_locresFile != value)
                {
                    _locresFile = value;
                    OnPropertyChanged(nameof(LocresFile));
                }
            }
        }
        public string TextFile
        {
            get => _textFile;
            set
            {
                if (_textFile != value)
                {
                    _textFile = value;
                    OnPropertyChanged(nameof(TextFile));
                }
            }
        }
        public TextConverter Converter { get; }

        public bool CloseWindow { get; private set; }
        
        public MainViewModel()
        {
            ParseCommandLine();
            CloseWindow = true;
            Converter = new TextConverter();

            if(_export != null)
            {
                if(_export == true)
                {
                    Converter.LoadLocresFile(LocresFile);
                    Converter.WriteTextFile(TextFile, _compact, _sortById);
                }
                else
                {
                    Converter.LoadTextFile(TextFile);
                    Converter.WriteLocresFile(LocresFile);
                }
            }
        }

        public void ParseCommandLine()
        {
            OptionSet options = new OptionSet()
                .Add("locres=", value => LocresFile = Path.GetFullPath(value))
                .Add("txt=", value => TextFile = Path.GetFullPath(value))
                .Add("export", value => _export = true)
                .Add("import", value => _export = false)
                .Add("compact", value => _compact = true)
                .Add("sort", value => _sortById = true)
                .Add("close", value => CloseWindow = true);

            options.Parse(Environment.GetCommandLineArgs());
        }

    }
}
