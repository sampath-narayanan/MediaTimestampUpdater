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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MediaTimestampUpdater
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            Title = "Media Timestamp Updater";
            ViewModel = App.Current.Host.Services.GetRequiredService<MainViewModel>();

            ViewModel.ViewHandle = WinRT.Interop.WindowNative.GetWindowHandle( this );

            // adjust initial window size
            var windowId = Win32Interop.GetWindowIdFromWindow(ViewModel.ViewHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            var winSize = appWindow.Size;
            winSize.Height = winSize.Height > 700 ? 700 : winSize.Height;
            winSize.Width = winSize.Width > 1000 ? 1000 : winSize.Width;

            appWindow.Resize(winSize);
        }

        internal MainViewModel ViewModel { get; }
    }
}
