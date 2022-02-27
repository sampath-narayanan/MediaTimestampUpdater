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
        private readonly IExtractionConfig _config;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IJ4JLogger _logger;

        public AdjustCreationDTService(
            IExtractionConfig config,
            IHostApplicationLifetime lifetime,
            IJ4JLogger logger
        )
        {
            _config = config;
            _lifetime = lifetime;

            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public Task StartAsync( CancellationToken cancellationToken )
        {
            lock( _config )
            {
                Console.WriteLine("\nAdjusting file creation timestamps...\n");

                var skipped = 0;

                for (var idx = 0; idx < _config.Changes.Count; idx++)
                {
                    var curChange = _config.Changes[ idx ];

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
