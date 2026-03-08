using UE4BulkAssetEditor;

if (args.Length < 1)
{
    Console.WriteLine("UE4 Bulk Asset Editor - Font replacement tool for UE4 uassets with zlib-compressed bulk data");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  UE4BulkAssetEditor decompress <input.uasset> <output.ttf>");
    Console.WriteLine("  UE4BulkAssetEditor compress <input.uasset> <replacement.ttf> <output.uasset>");
    return;
}

string command = args[0].ToLower();

switch (command)
{
    case "decompress":
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: UE4BulkAssetEditor decompress <input.uasset> <output.ttf>");
            return;
        }
        string inputPath = args[1];
        string outputPath = args[2];

        var patcher = new FontBulkPatcher();
        byte[] font = patcher.ExtractFont(inputPath);
        File.WriteAllBytes(outputPath, font);
        Console.WriteLine($"Extracted {font.Length} bytes to {outputPath}");
        break;
    }
    case "compress":
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: UE4BulkAssetEditor compress <input.uasset> <replacement.ttf> <output.uasset>");
            return;
        }
        string inputPath = args[1];
        string fontPath = args[2];
        string outputPath = args[3];

        byte[] newFont = File.ReadAllBytes(fontPath);
        var patcher = new FontBulkPatcher();
        patcher.ReplaceFont(inputPath, newFont, outputPath);
        Console.WriteLine($"Replaced font ({newFont.Length} bytes) in {outputPath}");
        break;
    }
    default:
        Console.WriteLine($"Unknown command: {command}");
        break;
}
