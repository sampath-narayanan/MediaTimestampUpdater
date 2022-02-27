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
    public class LogViewModel : ObservableObject
    {
        private readonly List<NetEventArgs> _logEvents = new();

        private bool _hideInformationalEvents;
        private LogEventLevel _minLogEventLevel = LogEventLevel.Information;

        public LogViewModel()
        {
            var logger = App.Current.Host.Services.GetRequiredService<IJ4JLogger>();
            logger.LogEvent += Logger_LogEvent;

            ClearLog = new RelayCommand( ClearLogHandler );
        }

        private void Logger_LogEvent( object? sender, NetEventArgs e )
        {
            _logEvents.Add( e );
            OnPropertyChanged( nameof( LogEvents ) );
        }

        public IEnumerable<NetEventArgs> LogEvents =>
            _logEvents.Where( x => x.LogEvent.Level >= _minLogEventLevel );

        public RelayCommand ClearLog { get; }

        private void ClearLogHandler()
        {
            _logEvents.Clear();
            OnPropertyChanged( nameof( LogEvents ) );
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

        public bool HideInformationalEvents
        {
            get => _hideInformationalEvents;

            set
            {
                SetProperty( ref _hideInformationalEvents, value );
                OnPropertyChanged( nameof( LogEvents ) );
            }
        }
    }
}
