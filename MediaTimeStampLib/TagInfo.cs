namespace J4JSoftware.ExifTSUpdater;

public class TagInfo
{
    public TagInfo()
    {
    }

    public TagInfo(
        MetadataExtractor.Tag tag
    )
    {
        DirectoryName = tag.DirectoryName;
        TagName = tag.Name;
        Value = tag.Description;
    }

    public string DirectoryName { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public string? Value { get; set; }
}
