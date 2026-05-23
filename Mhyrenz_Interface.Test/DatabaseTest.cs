using System;
using System.IO;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Mhyrenz_Interface.Test
{
    public abstract class DatabaseTest
    {
        protected InventoryDbContextFactory Factory { get; private set; }

        protected string ProjectRoot { get; private set; }
        protected string SeedDbPath { get; private set; }
        protected string TempDbPath { get; private set; }

        [SetUp]
        public virtual void Setup()
        {
            ProjectRoot = Directory.GetParent(
                AppDomain.CurrentDomain.BaseDirectory)
                .Parent
                .Parent
                .FullName;

            SeedDbPath = Path.Combine(
                ProjectRoot,
                "dev_inventory.db");

            TempDbPath = Path.Combine(
                ProjectRoot,
                "temp_test.db");

            File.Copy(
                SeedDbPath,
                TempDbPath,
                overwrite: true);

            Factory = new InventoryDbContextFactory(options =>
            {
                options.UseSqlite(
                    $"Data Source={TempDbPath}");
            });

            OnSetup();
        }

        [TearDown]
        public virtual void TearDown()
        {
            OnTearDown();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (File.Exists(TempDbPath))
            {
                File.Delete(TempDbPath);
            }
        }

        protected virtual void OnSetup() { }

        protected virtual void OnTearDown() { }
    }
}