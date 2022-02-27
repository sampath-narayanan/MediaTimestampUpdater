namespace J4JSoftware.ExifTSUpdater;

public interface IAppConfig : IExtractionConfig
{
    bool SkipChanges { get; set; }
    bool HelpRequested { get; set; }
}
