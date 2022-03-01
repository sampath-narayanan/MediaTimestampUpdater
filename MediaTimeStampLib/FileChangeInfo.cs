using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.ExifTSUpdater
{
    public class FileChangeInfo : IFileChangeInfo
    {
        private string _filePath = string.Empty;

        public string FilePath
        {
            get => _filePath;

            set
            {
                if( !File.Exists( value ) )
                    return;

                _filePath = value;
                DateCreated = File.GetCreationTime(FilePath);
            }
        }

        public DateTime DateCreated { get; set; }
        public DateTime? DateTaken { get; set; }
        public ScanStatus ScanStatus { get; set; } = ScanStatus.NotScanned;
        public string? ExtractorName { get; set; }
        public List<TagInfo> Tags { get; } = new();
    }
}
