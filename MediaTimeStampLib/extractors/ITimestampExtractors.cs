namespace J4JSoftware.ExifTSUpdater;

public interface ITimestampExtractors
{
    IReadOnlyCollection<string> SupportedExtensions { get; }
    void GetTimestamp( FileChangeInfo changeInfo );
}
