using Mp4Tagger.Services;

if (args.Length < 2 || args.Contains("--help", StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine("Mp4Tagger - batch MP4 metadata writer");
    Console.WriteLine("Usage: Mp4Tagger <media-folder> <metadata.csv|json> [--dry-run] [--recursive]");
    return args.Length < 2 ? 1 : 0;
}

var root = Path.GetFullPath(args[0]);
var metadataPath = Path.GetFullPath(args[1]);
var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
var recursive = args.Contains("--recursive", StringComparer.OrdinalIgnoreCase);

if (!Directory.Exists(root)) { Console.Error.WriteLine($"Folder not found: {root}"); return 2; }
if (!File.Exists(metadataPath)) { Console.Error.WriteLine($"Metadata file not found: {metadataPath}"); return 2; }

IReadOnlyList<Mp4Tagger.Models.MediaMetadata> items;
try { items = MetadataLoader.Load(metadataPath); }
catch (Exception ex) { Console.Error.WriteLine($"Cannot load metadata: {ex.Message}"); return 3; }

var mp4Files = Directory.EnumerateFiles(root, "*.mp4", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
    .ToDictionary(p => Path.GetRelativePath(root, p), p => p, StringComparer.OrdinalIgnoreCase);

var ok = 0; var failed = 0;
foreach (var item in items)
{
    var relative = item.File.Replace('/', Path.DirectorySeparatorChar);
    if (!mp4Files.TryGetValue(relative, out var path))
    {
        // Convenience fallback for root-level file names.
        path = Path.Combine(root, relative);
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
