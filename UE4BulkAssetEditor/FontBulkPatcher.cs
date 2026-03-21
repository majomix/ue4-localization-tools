using System.Text;

namespace UE4BulkAssetEditor;

/// <summary>
/// Font patcher for custom UE4 engines (e.g. Gears 5) using raw binary patching.
/// Supports multiple fonts per asset with interleaved bulk data.
/// </summary>
public class FontBulkPatcher : IFontPatcher
{
    private const uint PACKAGE_FILE_TAG = 0x9E2A83C1;
    private const int EXTRAS_SIZE = 20;

    public List<FontEntry> ExtractFonts(string assetPath)
    {
        byte[] fileData = File.ReadAllBytes(assetPath);
        var offsets = FindAllBulkDataTags(fileData);
        var names = FindFontFilenames(fileData);
        var fonts = new List<FontEntry>();

        for (int i = 0; i < offsets.Count; i++)
        {
            int sizeOnDisk = BitConverter.ToInt32(fileData, offsets[i] - EXTRAS_SIZE + 8);
            byte[] bulkData = new byte[sizeOnDisk];
            Array.Copy(fileData, offsets[i], bulkData, 0, sizeOnDisk);

            string name = i < names.Count ? names[i] : $"font_{i}.ttf";
            fonts.Add(new FontEntry(name, BulkDataCodec.Decompress(bulkData)));
        }

        return fonts;
    }

    public void ReplaceFonts(string assetPath, List<byte[]> newFonts, string outputPath)
    {
        byte[] original = File.ReadAllBytes(assetPath);
        var offsets = FindAllBulkDataTags(original);

        if (newFonts.Count != offsets.Count)
            throw new ArgumentException($"Expected {offsets.Count} fonts, got {newFonts.Count}");

        var compressed = newFonts.Select(BulkDataCodec.Compress).ToList();

        using var output = new MemoryStream();
        int srcPos = 0;

        for (int i = 0; i < offsets.Count; i++)
        {
            int bulkStart = offsets[i];
            int extrasStart = bulkStart - EXTRAS_SIZE;
            int origSizeOnDisk = BitConverter.ToInt32(original, extrasStart + 8);

            // Copy everything up to Extras
            output.Write(original, srcPos, extrasStart - srcPos);

            // Write patched Extras
            byte[] extras = new byte[EXTRAS_SIZE];
            Array.Copy(original, extrasStart, extras, 0, EXTRAS_SIZE);
            BitConverter.GetBytes(newFonts[i].Length).CopyTo(extras, 4);
            BitConverter.GetBytes(compressed[i].Length - 4).CopyTo(extras, 8);
            output.Write(extras);

            // Write compressed data without sentinel (original file has one at the very end)
            output.Write(compressed[i], 0, compressed[i].Length - 4);

            srcPos = bulkStart + origSizeOnDisk;
        }

        // Copy remaining data (font path references + sentinel)
        output.Write(original, srcPos, original.Length - srcPos);

        byte[] result = output.ToArray();

        // Patch OffsetInFile where needed
        foreach (int offset in FindAllBulkDataTags(result))
        {
            int extrasStart = offset - EXTRAS_SIZE;
            if (BitConverter.ToInt64(result, extrasStart + 12) != 0)
                BitConverter.GetBytes((long)offset).CopyTo(result, extrasStart + 12);
        }

        File.WriteAllBytes(outputPath, result);
    }

    private static List<int> FindAllBulkDataTags(byte[] data)
    {
        var offsets = new List<int>();
        for (int i = 4; i < data.Length - 4; i++)
            if (data[i] == 0xC1 && data[i + 1] == 0x83 && data[i + 2] == 0x2A && data[i + 3] == 0x9E)
                offsets.Add(i);
        return offsets;
    }

    private static List<string> FindFontFilenames(byte[] data)
    {
        var names = new List<string>();
        for (int i = 0; i < data.Length - 5; i++)
        {
            if (data[i] != '.' || data[i + 4] != 0) continue;
            bool isTtf = data[i + 1] == 't' && data[i + 2] == 't' && data[i + 3] == 'f';
            bool isOtf = data[i + 1] == 'o' && data[i + 2] == 't' && data[i + 3] == 'f';
            if (!isTtf && !isOtf) continue;

            int end = i + 4;
            int start = i;
            while (start > 0 && data[start - 1] >= 0x20 && data[start - 1] < 0x7F)
                start--;

            names.Add(Path.GetFileName(Encoding.ASCII.GetString(data, start, end - start)));
        }
        return names;
    }
}
