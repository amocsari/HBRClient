using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HBR.Context
{
    /// <summary>
    /// Temporary
    /// Will be removed after DI is added
    /// </summary>
    public static class ContextHelper
    {
        public static async Task<HbrClientDbContext> CreateContextAsync()
        {
            var context = (HbrClientDbContext)Activator.CreateInstance(typeof(HbrClientDbContext));
            await context.Database.EnsureCreatedAsync();
            await context.Database.MigrateAsync();

            return context;
        }
        public static HbrClientDbContext CreateContext()
        {
            var context = (HbrClientDbContext)Activator.CreateInstance(typeof(HbrClientDbContext));
            context.Database.EnsureCreated();
            context.Database.Migrate();

            return context;
        }
    }
}