using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using J4JSoftware.ExifTSUpdater;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace MediaTimestampUpdater;

public class MediaFileViewModel : ObservableObject, IFileChangeInfo
{
    private readonly DispatcherQueue _dQueue = DispatcherQueue.GetForCurrentThread();

    private string _rootPath = string.Empty;
    private string _filePath = string.Empty;
    private DateTime?  _created;
    private DateTime? _taken;

    public string RootPath
    {
        get => _rootPath;

        set
        {
            SetProperty( ref _rootPath, value );
            OnPropertyChanged( nameof( DisplayPath ) );
        }
    }

    public bool FileExists { get; private set; }

    public string FilePath
    {
        get => _filePath;

        set
        {
            FileExists = File.Exists( value );
            OnPropertyChanged(nameof(FileExists));

            SetProperty( ref _filePath, value );
            OnPropertyChanged( nameof( DisplayPath ) );

            DateCreated = FileExists ? File.GetCreationTime( _filePath ) : null;
        }
    }

    [Display(Name = "File Path")]
    public string DisplayPath
    {
        get
        {
            if( string.IsNullOrEmpty( _filePath ) || string.IsNullOrEmpty(_rootPath) )
                return _filePath;

            var rootStart = _filePath.IndexOf( _rootPath, StringComparison.OrdinalIgnoreCase );

            return rootStart != 0 ? _filePath : _filePath[ ( _rootPath!.Length + 1 )..^0 ];
        }
    }

    [Display(Name = "Date File Created")]
    public DateTime? DateCreated
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

    public ScanStatus ScanStatus { get; set; }
    public string? ExtractorName { get; set; }
    public List<TagInfo> Tags { get; } = new();

    public void DoAction( Action<IFileChangeInfo> action )
    {
        _dQueue.TryEnqueue( () => action( this ) );
    }
}
