using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using MetadataExtractor.Formats.Exif;

namespace J4JSoftware.ExifTSUpdater;

[Predecessor(null)]
public class ExIfSubIfdExtractor : SingleDateTimeTagExtractor<ExifSubIfdDirectory>
{
    public ExIfSubIfdExtractor( 
        IJ4JLogger logger 
    )
        : base(
               "Date/Time Original", 
               "yyyy:MM:dd HH:mm:ss", 
               null, 
               logger,
               "jpg", "jpeg", "mov", "mp4", "heic")
    {
    }
}
