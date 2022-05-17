using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Data.Entities
{
    public class Categori
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        List<ArticleCategori> CategoriList { get; set;}
    }
}
