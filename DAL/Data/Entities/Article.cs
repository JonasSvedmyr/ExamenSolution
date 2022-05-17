using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Data.Entities
{
    public class Article
    {
        [Key]
        public string Id { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string author { get; set; }
        public List<ArticleCategori> ArticleCategoris { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string urlToImage { get; set; }
        public string publishedAt { get; set; }
        public string content { get; set; }
    }
}
