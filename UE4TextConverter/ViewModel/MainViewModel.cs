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
        private bool _batchExport;
        private bool _batchImport;
        private string _batchOperationDir;
        private string _batchOperationLang = "en";

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

            if (_batchExport)
            {
                if (string.IsNullOrEmpty(_batchOperationDir))
                {
                    Console.Error.WriteLine("Error: --dir parameter is required for batch export.");
                    return;
                }
                if (string.IsNullOrEmpty(TextFile))
                {
                    Console.Error.WriteLine("Error: --txt parameter is required for batch export.");
                    return;
                }
                new BatchConverter(_compact, _sortById).BatchExport(_batchOperationDir, TextFile, _batchOperationLang);
                return;
            }

            if (_batchImport)
            {
                if (string.IsNullOrEmpty(TextFile))
                {
                    Console.Error.WriteLine("Error: --txt parameter is required for batch import.");
                    return;
                }
                if (string.IsNullOrEmpty(_batchOperationDir))
                {
                    Console.Error.WriteLine("Error: --dir parameter is required for batch import.");
                    return;
                }
                new BatchConverter(_compact, _sortById).BatchImport(TextFile, _batchOperationDir);
                return;
            }

            if (_export != null)
            {
                if (_export == true)
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
                .Add("close", value => CloseWindow = true)
                .Add("batch-export", value => _batchExport = true)
                .Add("batch-import", value => _batchImport = true)
                .Add("dir=", value => _batchOperationDir = Path.GetFullPath(value))
                .Add("lang=", value => _batchOperationLang = value);

            options.Parse(Environment.GetCommandLineArgs());
        }

    }
}
