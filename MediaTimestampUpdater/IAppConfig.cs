namespace J4JSoftware.ExifTSUpdater;

public interface IAppConfig
{
    List<string> Extensions { get; set; }
    string MediaDirectory { get; set; }

    InfoToReport InfoToReport { get; set; }
    bool ReportChanges { get; }
    bool ReportTags { get; }

    bool SkipChanges { get; set; }
    
    bool HelpRequested { get; set; }

    List<FileChangeInfo> Changes { get; }
}
