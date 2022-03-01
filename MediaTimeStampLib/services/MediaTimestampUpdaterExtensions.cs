using System.Collections.ObjectModel;
using System.Text.Json;

namespace J4JSoftware.ExifTSUpdater;

public static class MediaTimestampUpdaterExtensions
{
    private static readonly uint ScanStatusMask;

    static MediaTimestampUpdaterExtensions()
    {
        for (var idx = 0; idx < 32; idx++)
        {
            ScanStatusMask += (uint)1 << idx;
        }
    }

    public static void ScanStatusCountToConsole(this Collection<IFileChangeInfo> fileChanges, ScanStatus scanStatus)
    {
        var description = scanStatus switch
                          {
                              ScanStatus.NotScanned                      => "Could not find metadata directory",
                              ScanStatus.SupportedMetadataDirectoryFound => "Could not find date/time tag(s)",
                              ScanStatus.DateTimeTagFound                => "Could not parse date/time text",
                              ScanStatus.ExceptionOnScan                 => "Exception encountered in extraction",
                              _                                          => string.Empty
                          };

        if (string.IsNullOrEmpty(description))
            return;

        var numericStatus = (uint)scanStatus;
        var flippedNumericStatus = numericStatus ^ ScanStatusMask;

        var errorCount = fileChanges.Count(x => (x.ScanStatus & scanStatus) == scanStatus
                                                && ((uint)x.ScanStatus & flippedNumericStatus) == 0);

        Console.WriteLine($"\t{description}: {errorCount:n0}");
    }

    public static async Task OutputJsonFile(this Collection<IFileChangeInfo> fileChanges, InfoToReport infoToReport )
    {
        var toReport = infoToReport switch
                       {
                           InfoToReport.AllTimestamps => fileChanges,
                           InfoToReport.InvalidTimestamps =>
                               fileChanges.Where( x => x.ScanStatus != ScanStatus.Valid ),
                           InfoToReport.ValidTimestamps =>
                               fileChanges.Where( x => x.ScanStatus == ScanStatus.Valid ),
                           _ => Enumerable.Empty<IFileChangeInfo>()
                       };

        await using var fileStream =
            File.Create(Path.Combine(Directory.GetCurrentDirectory(), "changes.json"));

        await JsonSerializer.SerializeAsync(fileStream,
                                            toReport,
                                            new JsonSerializerOptions() { WriteIndented = true });
    }
}
