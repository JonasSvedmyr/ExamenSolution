using DAL.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DAL.Data
{
    public partial class NewsContext : DbContext
    {
        public DbSet<Article> Articles { get; set; }
        public DbSet<Categori> Categoris { get; set; }

        public NewsContext()
        {

        }
        public NewsContext(DbContextOptions<NewsContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                using (StreamReader r = new StreamReader("Config.json"))
                {
                    string json = r.ReadToEnd();
                    Config config = JsonConvert.DeserializeObject<Config>(json);
                    optionsBuilder.UseSqlServer(config.ConnectionString);
                }


            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>().HasIndex(x => new {x.SourceName,x.author,x.title,x.publishedAt});
            modelBuilder.Entity<Article>().HasKey(x => x.Id);

            modelBuilder.Entity<Categori>().HasData(new Categori
            {
                Id = 1,
                Name = "business"
            });
            modelBuilder.Entity<Categori>().HasData(new Categori
            {
                Id = 2,
                Name = "entertainment"
            });
            modelBuilder.Entity<Categori>().HasData(new Categori
            {
                Id = 3,
                Name = "general"
            });
            modelBuilder.Entity<Categori>().HasData(new Categori
            {
                Id = 4,
                Name = "health"
            });
            modelBuilder.Entity<Categori>().HasData(new Categori
            {
                Id = 5,
                Name = "science"
            });
            modelBuilder.Entity<Categori>().HasData(new Categori
            {
                Id = 6,
                Name = "sports"
            });
            modelBuilder.Entity<Categori>().HasData(new Categori
            {
                Id = 7,
                Name = "technology"
            });
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
