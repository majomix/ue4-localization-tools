using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UE4TextConverter.Model;

namespace UE4TextConverter.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        private string myLocresFile;
        private string myTextFile;
        private bool? myExport;

        public string LocresFile
        {
            get { return myLocresFile; }
            set
            {
                if(myLocresFile != value)
                {
                    myLocresFile = value;
                    OnPropertyChanged("LocresFile");
                }
            }
        }
        public string TextFile
        {
            get { return myTextFile; }
            set
            {
                if (myTextFile != value)
                {
                    myTextFile = value;
                    OnPropertyChanged("TextFile");
                }
            }
        }
        public TextConverter Converter { get; private set; }
        public List<TextEntry> SpecificList { get { return Converter.LocSections.First().Entries; } }
        public bool CloseWindow { get; private set; }
        
        public MainViewModel()
        {
            ParseCommandLine();
            Converter = new TextConverter();

            if(myExport != null)
            {
                if(myExport == true)
                {
                    Converter.LoadLocresFile(LocresFile);
                    Converter.WriteTextFile(TextFile);
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
                .Add("export", value => myExport = true)
                .Add("import", value => myExport = false)
                .Add("close", value => CloseWindow = true);

            options.Parse(Environment.GetCommandLineArgs());
        }

    }
}
