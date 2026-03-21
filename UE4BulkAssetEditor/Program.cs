using UAssetAPI.UnrealTypes;
using UE4BulkAssetEditor;

if (args.Length < 2)
{
    PrintUsage();
    return;
}

string command = args[0].ToLower();
bool gearsMode = args.Any(a => a == "--gears");
var engine = ParseEngine(args.FirstOrDefault(a => a.StartsWith("--engine="))?.Split('=')[1]);
var positional = args.Skip(1).Where(a => !a.StartsWith("--")).ToArray();

IFontPatcher patcher = gearsMode
    ? new FontBulkPatcher()
    : new UAssetFontPatcher(engine);

switch (command)
{
    case "decompress":
    {
        if (positional.Length < 2)
        {
            Console.WriteLine("Usage: UE4BulkAssetEditor decompress <input.uasset> <output_dir>");
            return;
        }
        Directory.CreateDirectory(positional[1]);

        var fonts = patcher.ExtractFonts(positional[0]);
        foreach (var font in fonts)
        {
            File.WriteAllBytes(Path.Combine(positional[1], font.Name), font.Data);
            Console.WriteLine($"  {font.Data.Length,10} bytes -> {font.Name}");
        }
        Console.WriteLine($"Extracted {fonts.Count} font(s)");
        break;
    }
    case "compress":
    {
        if (positional.Length < 3)
        {
            Console.WriteLine("Usage: UE4BulkAssetEditor compress <input.uasset> <output.uasset> <font1.ttf> [font2.ttf] ...");
            return;
        }
        var newFonts = positional.Skip(2).Select(File.ReadAllBytes).ToList();
        patcher.ReplaceFonts(positional[0], newFonts, positional[1]);
        Console.WriteLine($"Replaced {newFonts.Count} font(s) in {positional[1]}");
        break;
    }
    default:
        Console.WriteLine($"Unknown command: {command}");
        PrintUsage();
        break;
}

static EngineVersion ParseEngine(string? version) => version switch
{
    "4.13" => EngineVersion.VER_UE4_13,
    "4.27" => EngineVersion.VER_UE4_27,
    _ => EngineVersion.VER_UE4_13,
};

static void PrintUsage()
{
    Console.WriteLine("UE4 Bulk Asset Editor - Font replacement tool for UE4 uassets");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  UE4BulkAssetEditor decompress <input.uasset> <output_dir> [options]");
    Console.WriteLine("  UE4BulkAssetEditor compress <input.uasset> <output.uasset> <font.ttf> [font2.ttf ...] [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --engine=VER   Engine version: 4.13 (default), 4.27");
    Console.WriteLine("  --gears        Custom engine mode for Gears 5/Tactics (multiple fonts, interleaved bulk data)");
}
