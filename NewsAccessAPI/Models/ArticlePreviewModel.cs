using System.Collections.Generic;

namespace NewsAccessAPI.Models
{
    public class ArticlePreviewModel
    {
        public string Id { get; set; }
        public string SourceName { get; set; }
        public string author { get; set; }
        public string description { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string urlToImage { get; set; }
        public string publishedAt { get; set; }
        public List<string> Categories { get; set; }
    }
}
