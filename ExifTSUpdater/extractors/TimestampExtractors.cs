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
        private readonly IAppConfig _appConfig;
        private readonly List<ITimestampExtractor> _tsExtractors;
        private readonly IJ4JLogger _logger;

        public TimestampExtractors(
            IAppConfig appConfig,
            IEnumerable<ITimestampExtractor> tsExtractors,
            IJ4JLogger logger
        )
        {
            _appConfig = appConfig;

            var temp = tsExtractors.ToList();
            var topoList = new Nodes<ITimestampExtractor>();

            foreach( var tsExtractor in temp )
            {
                var predAttr = tsExtractor.GetType().GetCustomAttribute<PredecessorAttribute>( false );
                if( predAttr == null )
                    continue;

                if( predAttr.Predecessor == null )
                    topoList.AddIndependentNode( tsExtractor );
                else
                {
                    var predecessor = temp.FirstOrDefault( x => x.GetType() == predAttr.Predecessor );
                    if( predecessor == null )
                        throw new
                            NullReferenceException( $"Couldn't find predecessor extractor {predAttr.Predecessor.Name}" );

                    topoList.AddDependentNode( tsExtractor, predecessor );
                }
            }

            if( !topoList.Sort( out var sorted, out var remainingEdges ) )
                throw new
                    ArgumentException( $"Failed to sort list of timestamp extractors. Check their PredecessorAttributes." );

            _tsExtractors = sorted!;

            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public void GetTimestamp( FileChangeInfo changeInfo )
        {
            var mdDirectories = MDE.ImageMetadataReader.ReadMetadata(changeInfo.FilePath);

            changeInfo.ScanStatus = ScanStatus.NotScanned;

            foreach (var tsExtractor in _tsExtractors)
            {
                var scanInfo = tsExtractor.GetDateTime( mdDirectories );

                if( scanInfo.ScanResult > changeInfo.ScanStatus )
                    changeInfo.ScanStatus = scanInfo.ScanResult;

                if( ( scanInfo.ScanResult & ScanStatus.Valid ) != ScanStatus.Valid )
                    continue;

                changeInfo.DateTaken = scanInfo.Timestamp;
                changeInfo.ExtractorName = scanInfo.ExtractorName;
                break;
            }

            if (_appConfig.ReportTags)
                StoreMetadataTags(changeInfo, mdDirectories);
        }

        private void StoreMetadataTags(FileChangeInfo changeInfo, IReadOnlyList<MDE.Directory> directories)
        {
            foreach( var tag in directories.SelectMany( x => x.Tags )
                                           .Where( x => x.Name.IndexOf( "date", StringComparison.OrdinalIgnoreCase )
                                                        >= 0 ) )
            {
                var tagInfo = changeInfo.Tags
                                        .FirstOrDefault( x => x.DirectoryName.Equals( tag.DirectoryName,
                                                                            StringComparison.OrdinalIgnoreCase )
                                                     && x.TagName.Equals( tag.Name,
                                                                         StringComparison.OrdinalIgnoreCase ) );

                if( tagInfo == null )
                    changeInfo.Tags.Add( new TagInfo( tag ) );
            }
        }
    }
}
