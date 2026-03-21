using UAssetAPI;
using UAssetAPI.UnrealTypes;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;

namespace UE4BulkAssetEditor;

/// <summary>
/// Font patcher for standard UE4 assets via UAssetAPI.
/// Works when bulk data is appended at the end of the file (single font per asset).
/// </summary>
public class UAssetFontPatcher : IFontPatcher
{
    private readonly EngineVersion _engineVersion;

    public UAssetFontPatcher(EngineVersion engineVersion)
    {
        _engineVersion = engineVersion;
    }

    public List<FontEntry> ExtractFonts(string assetPath)
    {
        var asset = LoadAsset(assetPath);
        byte[] font = BulkDataCodec.Decompress(asset.BulkData);
        string name = FindFontFilename(asset) ?? Path.GetFileNameWithoutExtension(assetPath) + ".ttf";
        return [new FontEntry(name, font)];
    }

    public void ReplaceFonts(string assetPath, List<byte[]> newFonts, string outputPath)
    {
        if (newFonts.Count != 1)
            throw new ArgumentException("UAssetAPI mode supports exactly 1 font per asset");

        var asset = LoadAsset(assetPath);
        byte[] newBulkData = BulkDataCodec.Compress(newFonts[0]);

        var bulkExport = FindBulkDataExport(asset);
        byte[] extras = bulkExport.Extras;
        BitConverter.GetBytes(newFonts[0].Length).CopyTo(extras, 4);       // uncompressed size
        BitConverter.GetBytes(newBulkData.Length - 4).CopyTo(extras, 8);   // size on disk (excl sentinel)

        asset.BulkData = newBulkData;
        asset.Write(outputPath);

        Console.WriteLine($"  Original bulk: {asset.BulkData.Length}, New bulk: {newBulkData.Length}");
    }

    private UAsset LoadAsset(string path)
    {
        var asset = new UAsset(path, _engineVersion);
        if (asset.BulkData == null || asset.BulkData.Length == 0)
            throw new InvalidDataException("No bulk data found. Try --gears mode for custom engine layouts.");
        return asset;
    }

    private static Export FindBulkDataExport(UAsset asset)
    {
        return asset.Exports.FirstOrDefault(e =>
                   e.GetExportClassType().Value.Value == "FontBulkData" &&
                   e.Extras is { Length: 20 })
               ?? throw new InvalidDataException("FontBulkData export not found");
    }

    private static string? FindFontFilename(UAsset asset)
    {
        foreach (var export in asset.Exports.OfType<NormalExport>())
        foreach (var prop in export.Data)
            if (FindFilenameInProperty(prop) is { } name)
                return name;
        return null;
    }

    private static string? FindFilenameInProperty(PropertyData prop)
    {
        if (prop.Name.ToString() == "FontFilename" && prop.RawValue is string s && s.Length > 0)
            return Path.GetFileName(s);

        if (prop is StructPropertyData sp)
            foreach (var child in sp.Value)
                if (FindFilenameInProperty(child) is { } name)
                    return name;

        if (prop is ArrayPropertyData ap && ap.Value != null)
            foreach (var item in ap.Value)
                if (FindFilenameInProperty(item) is { } name)
                    return name;

        return null;
    }
}
