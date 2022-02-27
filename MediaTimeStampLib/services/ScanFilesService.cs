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
        private readonly IExtractionConfig _config;
        private readonly ITimestampExtractors _tsExtractors;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly List<FileChangeInfo> _changes = new();
        private readonly uint _scanStatusMask;
        private readonly IJ4JLogger _logger;

        public ScanFilesService(
            IExtractionConfig config,
            ITimestampExtractors tsExtractors,
            IHostApplicationLifetime lifetime,
            IJ4JLogger logger
        )
        {
            _config = config;
            _tsExtractors = tsExtractors;
            _lifetime = lifetime;

            for( var idx = 0; idx < 32; idx++ )
            {
                _scanStatusMask += (uint) 1 << idx;
            }

            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public async Task StartAsync( CancellationToken cancellationToken )
        {
            _changes.Clear();

            foreach( var fileExt in _tsExtractors.SupportedExtensions )
            {
                _changes.AddRange( Directory.EnumerateFiles( _config.MediaDirectory,
                                                            $"*{fileExt}",
                                                            _config.ScanSubfolders
                                                                ? SearchOption.AllDirectories
                                                                : SearchOption.TopDirectoryOnly )
                                            .Select( f => new FileChangeInfo( f ) ) );
            }

            for( var idx = 0; idx < _changes.Count; idx++ )
            {
                try
                {
                    _tsExtractors.GetTimestamp( _changes[ idx ] );
                }
                catch( Exception )
                {
                    _changes[ idx ].ScanStatus = ScanStatus.ExceptionOnScan;
                }

                Console.Write( $"Scanned {( idx + 1 ):n0} of {_changes.Count:n0} files\r" );
            }

            _config.Changes.Clear();
            _config.Changes.AddRange( _changes );

            Console.WriteLine( $"\n\n{_changes.Count:n0} files scanned" );

            ReportScanStatusCount( ScanStatus.NotScanned );
            ReportScanStatusCount( ScanStatus.SupportedMetadataDirectoryFound );
            ReportScanStatusCount( ScanStatus.DateTimeTagFound );
            ReportScanStatusCount( ScanStatus.ExceptionOnScan );

            await OutputJsonFile( cancellationToken );

            _lifetime.StopApplication();
        }

        private void ReportScanStatusCount( ScanStatus scanStatus )
        {
            var description = scanStatus switch
                              {
                                  ScanStatus.NotScanned                      => "Could not find metadata directory",
                                  ScanStatus.SupportedMetadataDirectoryFound => "Could not find date/time tag(s)",
                                  ScanStatus.DateTimeTagFound                  => "Could not parse date/time text",
                                  ScanStatus.ExceptionOnScan                 => "Exception encountered in extraction",
                                  _                                          => string.Empty
                              };

            if( string.IsNullOrEmpty( description ) )
                return;

            var numericStatus = (uint) scanStatus;
            var flippedNumericStatus = numericStatus ^ _scanStatusMask;

            var errorCount = _changes.Count(x => (x.ScanStatus & scanStatus) == scanStatus
                                                 && ((uint) x.ScanStatus & flippedNumericStatus) == 0);

            Console.WriteLine($"\t{description}: {errorCount:n0}");
        }

        private async Task OutputJsonFile( CancellationToken cancellationToken )
        {
            if( !_config.ReportChanges )
                return;

            var toReport = _config.InfoToReport switch
                           {
                               InfoToReport.AllTimestamps => _changes,
                               InfoToReport.InvalidTimestamps =>
                                   _changes.Where(x => x.ScanStatus != ScanStatus.Valid),
                               InfoToReport.ValidTimestamps => _changes.Where(x => x.ScanStatus == ScanStatus.Valid),
                               _                            => Enumerable.Empty<FileChangeInfo>()
                           };

            await using var fileStream =
                File.Create( Path.Combine( Directory.GetCurrentDirectory(), "changes.json" ) );

            await JsonSerializer.SerializeAsync( fileStream,
                                                toReport,
                                                new JsonSerializerOptions() { WriteIndented = true },
                                                cancellationToken );

            Console.WriteLine( $"\nTimestamp change results written to {fileStream.Name}" );
        }

        public Task StopAsync( CancellationToken cancellationToken )
        {
            _lifetime.StopApplication();

            return Task.CompletedTask;
        }
    }
}
