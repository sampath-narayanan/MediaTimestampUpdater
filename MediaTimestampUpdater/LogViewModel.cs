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
using Serilog.Events;

namespace MediaTimestampUpdater
{
    public class LogViewModel : ObservableObject
    {
        public LogViewModel()
        {
            var logger = App.Current.Host.Services.GetRequiredService<IJ4JLogger>();
            logger.LogEvent += Logger_LogEvent;

            ClearLog = new RelayCommand( ClearLogHandler );
        }

        private void Logger_LogEvent(object? sender, NetEventArgs e)
        {
            LogEvents.Add( e );
        }

        public ObservableCollection<NetEventArgs> LogEvents { get; } = new();

        public RelayCommand ClearLog { get; }

        private void ClearLogHandler() => LogEvents.Clear();
    }
}
