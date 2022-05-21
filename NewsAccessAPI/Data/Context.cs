using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewsAccessAPI.Data.Entities;
using System;

namespace NewsAccessAPI.Data
{
    public class Context : IdentityDbContext<User>
    {

        public DbSet<Categori> categoris { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<SourceBlackListItem> sourceBlackList { get; set; }
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "root",
                NormalizedName = "ROOT"
            });

            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "user",
                NormalizedName = "USER"
            });
        }
    }
}
