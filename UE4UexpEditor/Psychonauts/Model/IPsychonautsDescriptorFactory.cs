namespace UE4UexpEditor.Psychonauts.Model
{
    public interface IPsychonautsDescriptorFactory
    {
        Uexp CreateUexpMainDe(string inputDirectory, string outputDirectory, string rebuildDirectory);
        Uexp CreateUexpMainEn(string inputDirectory, string outputDirectory, string rebuildDirectory);
        Uexp CreateUexpMainEs(string inputDirectory, string outputDirectory, string rebuildDirectory);
        Uexp CreateUexpMainSk(string inputDirectory, string outputDirectory, string rebuildDirectory);
        Uexp CreateUexpSystemDe(string inputDirectory, string outputDirectory, string rebuildDirectory);
        Uexp CreateUexpSystemEn(string inputDirectory, string outputDirectory, string rebuildDirectory);
        Uexp CreateUexpSystemEs(string inputDirectory, string outputDirectory, string rebuildDirectory);
    }
}
