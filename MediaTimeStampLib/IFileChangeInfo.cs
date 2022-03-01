namespace J4JSoftware.ExifTSUpdater;

public interface IFileChangeInfo
{
    string FilePath { get; set; }
    DateTime DateCreated { get; }
    DateTime? DateTaken { get; set; }
    ScanStatus ScanStatus { get; set; }
    string? ExtractorName { get; set; }
    List<TagInfo> Tags { get; }
}
