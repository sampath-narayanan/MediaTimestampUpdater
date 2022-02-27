using J4JSoftware.Logging;

namespace J4JSoftware.ExifTSUpdater;

public class SingleDateTimeTagExtractor<T> : TimestampExtractor<T>
    where T : MetadataExtractor.Directory
{
    private readonly string _dtTagName;

    protected SingleDateTimeTagExtractor(
        string dtTagName,
        string dtFormat,
        Type? priorExtractor,
        IJ4JLogger logger,
        params string[] supportedExtensions
    )
        : base( dtFormat, logger, supportedExtensions )
    {
        _dtTagName = dtTagName;
    }

    protected override string? GetDateTimeString( T directory )
    {
        var dtTag = directory.Tags.FirstOrDefault( x => x.Name.Equals( _dtTagName,
                                                                      StringComparison.OrdinalIgnoreCase ) );

        return dtTag?.Description;
    }
}
