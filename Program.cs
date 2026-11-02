using Mp4Tagger.Services;

static void Help()
{
    Console.WriteLine("Mp4Tagger - batch MP4 metadata reader/writer");
    Console.WriteLine();
    Console.WriteLine("Import/write:");
    Console.WriteLine("  Mp4Tagger <media-folder> <metadata.csv|json> [--dry-run] [--recursive]");
    Console.WriteLine();
    Console.WriteLine("List metadata:");
    Console.WriteLine("  Mp4Tagger list <media-folder> [--recursive]");
    Console.WriteLine();
    Console.WriteLine("Export CSV:");
    Console.WriteLine("  Mp4Tagger export <media-folder> <output.csv> [--recursive]");
}

if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase))
{
    Help();
    return 0;
}

if (args[0].Equals("list", StringComparison.OrdinalIgnoreCase))
{
    if (args.Length < 2) { Help(); return 1; }

    var root = Path.GetFullPath(args[1]);
    var recursive = args.Contains("--recursive", StringComparer.OrdinalIgnoreCase);
    if (!Directory.Exists(root)) { Console.Error.WriteLine($"Folder not found: {root}"); return 2; }

    MetadataLister.List(root, recursive);
    return 0;
}

if (args[0].Equals("export", StringComparison.OrdinalIgnoreCase))
{
    if (args.Length < 3) { Help(); return 1; }

    var root = Path.GetFullPath(args[1]);
    var output = Path.GetFullPath(args[2]);
    var recursive = args.Contains("--recursive", StringComparer.OrdinalIgnoreCase);

    if (!Directory.Exists(root)) { Console.Error.WriteLine($"Folder not found: {root}"); return 2; }
    if (!Path.GetExtension(output).Equals(".csv", StringComparison.OrdinalIgnoreCase))
    { Console.Error.WriteLine("Export output must be a .csv file."); return 2; }

    try
    {
        MetadataExporter.ExportCsv(root, output, recursive);
        Console.WriteLine($"Metadata exported to: {output}");
        return 0;
    }
    catch (Exception ex) { Console.Error.WriteLine($"Export failed: {ex.Message}"); return 5; }
}

if (args.Length < 2) { Help(); return 1; }

var mediaRoot = Path.GetFullPath(args[0]);
var metadataPath = Path.GetFullPath(args[1]);
var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
var importRecursive = args.Contains("--recursive", StringComparer.OrdinalIgnoreCase);

if (!Directory.Exists(mediaRoot)) { Console.Error.WriteLine($"Folder not found: {mediaRoot}"); return 2; }
if (!File.Exists(metadataPath)) { Console.Error.WriteLine($"Metadata file not found: {metadataPath}"); return 2; }

IReadOnlyList<Mp4Tagger.Models.MediaMetadata> items;
try { items = MetadataLoader.Load(metadataPath); }
catch (Exception ex) { Console.Error.WriteLine($"Cannot load metadata: {ex.Message}"); return 3; }

var mp4Files = Directory.EnumerateFiles(mediaRoot, "*.mp4", importRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
    .ToDictionary(p => Path.GetRelativePath(mediaRoot, p), p => p, StringComparer.OrdinalIgnoreCase);

var ok = 0; var failed = 0;
foreach (var item in items)
{
    var relative = item.File.Replace('/', Path.DirectorySeparatorChar);
    if (!mp4Files.TryGetValue(relative, out var path))
    {
        path = Path.Combine(mediaRoot, relative);
        if (!File.Exists(path)) { Console.Error.WriteLine($"[MISSING] {item.File}"); failed++; continue; }
    }

    try
    {
        Console.WriteLine($"[{(dryRun ? "DRY" : "WRITE")}] {item.File} | {item.Artist} | {item.Album} | {item.Title}");
        TagWriter.Apply(path, item, dryRun);
        ok++;
    }
    catch (Exception ex) { Console.Error.WriteLine($"[ERROR] {item.File}: {ex.Message}"); failed++; }
}

Console.WriteLine($"Done. Successful: {ok}, Failed: {failed}, DryRun: {dryRun}");
return failed == 0 ? 0 : 4;
