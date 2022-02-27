using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.ExifTSUpdater;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Path = System.IO.Path;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MediaTimestampUpdater
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;

        public Window? MainWindow { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            //Register Syncfusion license
            Syncfusion.Licensing
                      .SyncfusionLicenseProvider
                      .RegisterLicense("NTg2NTM0QDMxMzkyZTM0MmUzMGpmNUlrRmd6WXdHenpRd0thSTZDeDA4SW0xV1NJZGJuRUZNWDhWVnR0YkE9");

            var hostConfig = new J4JHostConfiguration()
                             .Publisher("J4JSoftware")
                             .ApplicationName("MediaTimestampUpdater")
                             .LoggerInitializer(InitializeLogger)
                             .AddNetEventSinkToLogger()
                             .AddDependencyInjectionInitializers(SetupDependencyInjection)
                             .AddServicesInitializers(InitializeServices)
                             .FilePathTrimmer(FilePathTrimmer);

            if (hostConfig.MissingRequirements != J4JHostRequirements.AllMet)
                throw new ApplicationException($"Missing J4JHostConfiguration items: {hostConfig.MissingRequirements}");

            Host = hostConfig.Build()
                   ?? throw new NullReferenceException($"Failed to build {nameof(IJ4JHost)}");

            var logger = Host.Services.GetRequiredService<IJ4JLogger>();

            logger.OutputCache(hostConfig.Logger);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }

        public IJ4JHost Host { get; }

        private static void InitializeLogger(IConfiguration config, J4JLoggerConfiguration loggerConfig)
        {
            loggerConfig.SerilogConfiguration
                        .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "log.txt"),
                                      rollingInterval: RollingInterval.Day);
        }

        private static void SetupDependencyInjection(HostBuilderContext hbc, ContainerBuilder builder)
        {
            builder.RegisterType<ExtractionConfig>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            var typeTests = new TypeTests<ITimestampExtractor>()
                            .AddTests(PredefinedTypeTests.OnlyJ4JLoggerRequired)
                            .AddTests(PredefinedTypeTests.NonAbstract);

            builder.RegisterTypeAssemblies<FileChangeInfo>(typeTests);

            builder.RegisterType<TimestampExtractors>()
                   .As<ITimestampExtractors>()
                   .SingleInstance();

            builder.RegisterType<MainViewModel>()
                   .AsSelf()
                   .SingleInstance();

            builder.RegisterType<ScanFilesService>()
                   .AsSelf()
                   .SingleInstance();

            builder.RegisterType<AdjustCreationDTService>()
                   .AsSelf()
                   .SingleInstance();
        }

        private void InitializeServices(HostBuilderContext hbc, IServiceCollection services)
        {
            services.AddHostedService<ScanFilesService>();
            services.AddHostedService<AdjustCreationDTService>();
        }

        // these next two methods serve to strip the project path off of source code
        // file paths
        private string FilePathTrimmer(Type? loggedType,
                                              string callerName,
                                              int lineNum,
                                              string srcFilePath)
        {
            return CallingContextEnricher.DefaultFilePathTrimmer(loggedType,
                                                                 callerName,
                                                                 lineNum,
                                                                 CallingContextEnricher.RemoveProjectPath(srcFilePath,
                                                                  GetProjectPath()));
        }

        private string GetProjectPath([CallerFilePath] string filePath = "")
        {
            var dirInfo = new DirectoryInfo(Path.GetDirectoryName(filePath)!);

            while (dirInfo.Parent != null)
            {
                if (dirInfo.EnumerateFiles("*.csproj").Any())
                    break;

                dirInfo = dirInfo.Parent;
            }

            return dirInfo.FullName;
        }
    }
}
