using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.ExifTSUpdater
{
    public enum ScanStatus
    {
        NotScanned,
        Okay,
        [Description("Metadata subdirectory not found")]
        MetaDataSubDirectoryNotFound,
        [Description("Date/time taken not found")]
        DateTimeTagNotFound,
        [Description("Could not parse date/time taken")]
        DateTimeParsingFailed,
        [Description("Exception encountered on scan")]
        ExceptionOnScan
    }

    public static class ScanStatusExtensions
    {
        public static string GetDescription( this ScanStatus status )
        {
            var enumFieldName = typeof( ScanStatus ).GetEnumName( status )!;
            var enumField = typeof( ScanStatus ).GetField( enumFieldName );

            if( enumField == null )
                return enumFieldName!;

            return enumField.GetCustomAttribute<DescriptionAttribute>( false )
                                    ?.Description
                           ?? enumFieldName;
        }
    }
}
