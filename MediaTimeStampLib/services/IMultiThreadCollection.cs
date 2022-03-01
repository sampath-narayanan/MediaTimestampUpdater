namespace J4JSoftware.ExifTSUpdater;

public interface IMultiThreadCollection<T> : IList<T>
{
    void DoAction( Action<ICollection<T>> action );
}
