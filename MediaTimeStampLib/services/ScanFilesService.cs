using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using Microsoft.Extensions.Hosting;
using MDE = MetadataExtractor;

namespace J4JSoftware.ExifTSUpdater
{
    public class ScanFilesService<T> : BaseTimestampService<T>
        where T : class, IFileChangeInfo, new()
    {
        private readonly ITimestampExtractors _tsExtractors;

        public ScanFilesService(
            IExtractionConfig config,
            ICollection<T> fileChanges,
            ITimestampExtractors tsExtractors,
            IHostApplicationLifetime lifetime,
            IJ4JLogger logger
        )
        :base(config, fileChanges, lifetime, logger)
        {
            _tsExtractors = tsExtractors;
        }

        protected override Task Process( CancellationToken cancellationToken )
        {
            foreach( var fileExt in _tsExtractors.SupportedExtensions )
            {
                foreach( var filePath in Directory.EnumerateFiles( Configuration.MediaDirectory,
                                                                  $"*{fileExt}",
                                                                  Configuration.ScanSubfolders
                                                                      ? SearchOption.AllDirectories
                                                                      : SearchOption.TopDirectoryOnly ) )
                {
                    FileChanges.Add( new T { FilePath = filePath } );
                }
            }

            foreach( var fileChange in FileChanges )
            {
                try
                {
                    _tsExtractors.GetTimestamp( fileChange );
                }
                catch( Exception )
                {
                    fileChange.ScanStatus = ScanStatus.ExceptionOnScan;
                }

                OnFileProcessed( fileChange.FilePath );
            }

            //ReportScanStatusCount( ScanStatus.NotScanned );
            //ReportScanStatusCount( ScanStatus.SupportedMetadataDirectoryFound );
            //ReportScanStatusCount( ScanStatus.DateTimeTagFound );
            //ReportScanStatusCount( ScanStatus.ExceptionOnScan );

            //await OutputJsonFile( cancellationToken );

            Lifetime.StopApplication();

            return Task.CompletedTask;
        }
    }
}
