using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAccessAPI.Data.Entities
{
    public class User : IdentityUser
    {
        public List<Categori> Categoris { get; set; }
        public List<Country> Countries { get; set; }
        public List<SourceBlackListItem> SourceBlackList { get; set; }
    }

    public class SourceBlackListItem
    {
        [Key]
        public string Id { get; set; }
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public string SourceName { get; set; }
    }

    public class Categori
    {
        [Key]
        public string Id { get; set; }
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public string CategoriName { get; set; }
    }
    public class Country
    {
        [Key]
        public string Id { get; set; }
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public string CountryName { get; set; }
    }
}
