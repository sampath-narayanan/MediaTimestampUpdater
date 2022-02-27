namespace J4JSoftware.ExifTSUpdater;

public interface IExtractionConfig
{
    string MediaDirectory { get; set; }
    bool ScanSubfolders { get; set; }
    InfoToReport InfoToReport { get; set; }
    bool ReportChanges { get; }
    bool ReportTags { get; }

    List<FileChangeInfo> Changes { get; }
}
