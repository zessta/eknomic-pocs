using InventoryManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<ItemClassification> ItemClassifications { get; set; }
        public DbSet<Packaging> Packagings { get; set; }
        public DbSet<OriginDetails> OriginDetails { get; set; }
        public DbSet<Dimensions> Dimensions { get; set; }
        public DbSet<ExpiryDetails> ExpiryDetails { get; set; }
        public DbSet<InventoryStatus> InventoriesStatus { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseInventory> WarehouseInventories { get; set; }
        public DbSet<ItemMovement> ItemMovements { get; set; }

        public void SeedData()
        {
            // Seed Packaging
            if (!Packagings.Any())
            {
                Packagings.AddRange(new List<Packaging>
                {
                    new Packaging { Id = 1, Type = "Single", QuantityPerPackage = 1 },
                    new Packaging { Id = 2, Type = "Carton", QuantityPerPackage = 12 },
                    new Packaging { Id = 3, Type = "Crate", QuantityPerPackage = 24 }
                });
            }

            // Seed OriginDetails
            if (!OriginDetails.Any())
            {
                OriginDetails.AddRange(new List<OriginDetails>
                {
                    new OriginDetails { Id = 1, CountryOfOrigin = "USA", ManufacturerName = "Apple Inc.", ManufacturerDetails = "Technology Company", SupplierName = "Global Supplier", SupplierContact = "supplier@example.com" },
                    new OriginDetails { Id = 2, CountryOfOrigin = "South Korea", ManufacturerName = "Samsung", ManufacturerDetails = "Electronics", SupplierName = "Tech Supplier", SupplierContact = "techsupplier@example.com" }
                });
            }

            // Seed Item Classifications
            if (!ItemClassifications.Any())
            {
                ItemClassifications.AddRange(new List<ItemClassification>
                {
                    new ItemClassification { Id = 1, Segment = "Electronics", Category = "Mobile Phones", Type = "Smartphone" },
                    new ItemClassification { Id = 2, Segment = "Appliances", Category = "Kitchen", Type = "Refrigerator" }
                });
            }

            // Seed Dimensions
            if (!Dimensions.Any())
            {
                Dimensions.AddRange(new List<Dimensions>
                {
                    new Dimensions { Id = 1, Height = 10m, Width = 5m, Length = 1m, Weight = 0.5m },
                    new Dimensions { Id = 2, Height = 180m, Width = 70m, Length = 80m, Weight = 100m }
                });
            }

            // Seed ExpiryDetails
            if (!ExpiryDetails.Any())
            {
                ExpiryDetails.AddRange(new List<ExpiryDetails>
                {
                    new ExpiryDetails { Id = 1, ManufacturingDate = DateTime.UtcNow, ExpiryDate = DateTime.UtcNow.AddYears(2) }, // No expiry for electronics
                    new ExpiryDetails { Id = 2, ManufacturingDate = DateTime.UtcNow, ExpiryDate = DateTime.UtcNow.AddYears(2) }
                });
            }

            // Seed Locations
            if (!Locations.Any())
            {
                Locations.AddRange(new List<Location>
                {
                    new Location { Id = 1, LocationName = "Warehouse A", Address = "123 Main St", City = "New York", Country = "USA" },
                    new Location { Id = 2, LocationName = "Warehouse B", Address = "456 Market Ave", City = "Los Angeles", Country = "USA" }
                });
            }

            // Seed InventoryItems
            if (!InventoryItems.Any())
            {
                InventoryItems.AddRange(new List<InventoryItem>
                {
                    new InventoryItem
                    {
                        Id = 1,
                        Brand = "Apple",
                        Name = "iPhone 14",
                        Price = 999.99m,
                        Features = "128GB, 6GB RAM, OLED Display",
                        Details = "Latest model with A15 Bionic chip",
                        ItemClassificationId = 1,
                        PackagingId = 1,
                        OriginDetailsId = 1,
                        DimensionsId = 1,
                        ExpiryDetailsId = 1
                    },
                    new InventoryItem
                    {
                        Id = 2,
                        Brand = "Samsung",
                        Name = "Galaxy Refrigerator",
                        Price = 1500.00m,
                        Features = "Double Door, Frost Free",
                        Details = "High energy efficiency, smart control",
                        ItemClassificationId = 2,
                        PackagingId = 2,
                        OriginDetailsId = 2,
                        DimensionsId = 2,
                        ExpiryDetailsId = 2
                    }
                });
            }

            // Seed InventoryStatus
            if (!InventoriesStatus.Any())
            {
                InventoriesStatus.AddRange(new List<InventoryStatus>
                {
                    new InventoryStatus
                    {
                        Id = 1,
                        InventoryItemId = 1,
                        LocationId = 1,
                        Status = "On Shelf",
                        StatusChangeDate = DateTime.UtcNow,
                        Quantity = 100,
                        MovedBy = "John Doe"
                    },
                    new InventoryStatus
                    {
                        Id = 2,
                        InventoryItemId = 2,
                        LocationId = 2,
                        Status = "Dispatched",
                        StatusChangeDate = DateTime.UtcNow,
                        Quantity = 50,
                        MovedBy = "Jane Smith"
                    }
                });
            }

            // Save changes
            SaveChanges();
        }

    }
}
