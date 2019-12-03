using Android.Content;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HBR.DbContext
{
    /// <summary>
    /// Temporary
    /// Will be removed after DI is added
    /// </summary>
    public static class ContextHelper
    {
        public static void DeleteDatabase(this Context c)
        {
            var databaseName = "hbrDatabase.db";
            string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), databaseName);
            var a = c.DeleteDatabase(databasePath);
        }

        public static async Task<HbrClientDbContext> CreateContextAsync(this Context c)
        {
            //c.DeleteDatabase();
            var context = (HbrClientDbContext)Activator.CreateInstance(typeof(HbrClientDbContext));
            await context.Database.EnsureCreatedAsync();
            await context.Database.MigrateAsync();

            return context;
        }
        public static HbrClientDbContext CreateContext(this Context c)
        {
            //c.DeleteDatabase("hbrDatabase.db");
            var context = (HbrClientDbContext)Activator.CreateInstance(typeof(HbrClientDbContext));
            context.Database.EnsureCreated();
            context.Database.Migrate();

            return context;
        }
    }
}