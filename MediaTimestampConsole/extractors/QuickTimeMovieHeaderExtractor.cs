using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using MetadataExtractor.Formats.QuickTime;

namespace J4JSoftware.ExifTSUpdater;

[Predecessor(typeof(ExIfSubIfdExtractor))]
public class QuickTimeMovieHeaderExtractor : SingleDateTimeTagExtractor<QuickTimeMovieHeaderDirectory>
{
    public QuickTimeMovieHeaderExtractor(
        IJ4JLogger logger
    )
        : base("Date/Time Original", "MMM dd HH:mm:ss yyyy", typeof(ExIfSubIfdExtractor), logger)
    {
    }

    protected override string? GetDateTimeString( QuickTimeMovieHeaderDirectory directory )
    {
        var retVal = base.GetDateTimeString( directory );
        if( retVal == null )
            return null;

        var parts = retVal.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length != 5 ? null : string.Join( ' ', parts[ 1.. ] );
    }
}
