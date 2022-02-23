namespace J4JSoftware.ExifTSUpdater;

public record ScanInfo
{
    public ScanStatus ScanResult { get; set; } = ScanStatus.NotScanned;
    public DateTime Timestamp { get; set; } = DateTime.MinValue;
    public string? ExtractorName { get; set; }
}
