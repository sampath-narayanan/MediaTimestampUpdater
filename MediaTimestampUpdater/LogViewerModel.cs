using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Serilog;
using Serilog.Events;

namespace MediaTimestampUpdater
{
    public class LogViewerModel : ObservableObject
    {
        private LogEventLevel _minLogEventLevel = LogEventLevel.Information;

        public LogViewerModel()
        {
            var logger = App.Current.Host.Services.GetRequiredService<IJ4JLogger>();
            logger.LogEvent += Logger_LogEvent;

            ClearLogCommand = new RelayCommand( ClearLogHandler );
            CloseCommand = new RelayCommand( CloseHandler );
        }

        private void Logger_LogEvent( object? sender, NetEventArgs e )
        {
            LogEvents.Add( e );
        }

        public ObservableCollection<NetEventArgs> LogEvents { get; } = new();

        public RelayCommand ClearLogCommand { get; }

        private void ClearLogHandler()
        {
            LogEvents.Clear();
            OnPropertyChanged( nameof( LogEvents ) );
        }

        public RelayCommand CloseCommand { get; }

        private void CloseHandler()
        {
            var prevControl = App.Current.CachedElements.Pop();
            App.Current.MainWindow!.Content = prevControl;
        }

        public List<LogEventLevel> LogEventLevels { get; } = new()
                                                             {
                                                                 LogEventLevel.Verbose,
                                                                 LogEventLevel.Debug,
                                                                 LogEventLevel.Information,
                                                                 LogEventLevel.Warning,
                                                                 LogEventLevel.Error,
                                                                 LogEventLevel.Fatal
                                                             };

        public LogEventLevel MinimumLogEventLevelToDisplay
        {
            get => _minLogEventLevel;

            set
            {
                SetProperty( ref _minLogEventLevel, value );
                OnPropertyChanged( nameof( LogEvents ) );
            }
        }

        public bool FilterLogEvents( object item )
        {
            var logEvent = item as NetEventArgs;

            return logEvent?.LogEvent.Level >= MinimumLogEventLevelToDisplay;
        }
    }
}
