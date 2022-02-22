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
    public class AdjustCreationDTService : IHostedService
    {
        private readonly IAppConfig _appConfig;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IJ4JLogger _logger;

        public AdjustCreationDTService(
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

        public Task StartAsync( CancellationToken cancellationToken )
        {
            lock( _appConfig )
            {
                if( _appConfig.SkipChanges )
                {
                    Console.WriteLine("\nSkipping adjusting file creation timestamps");

                    _lifetime.StopApplication();
                    return Task.CompletedTask;
                }

                Console.WriteLine("\nAdjusting file creation timestamps...\n");

                var skipped = 0;

                for (var idx = 0; idx < _appConfig.Changes.Count; idx++)
                {
                    var curChange = _appConfig.Changes[ idx ];

                    if( curChange.ScanStatus != ScanStatus.Valid )
                        skipped++;
                    else
                        File.SetCreationTime(curChange.FilePath, curChange.DateTaken!.Value);

                    Console.Write($"Modified file creation timestamp on {(idx + 1 - skipped):n0} files, skipped {skipped:n0} files\r");
                }
            }

            _lifetime.StopApplication();

            return Task.CompletedTask;
        }

        public Task StopAsync( CancellationToken cancellationToken )
        {
            _lifetime.StopApplication();

            return Task.CompletedTask;
        }
    }
}
