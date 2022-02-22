namespace J4JSoftware.ExifTSUpdater;

public record TagInfo
{
    public TagInfo(
        MetadataExtractor.Tag tag
    )
    {
        DirectoryName = tag.DirectoryName;
        TagName = tag.Name;
        Value = tag.Description;
    }

    public string DirectoryName { get; init; }
    public string TagName { get; init; }
    public string? Value { get; init; }
}
