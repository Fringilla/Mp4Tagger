using Mp4Tagger.Models;

namespace Mp4Tagger.Services;

public static class TagWriter
{
    public static void Apply(string path, MediaMetadata m, bool dryRun)
    {
        if (dryRun) return;
        using var media = TagLib.File.Create(path);
        if (m.Title is not null) media.Tag.Title = m.Title;
        if (m.Artist is not null) media.Tag.Performers = [m.Artist];
        if (m.Album is not null) media.Tag.Album = m.Album;
        if (m.Year is not null) media.Tag.Year = m.Year.Value;
        if (m.Track is not null) media.Tag.Track = m.Track.Value;
        if (m.TrackCount is not null) media.Tag.TrackCount = m.TrackCount.Value;
        if (m.Disc is not null) media.Tag.Disc = m.Disc.Value;
        if (m.DiscCount is not null) media.Tag.DiscCount = m.DiscCount.Value;
        media.Save();
    }
}
