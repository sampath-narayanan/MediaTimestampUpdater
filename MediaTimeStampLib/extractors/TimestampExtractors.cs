using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using MDE = MetadataExtractor;

namespace J4JSoftware.ExifTSUpdater
{
    public class TimestampExtractors : ITimestampExtractors
    {
        private readonly IExtractionConfig _extractionConfig;
        private readonly List<ITimestampExtractor> _tsExtractors;
        private readonly List<string> _supportedExtensions;
        private readonly IJ4JLogger _logger;

        public TimestampExtractors(
            IExtractionConfig extractionConfig,
            IEnumerable<ITimestampExtractor> tsExtractors,
            IJ4JLogger logger
        )
        {
            _extractionConfig = extractionConfig;

            var topoList = tsExtractors.ToNodeList( logger: logger );

            if( !topoList.Sort( out var sorted, out var remainingEdges ) )
                throw new
                    ArgumentException( $"Failed to sort list of timestamp extractors. Check their PredecessorAttributes." );

            _tsExtractors = sorted!;
            _supportedExtensions = _tsExtractors.SelectMany( x => x.SupportedExtensions )
                                                .Distinct()
                                                .ToList();

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            foreach( var ext in _supportedExtensions )
            {
                _logger.Information<string>("Found date/time extractor for {0}", ext);
            }
        }

        public IReadOnlyCollection<string> SupportedExtensions => _supportedExtensions;

        public void GetTimestamp( IFileChangeInfo changeInfo )
        {
            var mdDirectories = MDE.ImageMetadataReader.ReadMetadata(changeInfo.FilePath);

            changeInfo.DoAction( x => x.ScanStatus = ScanStatus.NotScanned );
            var fileExt = Path.GetExtension( changeInfo.FilePath );

            foreach( var tsExtractor in _tsExtractors
                        .Where( x => x.SupportedExtensions
                                      .Any( y => y.Equals( fileExt, StringComparison.OrdinalIgnoreCase ) ) )
                   )
            {
                var scanInfo = tsExtractor.GetDateTime( mdDirectories );

                if( scanInfo.ScanResult > changeInfo.ScanStatus )
                    changeInfo.DoAction( x => x.ScanStatus = scanInfo.ScanResult );

                if( ( scanInfo.ScanResult & ScanStatus.Valid ) != ScanStatus.Valid )
                    continue;

                changeInfo.DoAction( x => x.DateTaken = scanInfo.Timestamp );
                changeInfo.DoAction( x => x.ExtractorName = scanInfo.ExtractorName );
                break;
            }

            if (_extractionConfig.ReportTags)
                StoreMetadataTags(changeInfo, mdDirectories);
        }

        private static void StoreMetadataTags(IFileChangeInfo changeInfo, IReadOnlyList<MDE.Directory> directories)
        {
            foreach( var tag in directories
                               .SelectMany( x => x.Tags )
                               .Where( x => x.Name.Contains( "date", StringComparison.OrdinalIgnoreCase ) ) )
            {
                var tagInfo = changeInfo.Tags
                                        .FirstOrDefault( x => x.DirectoryName
                                                               .Equals( tag.DirectoryName,
                                                                        StringComparison.OrdinalIgnoreCase )
                                                          && x.TagName
                                                              .Equals( tag.Name, StringComparison.OrdinalIgnoreCase ) );

                if( tagInfo == null )
                    changeInfo.DoAction( x => x.Tags.Add( new TagInfo( tag ) ) );
            }
        }
    }
}
