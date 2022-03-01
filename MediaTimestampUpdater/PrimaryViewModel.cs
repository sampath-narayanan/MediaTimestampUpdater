using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using J4JSoftware.ExifTSUpdater;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MediaTimestampUpdater
{
    public class PrimaryViewModel : ObservableObject
    {
        private readonly IExtractionConfig _config;
        private readonly ScanFilesService<MediaFileViewModel> _scanFilesService;
        private readonly AdjustTimestampService<MediaFileViewModel> _adjustTimestampService;
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly IJ4JLogger _logger;

        private string _folderPath = String.Empty;
        private bool _scanSubfolders;
        private bool _scanCompleted;
        private Visibility _progressBarVisibility = Visibility.Collapsed;
        private bool _progBarIndeterminate = true;
        private ObservableCollection<MediaFileViewModel> _fileInfo = new();
        private int _numFilesToAdjust;
        private int _curFilesProcessed;

        public PrimaryViewModel(
            IExtractionConfig config,
            ScanFilesService<MediaFileViewModel> scanFilesService,
            AdjustTimestampService<MediaFileViewModel> adjustTimestampService,
            IJ4JLogger logger
            )
        {
            _config = config;

            var sharedInfo = App.Current.Resources[ "SharedFileInfo" ] as SharedFileInfo;

            _scanFilesService = scanFilesService;
            _scanFilesService.FileChanges = sharedInfo;

            _adjustTimestampService = adjustTimestampService;
            _adjustTimestampService.FileChanges = sharedInfo;
            _adjustTimestampService.FileProcessed += Timestamp_FileProcessed;

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            PickFolderCommand = new AsyncRelayCommand( PickHolderHandler );
            AdjustTimestampsCommand = new AsyncRelayCommand( AdjustTimestampsHandler );
        }

        private void Timestamp_FileProcessed(object? sender, string filePath )
        {
            CurrentFilesProcessed++;
        }

        public AsyncRelayCommand PickFolderCommand { get; }

        private async Task PickHolderHandler()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker
                               {
                                   SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
                               };


            WinRT.Interop.InitializeWithWindow.Initialize( folderPicker, App.Current.MainWindowIntPtr );

            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            ScanCompleted = false;

            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                        FutureAccessList.AddOrReplace("PickedFolderToken", folder);

                FolderPath = folder.Path;

                _config.ScanSubfolders = ScanSubfolders;
                _config.MediaDirectory = folder.Path;

                ProgressBarVisibility = Visibility.Visible;
                ProgressBarIsIndeterminate = true;

                await Task.Run( async () =>
                                {
                                    await _scanFilesService.StartAsync(_tokenSource.Token);
                                } );

                ProgressBarVisibility = Visibility.Collapsed;

                ScanCompleted = true;

                ( (SharedFileInfo) _scanFilesService.FileChanges! ).CollectionReset();
            }
            else
            {
                _logger.Information( "Folder selection cancelled." );
            }
        }

        public AsyncRelayCommand AdjustTimestampsCommand { get; }

        private async Task AdjustTimestampsHandler()
        {
            ProgressBarVisibility = Visibility.Visible;
            ProgressBarIsIndeterminate = false;

            var changeInfo = App.Current.Resources[ "SharedFileInfo" ] as ObservableCollection<MediaFileViewModel>;
            NumFilesToAdjust = changeInfo!.Count;
            CurrentFilesProcessed = 0;

            await Task.Run( async () => await _adjustTimestampService.StartAsync( _tokenSource.Token ) );

            ProgressBarVisibility = Visibility.Collapsed;
        }

        public string FolderPath
        {
            get => _folderPath;

            set
            {
                SetProperty( ref _folderPath, value );

                ( (SharedFileInfo) _scanFilesService.FileChanges! ).RootPath = value;
            }
        }

        public bool ScanSubfolders
        {
            get => _scanSubfolders;
            set => SetProperty( ref _scanSubfolders, value );
        }

        public bool ScanCompleted
        {
            get => _scanCompleted;
            set => SetProperty( ref _scanCompleted, value );
        }

        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => SetProperty( ref _progressBarVisibility, value );
        }

        public bool ProgressBarIsIndeterminate
        {
            get => _progBarIndeterminate;
            set => SetProperty( ref _progBarIndeterminate, value );
        }

        public int NumFilesToAdjust
        {
            get => _numFilesToAdjust;
            set => SetProperty( ref _numFilesToAdjust, value );
        }

        public int CurrentFilesProcessed
        {
            get => _curFilesProcessed;
            set => SetProperty( ref _curFilesProcessed, value );
        }

        public ObservableCollection<MediaFileViewModel> FileInfo
        {
            get => _fileInfo;
            private set => SetProperty( ref _fileInfo, value );
        }
    }
}
