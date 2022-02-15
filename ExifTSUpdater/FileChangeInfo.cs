using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.ExifTSUpdater
{
    public class FileChangeInfo
    {
        public FileChangeInfo(
            string filePath
        )
        {
            FilePath = filePath;
            DateCreated = File.GetCreationTime( filePath );
        }

        public string FilePath { get; }
        public DateTime DateCreated { get; }
        public DateTime? DateTaken { get; set; }
        public ScanStatus ScanStatus { get; set; } = ScanStatus.NotScanned;
    }
}
