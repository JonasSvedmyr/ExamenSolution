using System.Collections.Generic;

namespace NewsAccessAPI.Models
{
    public class UserSettingsModel
    {
        public List<string> Categoris { get; set; }
        public List<string> Countries { get; set; }
        public List<string> SourceBlackList { get; set; }
    }
}
