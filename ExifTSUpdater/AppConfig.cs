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
        public string ChangesFile { get; set; } = string.Empty;
        public bool ErrorsOnly { get; set; }
        public bool NoChanges { get; set; }
        public bool HelpRequested { get; set; }

        public List<FileChangeInfo> Changes { get; } = new();
    }
}
