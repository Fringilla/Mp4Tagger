namespace Mp4Tagger.Services;

public static class MetadataLister
{
    public static void List(string root, bool recursive)
    {
        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.EnumerateFiles(root, "*.mp4", option)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (files.Length == 0)
        {
            Console.WriteLine("No MP4 files found.");
            return;
        }

        foreach (var path in files)
        {
            try
            {
                using var media = TagLib.File.Create(path);
                var relative = Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
                var artists = string.Join("; ", media.Tag.Performers ?? []);
                var track = FormatPair(media.Tag.Track, media.Tag.TrackCount);
                var disc = FormatPair(media.Tag.Disc, media.Tag.DiscCount);
                var year = media.Tag.Year == 0 ? "" : media.Tag.Year.ToString();

                Console.WriteLine(relative);
                Console.WriteLine($"  Title : {media.Tag.Title ?? ""}");
                Console.WriteLine($"  Artist: {artists}");
                Console.WriteLine($"  Album : {media.Tag.Album ?? ""}");
                Console.WriteLine($"  Year  : {year}");
                Console.WriteLine($"  Track : {track}");
                Console.WriteLine($"  Disc  : {disc}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] {path}: {ex.Message}");
            }
        }
    }

    private static string FormatPair(uint number, uint total)
    {
        if (number == 0 && total == 0) return "";
        if (total == 0) return number.ToString();
        return $"{number}/{total}";
    }
}
