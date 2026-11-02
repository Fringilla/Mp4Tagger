using System.Globalization;
using System.Text;
using Mp4Tagger.Models;

namespace Mp4Tagger.Services;

public static class MetadataExporter
{
    public static void ExportCsv(string root, string outputPath, bool recursive)
    {
        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.EnumerateFiles(root, "*.mp4", option)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

        using var writer = new StreamWriter(outputPath, false, new UTF8Encoding(true));
        writer.WriteLine("File,Title,Artist,Album,Year,Track,TrackCount,Disc,DiscCount");

        foreach (var path in files)
        {
            using var media = TagLib.File.Create(path);
            var row = new[]
            {
                Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'),
                media.Tag.Title ?? string.Empty,
                string.Join("; ", media.Tag.Performers ?? []),
                media.Tag.Album ?? string.Empty,
                Value(media.Tag.Year),
                Value(media.Tag.Track),
                Value(media.Tag.TrackCount),
                Value(media.Tag.Disc),
                Value(media.Tag.DiscCount)
            };
            writer.WriteLine(string.Join(',', row.Select(Escape)));
        }
    }

    private static string Value(uint value) => value == 0 ? string.Empty : value.ToString(CultureInfo.InvariantCulture);

    private static string Escape(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\r') && !value.Contains('\n'))
            return value;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
