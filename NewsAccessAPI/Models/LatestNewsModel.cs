using System.Collections.Generic;

namespace NewsAccessAPI.Models
{
    public class LatestNewsModel
    {
        public string Categori { get; set; }
        public List<ArticlePreviewModel> Articels { get; set; }
    }
}
