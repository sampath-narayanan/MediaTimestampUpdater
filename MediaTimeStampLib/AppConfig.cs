using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.ExifTSUpdater
{
    public class AppConfig : IAppConfig
    {
        public static string[] DefaultExtensions = new[] {
                                                             "jpg", "jpeg", "heic", "mov", "mp4"
                                                         };

        public List<string> Extensions { get; set; } = DefaultExtensions.ToList();
        public string MediaDirectory { get; set; } = Directory.GetCurrentDirectory();

        public InfoToReport InfoToReport { get; set; } = InfoToReport.Nothing;
        public bool ReportChanges => (InfoToReport & InfoToReport.AllTimestamps) != 0;
        public bool ReportTags => InfoToReport != InfoToReport.Nothing;

        public bool SkipChanges { get; set; }
        public bool HelpRequested { get; set; }

        public List<FileChangeInfo> Changes { get; } = new();
    }
}
