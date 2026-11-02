# Mp4Tagger

.NET 8 console app for importing and exporting common music metadata in MP4 files using TagLib#.

## Metadata fields
Title, Artist, Album, Year, Track, TrackCount, Disc, DiscCount.

## Build
```bash
dotnet restore
dotnet build -c Release
```

## Export existing MP4 metadata to CSV
```bash
dotnet run -- export "D:\\Music\\Album" "D:\\metadata.csv"
dotnet run -- export "D:\\Music" "D:\\metadata.csv" --recursive
```

The exported File field is relative to the media folder, allowing the CSV to be edited and imported again.
Multiple performers are exported separated by `; `.

## Import/write metadata
First test safely:
```bash
dotnet run -- "D:\\Music\\Album" "D:\\metadata.csv" --dry-run
```
Then write:
```bash
dotnet run -- "D:\\Music\\Album" "D:\\metadata.csv"
```
Recursive import:
```bash
dotnet run -- "D:\\Music" "D:\\metadata.csv" --recursive
```

CSV fields containing commas, quotes or line breaks are quoted and escaped on export.
Keep a backup of valuable media before batch editing.

## List metadata in the console
```bash
dotnet run -- list "D:\\Music\\Album"
dotnet run -- list "D:\\Music" --recursive
```

The list command reads MP4 tags without modifying the files and displays file, title, artist, album, year, track/track count, and disc/disc count.
