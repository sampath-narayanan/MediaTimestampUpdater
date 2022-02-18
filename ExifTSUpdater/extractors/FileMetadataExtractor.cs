using J4JSoftware.Logging;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;
using MetadataExtractor.Formats.FileType;

namespace J4JSoftware.ExifTSUpdater;

[Predecessor(typeof(QuickTimeMovieHeaderExtractor))]
public class FileMetadataExtractor : SingleDateTimeTagExtractor<FileMetadataDirectory>
{
    public FileMetadataExtractor(
        IJ4JLogger logger
    )
        : base( "File Modified Date", "MMM dd HH:mm:ss zzzz yyyy", typeof( QuickTimeMovieHeaderExtractor ), logger )
    {
    }

    protected override string? GetDateTimeString(FileMetadataDirectory directory)
    {
        var retVal = base.GetDateTimeString(directory);
        if (retVal == null)
            return null;

        var parts = retVal.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length != 6 ? null : string.Join(' ', parts[1..]);
    }
}
