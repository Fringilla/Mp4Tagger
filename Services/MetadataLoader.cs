using System.Globalization;
using System.Text.Json;
using Mp4Tagger.Models;

namespace Mp4Tagger.Services;

public static class MetadataLoader
{
    public static IReadOnlyList<MediaMetadata> Load(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".json" => LoadJson(path),
            ".csv" => LoadCsv(path),
            _ => throw new ArgumentException("Metadata file must be .csv or .json.")
        };
    }

    private static IReadOnlyList<MediaMetadata> LoadJson(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<MediaMetadata>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
    }

    private static IReadOnlyList<MediaMetadata> LoadCsv(string path)
    {
        var lines = File.ReadAllLines(path);
        if (lines.Length == 0) return [];

        var headers = ParseCsvLine(lines[0]);
        var result = new List<MediaMetadata>();

        for (var i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var values = ParseCsvLine(lines[i]);
            var row = headers.Select((h, n) => (h, v: n < values.Count ? values[n] : ""))
                .ToDictionary(x => x.h, x => x.v, StringComparer.OrdinalIgnoreCase);

            string? Get(string name) => row.TryGetValue(name, out var v) && !string.IsNullOrWhiteSpace(v) ? v : null;
            uint? GetUInt(string name) => uint.TryParse(Get(name), NumberStyles.None, CultureInfo.InvariantCulture, out var v) ? v : null;

            var file = Get("File") ?? throw new FormatException($"Missing File on CSV line {i + 1}.");
            result.Add(new MediaMetadata
            {
                File = file, Title = Get("Title"), Artist = Get("Artist"), Album = Get("Album"),
                Year = GetUInt("Year"), Track = GetUInt("Track"), TrackCount = GetUInt("TrackCount"),
                Disc = GetUInt("Disc"), DiscCount = GetUInt("DiscCount")
            });
        }
        return result;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var quoted = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (quoted && i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                else quoted = !quoted;
            }
            else if (c == ',' && !quoted) { values.Add(current.ToString()); current.Clear(); }
            else current.Append(c);
        }
        values.Add(current.ToString());
        return values;
    }
}
