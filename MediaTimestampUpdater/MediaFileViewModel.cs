using System;
using System.ComponentModel.DataAnnotations;
using J4JSoftware.ExifTSUpdater;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MediaTimestampUpdater;

public class MediaFileViewModel : ObservableObject
{
    private readonly string? _rootPath;
    private readonly IJ4JLogger _logger;

    private string? _path;
    private DateTime _created;
    private DateTime? _taken;

    public MediaFileViewModel(
        string? rootPath,
        FileChangeInfo changeInfo,
        IJ4JLogger logger
    )
    {
        _rootPath = rootPath;
        _logger = logger;

        _path = changeInfo.FilePath;
        OnPropertyChanged(nameof(DisplayPath));

        DateCreated = changeInfo.DateCreated;
        DateTaken = changeInfo.DateTaken;
    }

    [Display(Name = "File Path")]
    public string? DisplayPath
    {
        get
        {
            if( string.IsNullOrEmpty( _path ) )
                return null;

            if( string.IsNullOrEmpty(_rootPath))
                return _path;

            var rootStart = _path.IndexOf( _rootPath!, StringComparison.OrdinalIgnoreCase );
            return rootStart != 0 ? _path : _path[(_rootPath!.Length + 1)..^0];
        }
    }

    [Display(Name = "Date File Created")]
    public DateTime DateCreated
    {
        get => _created;
        set => SetProperty( ref _created, value );
    }

    [Display(Name = "Date Taken")]
    public DateTime? DateTaken
    {
        get => _taken;
        set => SetProperty( ref _taken, value );
    }

    public void ChangeDateCreated( DateTime? dateTaken = null )
    {
        dateTaken ??= DateTaken;

        if( dateTaken == null )
        {
            _logger.Warning<string?>( "{0}: Date taken is undefined, date created not updated", DisplayPath );
            return;
        }
    }
}
