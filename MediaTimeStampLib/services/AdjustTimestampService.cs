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
    public class AdjustTimestampService : IHostedService
    {
        public record Stats( int Total, int Skipped )
        {
            public int Adjusted => Total - Skipped;
        }

        public event EventHandler? Started;
        public event EventHandler? Adjusted;
        public event EventHandler<Stats>? Completed;

        private readonly IExtractionConfig _config;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IJ4JLogger _logger;

        public AdjustTimestampService(
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
                Started?.Invoke(this, EventArgs.Empty  );

                var skipped = 0;

                for (var idx = 0; idx < _config.Changes.Count; idx++)
                {
                    var curChange = _config.Changes[ idx ];

                    if( curChange.ScanStatus != ScanStatus.Valid )
                        skipped++;
                    else
                        File.SetCreationTime(curChange.FilePath, curChange.DateTaken!.Value);

                    Adjusted?.Invoke( this, EventArgs.Empty );
                }

                Completed?.Invoke( this, new Stats( _config.Changes.Count, skipped ) );
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
