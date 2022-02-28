using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Serilog.Events;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MediaTimestampUpdater
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LogViewerControl : UserControl
    {
        public LogViewerControl()
        {
            this.InitializeComponent();
        }

        private void OnMinLogEventLevelChanged( object sender, SelectionChangedEventArgs e )
        {
            // this handler gets called when the user control is initializing itself
            // but before the ItemsSource property is set and the View property is defined 
            if( LogEventsDataGrid.View == null )
                return;

            var viewModel = (LogViewerModel) DataContext;
            var minLevel = e.AddedItems.Cast<LogEventLevel>().First();

            viewModel.MinimumLogEventLevelToDisplay = minLevel;

            LogEventsDataGrid.View.Filter = viewModel.FilterLogEvents;
            LogEventsDataGrid.View.RefreshFilter( true );
        }
    }
}
