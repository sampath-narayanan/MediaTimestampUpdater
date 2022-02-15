using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.J4JCommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.ExifTSUpdater
{
    public class Program
    {
        static void Main( string[] args )
        {
            var hostConfig = new J4JHostConfiguration()
                             .Publisher( "J4JSoftware" )
                             .ApplicationName( "ExifTSUpdater" )
                             .AddDependencyInjectionInitializers(SetupDependencyInjection)
                             .AddServicesInitializers(InitializeServices)
                             .FilePathTrimmer( FilePathTrimmer );

            hostConfig.AddCommandLineProcessing( CommandLineOperatingSystems.Windows )
                      .OptionsInitializer( SetupOptions );

            if( hostConfig.MissingRequirements != J4JHostRequirements.AllMet )
            {
                Console.WriteLine( $"Missing J4JHostConfiguration items: {hostConfig.MissingRequirements}" );
                Environment.ExitCode = 1;

                return;
            }

            var host = hostConfig.Build();
            if( host == null )
            {
                Console.WriteLine( "Could not create IHost" );
                Environment.ExitCode = 1;

                return;
            }

            var config = host.Services.GetRequiredService<IConfiguration>();
            if( config == null )
                throw new NullReferenceException( "Undefined IConfiguration" );

            var parsed = config.Get<AppConfig>();

            if( parsed.HelpRequested )
            {
                var help = new ColorHelpDisplay( host.CommandLineLexicalElements!, host.Options! );
                help.Display();

                return;
            }

            host.Run();
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

            options.Bind<AppConfig, string>( x => x.ChangesFile, "c" )!
                   .SetDescription( "JSON file showing changes to date created" );

            options.Bind<AppConfig, string>(x => x.MediaDirectory, "d")!
                   .SetDefaultValue(Directory.GetCurrentDirectory())
                   .SetDescription("media directory to process");

            options.Bind<AppConfig, bool>( x => x.NoChanges, "s" )!
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