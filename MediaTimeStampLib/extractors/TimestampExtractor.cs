using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Serilog;
using MDE = MetadataExtractor;

namespace J4JSoftware.ExifTSUpdater
{
    public abstract class TimestampExtractor<T> : ITimestampExtractor
        where T : MDE.Directory
    {
        private readonly string _dtFormat;
        private readonly List<string> _supportedExtensions;

        protected TimestampExtractor(
            string dateTimeFormat,
            IJ4JLogger logger,
            params string[] supportedExtensions
        )
        {
            _dtFormat = dateTimeFormat;

            _supportedExtensions = supportedExtensions
                                       .Where(x=>x.Length > 0  )
                                       .Select(x=>x[0] == '.' ? x : $".{x}")
                                       .ToList();

            Logger = logger;
            Logger.SetLoggedType( GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public Type MdeDirectoryType => typeof( T );
        public IReadOnlyCollection<string> SupportedExtensions => _supportedExtensions;

        public ScanInfo GetDateTime( IReadOnlyList<MDE.Directory> directories )
        {
            var retVal = new ScanInfo();

            var directory = directories.OfType<T>().FirstOrDefault();
            if( directory is null )
                return retVal;

            retVal.ScanResult |= ScanStatus.SupportedMetadataDirectoryFound;

            var dtText = GetDateTimeString( directory );
            if( dtText == null )
                return retVal;

            retVal.ScanResult |= ScanStatus.DateTimeTagFound;

            if( !DateTime.TryParseExact( dtText,
                                        _dtFormat,
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out var dtTaken ) )
                return retVal;

            retVal.ScanResult |= ScanStatus.DateTimeParsed;
            retVal.Timestamp = dtTaken;

            retVal.ExtractorName = MdeDirectoryType!.Name;

            return retVal;
        }

        protected abstract string? GetDateTimeString( T directory );

        public bool Equals( ITimestampExtractor? other )
        {
            return other != null && other.MdeDirectoryType == this.MdeDirectoryType;
        }
    }
}
