﻿using Android.Content;
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
        private static HbrClientDbContext _context;

        public static void DeleteDatabase(this Context c)
        {
            var databaseName = "hbrClientDatabase.db";
            string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), databaseName);
            var a = c.DeleteDatabase(databasePath);
        }

        public static async Task<HbrClientDbContext> CreateContextAsync(this Context c)
        {
            if (_context == null)
            {
                var context = (HbrClientDbContext)Activator.CreateInstance(typeof(HbrClientDbContext));
                await context.Database.EnsureCreatedAsync();
                await context.Database.MigrateAsync();

                _context = context;
            }

            return _context;
        }
        public static HbrClientDbContext CreateContext(this Context c)
        {
            if (_context == null)
            {
                var context = (HbrClientDbContext)Activator.CreateInstance(typeof(HbrClientDbContext));
                context.Database.EnsureCreated();
                context.Database.Migrate();

                _context = context;
            }

            return _context;
        }
    }
}