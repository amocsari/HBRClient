using HBR.Model.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Permissions;

namespace HBR.DbContext
{
    public class HbrClientDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private const string databaseName = "hbrDatabase.db";

        public DbSet<Book> Books { get; set; }
        public DbSet<Bookmark> BookMarks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), databaseName);
            optionsBuilder.UseSqlite($"Filename={databasePath}");
        }
    }
}