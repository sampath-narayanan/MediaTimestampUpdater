using Directory = MetadataExtractor.Directory;

namespace J4JSoftware.ExifTSUpdater;

public interface ITimestampExtractor : IEquatable<ITimestampExtractor>
{
    Type MdeDirectoryType { get; }

    ScanInfo GetDateTime( IReadOnlyList<MetadataExtractor.Directory> directories );
}
