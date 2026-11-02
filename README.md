# Mp4Tagger

.NET 8 console app for batch-writing common music metadata to MP4 files using TagLib#.

## Fields
Title, Artist, Album, Year, Track, TrackCount, Disc, DiscCount.

## Build
```bash
dotnet restore
dotnet build -c Release
```

## Run
```bash
dotnet run -- "D:\\Music\\Album" "Samples\\metadata.csv" --dry-run
dotnet run -- "D:\\Music\\Album" "D:\\metadata.csv"
dotnet run -- "D:\\Music" "D:\\metadata.json" --recursive
```

Always test with `--dry-run` first and keep a backup of valuable media before batch editing.

CSV uses standard quoted-field handling, including commas inside quoted values. File paths are relative to the media-folder argument.
