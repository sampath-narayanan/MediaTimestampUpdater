using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.J4JCommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace J4JSoftware.ExifTSUpdater
{
    public class Program
    {
        private static J4JHostConfiguration _hostConfig;
        private static IJ4JHost? _host;

        static Program()
        {
            _hostConfig = new J4JHostConfiguration()
                          .Publisher("J4JSoftware")
                          .ApplicationName("ExifTSUpdater")
                          .LoggerInitializer(InitializeLogger)
                          .AddDependencyInjectionInitializers(SetupDependencyInjection)
                          .AddServicesInitializers(InitializeServices)
                          .FilePathTrimmer(FilePathTrimmer);

            _hostConfig.AddCommandLineProcessing(CommandLineOperatingSystems.Windows)
                      .OptionsInitializer(SetupOptions);

        }

        static void Main( string[] args )
        {
            if( _hostConfig.MissingRequirements != J4JHostRequirements.AllMet )
            {
                ReportLaunchFailure($"Missing J4JHostConfiguration items: {_hostConfig.MissingRequirements}" );
                return;
            }

            _host = _hostConfig.Build();
            if( _host == null )
            {
                ReportLaunchFailure( "Could not create IHost" );
                return;
            }

            var config = _host.Services.GetService<IConfiguration>();
            if( config == null )
            {
                ReportLaunchFailure("Undefined IConfiguration" );
                return;
            }

            var parsed = config.Get<AppConfig>();
            if( parsed == null )
            {
                ReportLaunchFailure("Could not parse command line");
                return;
            }

            if( parsed.HelpRequested )
            {
                var help = new ColorHelpDisplay( _host.CommandLineLexicalElements!, _host.Options! );
                help.Display();

                return;
            }

            _host.Run();
        }

        private static void ReportLaunchFailure( string mesg )
        {
            Console.WriteLine(mesg);

            if( _host != null )
            {
                var logger = _host.Services.GetRequiredService<IJ4JLogger>();
                logger?.OutputCache( _hostConfig.Logger );
            }

            Environment.ExitCode = 1;
        }

        private static void InitializeLogger( IConfiguration config, J4JLoggerConfiguration loggerConfig )
        {
            loggerConfig.SerilogConfiguration
                        .WriteTo.Console()
                        .WriteTo.File( Path.Combine( Directory.GetCurrentDirectory(), "log.txt" ),
                                      rollingInterval: RollingInterval.Day );
        }

        private static void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            builder.Register( c =>
                              {
                                  var config = c.Resolve<IConfiguration>();
                                  return config.Get<AppConfig>();
                              } )
                   .AsImplementedInterfaces()
                   .SingleInstance();

            var typeTests = new TypeTests<ITimestampExtractor>()
                            .AddTests( PredefinedTypeTests.OnlyJ4JLoggerRequired )
                            .AddTests( PredefinedTypeTests.NonAbstract );

            builder.RegisterTypeAssemblies<Program>( typeTests );

            builder.RegisterType<TimestampExtractors>()
                   .As<ITimestampExtractors>()
                   .SingleInstance();
        }

        private static void InitializeServices( HostBuilderContext hbc, IServiceCollection services )
        {
            services.AddHostedService<ScanFilesService>();
            services.AddHostedService<AdjustCreationDTService>();
        }

        private static void SetupOptions(OptionCollection options)
        {
            options.Bind<AppConfig, List<string>>(x => x.Extensions, "x")!
                   .SetDefaultValue( AppConfig.DefaultExtensions.ToList())
                   .SetDescription("media extensions to process");

            options.Bind<AppConfig, InfoToReport>( x => x.InfoToReport, "r" )!
                   .SetDescription( "information to report from file scanning (multiple flag values allowed)" );

            options.Bind<AppConfig, string>(x => x.MediaDirectory, "d")!
                   .SetDefaultValue(Directory.GetCurrentDirectory())
                   .SetDescription("media directory to process");

            options.Bind<AppConfig, bool>( x => x.SkipChanges, "s" )!
                   .SetDefaultValue( false )
                   .SetDescription( "flag to indicate no changes should actually be made (just generate the output file)" );

            options.Bind<AppConfig, bool>( x => x.HelpRequested, "h" )!
                   .SetDescription( "display help" );
        }

        // these next two methods serve to strip the project path off of source code
        // file paths
        private static string FilePathTrimmer(Type? loggedType,
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

        private static string GetProjectPath([CallerFilePath] string filePath = "")
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