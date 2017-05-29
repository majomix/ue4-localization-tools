namespace UE4PakEditor.ViewModel
{
    internal interface IFilePathProvider
    {
        string GetOpenFilePath();
        string GetSaveFilePath();
        string GetDirectoryPath();
    }
}
