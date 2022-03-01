using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.ExifTSUpdater
{
    public class ExtractionConfig : IExtractionConfig
    {
        public string MediaDirectory { get; set; } = Directory.GetCurrentDirectory();
        public bool ScanSubfolders { get; set; }

        public InfoToReport InfoToReport { get; set; } = InfoToReport.Nothing;
        public bool ReportChanges => (InfoToReport & InfoToReport.AllTimestamps) != 0;
        public bool ReportTags => InfoToReport != InfoToReport.Nothing;

        //public List<FileChangeInfo> Changes { get; } = new();
    }
}
