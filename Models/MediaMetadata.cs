namespace Mp4Tagger.Models;

public sealed class MediaMetadata
{
    public required string File { get; init; }
    public string? Title { get; init; }
    public string? Artist { get; init; }
    public string? Album { get; init; }
    public uint? Year { get; init; }
    public uint? Track { get; init; }
    public uint? TrackCount { get; init; }
    public uint? Disc { get; init; }
    public uint? DiscCount { get; init; }
}
