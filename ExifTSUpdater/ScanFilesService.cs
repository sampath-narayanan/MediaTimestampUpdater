using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using MetadataExtractor.Formats.Exif;
using Microsoft.Extensions.Hosting;
using MDE = MetadataExtractor;

namespace J4JSoftware.ExifTSUpdater
{
    public class ScanFilesService : IHostedService
    {
        private readonly IAppConfig _appConfig;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IJ4JLogger _logger;

        public ScanFilesService(
            IAppConfig appConfig,
            IHostApplicationLifetime lifetime,
            IJ4JLogger logger
        )
        {
            _appConfig = appConfig;
            _lifetime = lifetime;

            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public async Task StartAsync( CancellationToken cancellationToken )
        {
            var changes = new List<FileChangeInfo>();
            string? changesFile;

            lock( _appConfig )
            {
                changesFile = _appConfig.ChangesFile;

                if( !string.IsNullOrEmpty( changesFile ) )
                {
                    if( !Path.IsPathRooted( changesFile ) )
                        changesFile = Path.Combine( Directory.GetCurrentDirectory(), changesFile );

                    if( string.IsNullOrEmpty( Path.GetExtension( changesFile ) ) )
                        changesFile = $"{changesFile}.json";
                }

                _appConfig.Extensions
                          .ForEach( x =>
                                    {
                                        var fileExt = x[ 0 ] == '.' ? $"*{x}" : $"*.{x}";

                                        changes.AddRange( Directory.EnumerateFiles( _appConfig.MediaDirectory, fileExt )
                                                                   .Select( f => new FileChangeInfo( f ) ) );
                                    } );

                for( var idx = 0; idx < changes.Count; idx++ )
                {
                    try
                    {
                        var mdDirectories = MDE.ImageMetadataReader.ReadMetadata( changes[ idx ].FilePath );

                        var subIfDir = mdDirectories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                        if( subIfDir == null )
                            changes[ idx ].ScanStatus = ScanStatus.MetaDataSubDirectoryNotFound;
                        else
                        {
                            var dtTag = subIfDir.Tags.FirstOrDefault( x => x.Name == "Date/Time Original" );

                            if( dtTag == null )
                                changes[ idx ].ScanStatus = ScanStatus.DateTimeTagNotFound;
                            else
                            {
                                if( DateTime.TryParseExact( dtTag.Description,
                                                           "yyyy:MM:dd HH:mm:ss",
                                                           CultureInfo.InvariantCulture,
                                                           DateTimeStyles.None,
                                                           out var dtTaken ) )
                                {
                                    changes[ idx ].DateTaken = dtTaken;
                                    changes[ idx ].ScanStatus = ScanStatus.Okay;
                                }
                                else
                                    changes[ idx ].ScanStatus = ScanStatus.DateTimeParsingFailed;
                            }
                        }
                    }
                    catch( Exception )
                    {
                        changes[ idx ].ScanStatus = ScanStatus.ExceptionOnScan;
                    }

                    Console.Write( $"Scanned {( idx + 1 ):n0} of {changes.Count:n0} files\r" );
                }

                _appConfig.Changes.Clear();
                _appConfig.Changes.AddRange( changes );
            }

            if( !string.IsNullOrEmpty(changesFile) )
            {
                await using var fileStream =
                    File.Create( changesFile );

                await JsonSerializer.SerializeAsync( fileStream,
                                                    changes,
                                                    new JsonSerializerOptions() { WriteIndented = true },
                                                    cancellationToken );
            }

            Console.WriteLine($"\n\n{changes.Count:n0} files scanned");

            foreach( var scanStatus in new[]
                                       {
                                           ScanStatus.ExceptionOnScan,
                                           ScanStatus.MetaDataSubDirectoryNotFound,
                                           ScanStatus.DateTimeTagNotFound,
                                           ScanStatus.DateTimeParsingFailed
                                       } )
            {
                Console.WriteLine($"\t{scanStatus.GetDescription()}: {changes.Count(x => x.ScanStatus == scanStatus):n0}");
            }

            if( !string.IsNullOrEmpty( changesFile ) )
                Console.WriteLine( $"\nScan results written to {changesFile}" );

            _lifetime.StopApplication();
        }

        public Task StopAsync( CancellationToken cancellationToken )
        {
            _lifetime.StopApplication();

            return Task.CompletedTask;
        }
    }
}
