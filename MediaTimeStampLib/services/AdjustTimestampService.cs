using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class AdjustTimestampService<T> : BaseTimestampService<T>
        where T : class, IFileChangeInfo, new()
    {
        public AdjustTimestampService(
            IExtractionConfig config,
            IHostApplicationLifetime lifetime,
            IJ4JLogger logger
        )
        :base( config, lifetime, logger )
        {
        }

        protected override Task Process( CancellationToken token )
        {
            foreach( var fileChange in FileChanges! )
            {
                if (fileChange.ScanStatus != ScanStatus.Valid)
                    Skipped++;
                else
                    File.SetCreationTime(fileChange.FilePath, fileChange.DateTaken!.Value);

                OnFileProcessed(fileChange.FilePath);
            }

            return Task.CompletedTask;
        }
    }
}
