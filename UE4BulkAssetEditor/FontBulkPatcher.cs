using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace UE4BulkAssetEditor;

/// <summary>
/// Extracts and replaces zlib-compressed font data in UE4 uasset files via UAssetAPI.
/// Updates the FByteBulkData header (Extras) on the FontBulkData export.
/// </summary>
public class FontBulkPatcher
{
    private readonly EngineVersion _engineVersion;

    public FontBulkPatcher(EngineVersion engineVersion = EngineVersion.VER_UE4_13)
    {
        _engineVersion = engineVersion;
    }

    public byte[] ExtractFont(string assetPath)
    {
        var asset = new UAsset(assetPath, _engineVersion);
        var decompressor = new BulkDataDecompressor();
        return decompressor.Decompress(asset.BulkData);
    }

    public void ReplaceFont(string assetPath, byte[] newFont, string outputPath)
    {
        var asset = new UAsset(assetPath, _engineVersion);

        int origUncomp = BitConverter.ToInt32(asset.BulkData, 0x18);
        Console.WriteLine($"Original: uncompressed={origUncomp}, bulk={asset.BulkData.Length}");

        // Compress replacement font
        var compressor = new BulkDataCompressor();
        byte[] newBulkData = compressor.Compress(newFont);
        Console.WriteLine($"New: uncompressed={newFont.Length}, bulk={newBulkData.Length}");

        // Find the FontBulkData export and update its FByteBulkData header (Extras)
        var bulkExport = FindBulkDataExport(asset);
        byte[] extras = bulkExport.Extras;
        BitConverter.GetBytes(newFont.Length).CopyTo(extras, 4);           // uncompressed size
        BitConverter.GetBytes(newBulkData.Length - 4).CopyTo(extras, 8);   // size on disk (excluding sentinel)

        asset.BulkData = newBulkData;
        asset.Write(outputPath);
    }

    private static UAssetAPI.ExportTypes.Export FindBulkDataExport(UAsset asset)
    {
        foreach (var export in asset.Exports)
        {
            string classType = export.GetExportClassType().Value.Value;
            if (classType == "FontBulkData" && export.Extras is { Length: 20 })
                return export;
        }
        throw new InvalidDataException("Could not find FontBulkData export with 20-byte Extras");
    }
}
