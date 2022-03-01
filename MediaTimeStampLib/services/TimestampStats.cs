namespace J4JSoftware.ExifTSUpdater;

public record TimestampStats(int Total, int Skipped, bool Aborted = false )
{
    public int Adjusted => Total - Skipped;
}
