namespace J4JSoftware.ExifTSUpdater;

public interface IAppConfig
{
    List<string> Extensions { get; set; }
    string MediaDirectory { get; set; }
    string ChangesFile { get; set; }
    bool ErrorsOnly { get; set; }
    bool NoChanges { get; set; }
    bool HelpRequested { get; set; }

    List<FileChangeInfo> Changes { get; }
}
