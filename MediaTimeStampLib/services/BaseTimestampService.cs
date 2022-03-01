using System.Collections.ObjectModel;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.ExifTSUpdater;

public abstract class BaseTimestampService<T> : IHostedService
    where T : class, IFileChangeInfo, new()
{
    public event EventHandler? Started;
    public event EventHandler<string>? FileProcessed;
    public event EventHandler<TimestampStats>? Completed;

    protected BaseTimestampService(
        IExtractionConfig config,
        IHostApplicationLifetime lifetime,
        IJ4JLogger logger
    )
    {
        Configuration = config;
        Lifetime = lifetime;

        Logger = logger;
        Logger.SetLoggedType( GetType() );
    }

    protected IJ4JLogger Logger { get; }
    protected IExtractionConfig Configuration { get; }
    protected IHostApplicationLifetime Lifetime { get; }

    public IMultiThreadCollection<T>? FileChanges { get; set; }
    protected int Skipped { get; set; }

    public virtual async Task StartAsync( CancellationToken token )
    {
        if( FileChanges == null )
        {
            Logger.Fatal("FileChanges property not defined");
            return;
        }

        Started?.Invoke(this, EventArgs.Empty);
        Skipped = 0;

        await Process( token );

        Completed?.Invoke( this, new TimestampStats( FileChanges.Count, Skipped ) );
    }

    protected abstract Task Process( CancellationToken token );

    public virtual Task StopAsync( CancellationToken token )
    {
        if (FileChanges == null)
            Logger.Fatal("FileChanges property not defined");
        else
            Completed?.Invoke(this, new TimestampStats(FileChanges.Count, Skipped, true));

        Lifetime.StopApplication();

        return Task.CompletedTask;
    }

    protected virtual void OnFileProcessed( string filePath ) => FileProcessed?.Invoke( this, filePath );
}
