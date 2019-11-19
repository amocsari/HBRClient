using HBR.Model.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace HBR.Context
{
    public class HbrClientDbContext : DbContext
    {
        private const string databaseName = "hbrDatabase.db";

        public DbSet<Book> Books { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), databaseName);
            optionsBuilder.UseSqlite($"Filename={databasePath}");
        }
    }
}