using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Data.Entities
{
    public class ArticleCategori
    {
        [Key]
        public string id { get; set; }
        public int categoriId { get; set; }
        [ForeignKey("articleId")]
        public Article article { get; set; }
    }
}
