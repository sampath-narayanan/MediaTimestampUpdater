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
    [Flags]
    public enum ScanStatus
    {
        SupportedMetadataDirectoryFound = 1 << 0,
        DateTimeTagFound = 1 << 1,
        DateTimeParsed = 1 << 2,
        
        ExceptionOnScan = 1 << 31,

        NotScanned = 0,

        Valid = SupportedMetadataDirectoryFound | DateTimeTagFound | DateTimeParsed
    }
}
