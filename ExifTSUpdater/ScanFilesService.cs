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
using MetadataExtractor.Formats.QuickTime;
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
            bool errorsOnly;

            lock( _appConfig )
            {
                changesFile = _appConfig.ChangesFile;
                errorsOnly = _appConfig.ErrorsOnly;

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

                        changes[idx].ScanStatus = GetTimestampTag( changes[ idx ].FilePath, out var timestamp );

                        if(changes[idx].ScanStatus == ScanStatus.Okay)
                            changes[idx].DateTaken = timestamp;
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
                                                    errorsOnly ? changes.Where(x=>x.ScanStatus != ScanStatus.Okay) : changes,
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

        private ScanStatus GetTimestampTag( string filePath, out DateTime timestamp )
        {
            timestamp = DateTime.MinValue;

            var mdDirectories = MDE.ImageMetadataReader.ReadMetadata(filePath);

            var exifDir = mdDirectories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if(exifDir != null )
                return GetExifTimestampTag(exifDir, out timestamp );

            var qtDir = mdDirectories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
            return qtDir != null 
                       ? GetQtTimestampTag( qtDir, out timestamp ) 
                       : ScanStatus.MetaDataSubDirectoryNotFound;
        }

        private ScanStatus GetExifTimestampTag( ExifSubIfdDirectory directory, out DateTime timestamp )
        {
            timestamp = DateTime.MinValue;

            var dtTag = directory.Tags.FirstOrDefault( x => x.Name == "Date/Time Original" );

            if( dtTag == null )
                return ScanStatus.DateTimeTagNotFound;

            if( !DateTime.TryParseExact( dtTag.Description,
                                        "yyyy:MM:dd HH:mm:ss",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out var dtTaken ) )
                return ScanStatus.DateTimeParsingFailed;

            timestamp = dtTaken;

            return ScanStatus.Okay;
        }

        private ScanStatus GetQtTimestampTag(QuickTimeMovieHeaderDirectory directory, out DateTime timestamp)
        {
            timestamp = DateTime.MinValue;

            var dtTag = directory.Tags.FirstOrDefault(x => x.Name == "Created");

            if( dtTag == null || string.IsNullOrEmpty( dtTag.Description ) )
                return ScanStatus.DateTimeTagNotFound;

            var parts = dtTag.Description.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
            if( parts.Length != 5)
                return ScanStatus.DateTimeParsingFailed;

            if (!DateTime.TryParseExact(string.Join(' ', parts[1..]),
                                        "MMM dd HH:mm:ss yyyy",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out var dtTaken))
                return ScanStatus.DateTimeParsingFailed;

            timestamp = dtTaken;

            return ScanStatus.Okay;
        }

        public Task StopAsync( CancellationToken cancellationToken )
        {
            _lifetime.StopApplication();

            return Task.CompletedTask;
        }
    }
}
