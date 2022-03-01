namespace J4JSoftware.ExifTSUpdater;

public class FileChanges : List<FileChangeInfo>, IMultiThreadCollection<FileChangeInfo>
{
    public void DoAction( Action<ICollection<FileChangeInfo>> action )
    {
        action( this );
    }
}
