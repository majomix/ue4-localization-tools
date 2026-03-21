namespace UE4BulkAssetEditor;

public record FontEntry(string Name, byte[] Data);

public interface IFontPatcher
{
    List<FontEntry> ExtractFonts(string assetPath);
    void ReplaceFonts(string assetPath, List<byte[]> newFonts, string outputPath);
}
