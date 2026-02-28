 using System.IO;
 using UE4UexpEditor.Psychonauts.Model;

namespace UE4UexpEditor.Psychonauts
{
    public class PsychonautsDescriptorFactory : IPsychonautsDescriptorFactory
    {
        public Uexp CreateUexpMainDe(string inputDirectory, string outputDirectory, string rebuildDirectory)
        {
            var uexp = new Uexp();

            uexp.InputPath = $@"{inputDirectory}\MainGame_deDE.uexp";
            uexp.ExportPath = $@"{outputDirectory}\Main_de.txt";
            uexp.RebuildPath = $@"{rebuildDirectory}\MainGame_deDE.uexp";

            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 124,
                Type = SectionType.IdArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 319013,
                Type = SectionType.StringArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 1817870,
                Type = SectionType.StringLookup
            });

            return uexp;
        }

        public Uexp CreateUexpMainEn(string inputDirectory, string outputDirectory, string rebuildDirectory)
        {
            var uexp = new Uexp();

            uexp.InputPath = $@"{inputDirectory}\MainGame_enUS.uexp";
            uexp.ExportPath = $@"{outputDirectory}\Main_en.txt";
            uexp.RebuildPath = $@"{rebuildDirectory}\MainGame_enUS.uexp";

            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 83,
                Type = SectionType.IdArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 318972,
                Type = SectionType.StringArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 1272438,
                Type = SectionType.StringLookup
            });

            uexp.Parent = new Uasset
            {
                InputPath = $@"{inputDirectory}\MainGame_enUS.uasset",
                RebuildPath = $@"{rebuildDirectory}\MainGame_enUS.uasset",
                Offset = -92,
                SeekOrigin = SeekOrigin.End
            };

            return uexp;
        }

        public Uexp CreateUexpMainEs(string inputDirectory, string outputDirectory, string rebuildDirectory)
        {
            var uexp = new Uexp();

            uexp.InputPath = $@"{inputDirectory}\MainGame_esES.uexp";
            uexp.ExportPath = $@"{outputDirectory}\Main_es.txt";
            uexp.RebuildPath = $@"{rebuildDirectory}\MainGame_esES.uexp";

            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 124,
                Type = SectionType.IdArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 319013,
                Type = SectionType.StringArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 1793526,
                Type = SectionType.StringLookup
            });

            return uexp;
        }

        public Uexp CreateUexpMainSk(string inputDirectory, string outputDirectory, string rebuildDirectory)
        {
            var uexp = new Uexp();

            uexp.InputPath = $@"{inputDirectory}\Main_en.txt.bin";
            uexp.ExportPath = $@"{outputDirectory}\Main_sk.txt";

            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 83,
                Type = SectionType.IdArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 318972,
                Type = SectionType.StringArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 1949884,
                Type = SectionType.StringLookup
            });

            return uexp;
        }

        public Uexp CreateUexpSystemEn(string inputDirectory, string outputDirectory, string rebuildDirectory)
        {
            var uexp = new Uexp();

            uexp.InputPath = $@"{inputDirectory}\System_enUS.uexp";
            uexp.ExportPath = $@"{outputDirectory}\System_en.txt";
            uexp.RebuildPath = $@"{rebuildDirectory}\System_enUS.uexp";

            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 81,
                Type = SectionType.IdArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 3467,
                Type = SectionType.StringArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 11497,
                Type = SectionType.StringLookup
            });

            uexp.Parent = new Uasset
            {
                InputPath = $@"{inputDirectory}\System_enUS.uasset",
                RebuildPath = $@"{rebuildDirectory}\System_enUS.uasset",
                Offset = -92,
                SeekOrigin = SeekOrigin.End
            };

            return uexp;
        }

        public Uexp CreateUexpSystemDe(string inputDirectory, string outputDirectory, string rebuildDirectory)
        {
            var uexp = new Uexp();

            uexp.InputPath = $@"{inputDirectory}\System_deDE.uexp";
            uexp.ExportPath = $@"{outputDirectory}\System_de.txt";
            uexp.RebuildPath = $@"{rebuildDirectory}\System_deDE.uexp";

            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 122,
                Type = SectionType.IdArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 3508,
                Type = SectionType.StringArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 16748,
                Type = SectionType.StringLookup
            });

            return uexp;
        }

        public Uexp CreateUexpSystemEs(string inputDirectory, string outputDirectory, string rebuildDirectory)
        {
            var uexp = new Uexp();

            uexp.InputPath = $@"{inputDirectory}\System_esES.uexp";
            uexp.ExportPath = $@"{outputDirectory}\System_es.txt";
            uexp.RebuildPath = $@"{rebuildDirectory}\System_esES.uexp";

            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 122,
                Type = SectionType.IdArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 3508,
                Type = SectionType.StringArray
            });
            uexp.Descriptors.Add(new UexpDescriptor
            {
                StartOffset = 17068,
                Type = SectionType.StringLookup
            });

            return uexp;
        }
    }
}
