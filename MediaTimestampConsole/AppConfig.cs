using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.ExifTSUpdater
{
    public class AppConfig : ExtractionConfig, IAppConfig
    {
        public bool SkipChanges { get; set; }
        public bool HelpRequested { get; set; }
    }
}
