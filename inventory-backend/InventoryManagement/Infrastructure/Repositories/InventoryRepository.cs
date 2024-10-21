using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Domain.DTO;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Interfaces;

namespace InventoryManagement.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly InventoryDbContext _context;

        public InventoryRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryDto>> GetAllInventoryItemsAsync()
        {
            return await _context.InventoryItems.Select(iv => new InventoryDto()
            {
                Id = iv.Id.ToString(),
                Brand = iv.Brand,
                Name = iv.Name,
                Price = iv.Price,
                Features = iv.Features,
                Details = iv.Details,
                Segment = iv.ItemClassification.Segment,
                Category = iv.ItemClassification.Category,
                ClassificationType = iv.ItemClassification.Type,
                Height = iv.Dimensions.Height,
                Width = iv.Dimensions.Width,
                Length = iv.Dimensions.Length,
                Weight = iv.Dimensions.Weight,
                PackagingType = iv.Packaging.Type,
                QuantityPerPackage = iv.Packaging.QuantityPerPackage,
                CountryOfOrigin = iv.OriginDetails.CountryOfOrigin,
                ManufacturerName = iv.OriginDetails.ManufacturerName,
                ManufacturerDetails = iv.OriginDetails.ManufacturerDetails,
                SupplierName = iv.OriginDetails.SupplierName,
                SupplierContact = iv.OriginDetails.SupplierContact,
                ManufacturingDate = iv.ExpiryDetails.ManufacturingDate,
                ExpiryDate = iv.ExpiryDetails.ExpiryDate
            }).ToListAsync();
        }

        public async Task<string> AddInventoryItemAsync(InventoryDto item, string warehouseId, int quantity)
        {
            // Creating new ItemClassification
            var itemClassification = new ItemClassification()
            {
                Category = item.Category,
                Segment = item.Segment,
                Type = item.ClassificationType
            };

            // Creating new Packaging
            var packaging = new Packaging()
            {
                Type = item.PackagingType,
                QuantityPerPackage = item.QuantityPerPackage
            };

            // Creating new OriginDetails
            var originDetails = new OriginDetails()
            {
                CountryOfOrigin = item.CountryOfOrigin,
                ManufacturerDetails = item.ManufacturerDetails,
                ManufacturerName = item.ManufacturerName,
                SupplierContact = item.SupplierContact,
                SupplierName = item.SupplierName
            };

            // Creating new Dimensions
            var dimensions = new Dimensions()
            {
                Height = item.Height,
                Length = item.Length,
                Weight = item.Weight,
                Width = item.Width
            };

            // Creating new ExpiryDetails
            var expiryDetails = new ExpiryDetails()
            {
                ExpiryDate = item.ExpiryDate,
                ManufacturingDate = item.ManufacturingDate
            };

            // Add new records to the context
            _context.ItemClassifications.Add(itemClassification);
            _context.Packagings.Add(packaging);
            _context.OriginDetails.Add(originDetails);
            _context.Dimensions.Add(dimensions);
            _context.ExpiryDetails.Add(expiryDetails);
            await _context.SaveChangesAsync();

            // Creating new InventoryItem
            var inventoryItem = new InventoryItem()
            {
                Brand = item.Brand,
                Name = item.Name,
                Price = item.Price,
                Features = item.Features,
                Details = item.Details,
                ItemClassificationId = itemClassification.Id,
                PackagingId = packaging.Id,
                OriginDetailsId = originDetails.Id,
                DimensionsId = dimensions.Id,
                ExpiryDetailsId = expiryDetails.Id
            };

            // Add InventoryItem to the context
            await _context.InventoryItems.AddAsync(inventoryItem);
            await _context.SaveChangesAsync();

            // Associate InventoryItem with Warehouse
            var warehouseInventory = new WarehouseInventory()
            {
                WarehouseId = int.Parse(warehouseId),
                InventoryItemId = inventoryItem.Id,
                Quantity = quantity
            };

            // Add WarehouseInventory to the context
            await _context.WarehouseInventories.AddAsync(warehouseInventory);
            await _context.SaveChangesAsync();
            return inventoryItem.Id.ToString();
        }



        public async Task<bool> UpdateInventoryItemAsync(string id, InventoryDto itemDto)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(id);
            if (inventoryItem == null) return false;

            inventoryItem.Name = itemDto.Name;
            inventoryItem.Brand = itemDto.Brand;
            inventoryItem.Price = itemDto.Price;
            inventoryItem.Features = itemDto.Features;
            inventoryItem.Details = itemDto.Details;
            inventoryItem.ItemClassification = new ItemClassification()
            {
                Id = inventoryItem.ItemClassification.Id,
                Category = itemDto.Category,
                Segment = itemDto.Segment,
                Type = itemDto.ClassificationType
            };
            inventoryItem.Packaging = new Packaging()
            {
                Id = inventoryItem.Packaging.Id,
                QuantityPerPackage = itemDto.QuantityPerPackage,
                Type = itemDto.PackagingType
            };
            inventoryItem.OriginDetails = new OriginDetails()
            {
                Id = inventoryItem.OriginDetails.Id,
                CountryOfOrigin = itemDto.CountryOfOrigin,
                ManufacturerDetails = itemDto.ManufacturerDetails,
                ManufacturerName = itemDto.ManufacturerName,
                SupplierContact = itemDto.SupplierContact,
                SupplierName = itemDto.SupplierName
            };
            inventoryItem.Dimensions = new Dimensions()
            {
                Id = inventoryItem.Dimensions.Id,
                Height = itemDto.Height,
                Width = itemDto.Width,
                Length = itemDto.Length,
                Weight = itemDto.Weight
            };
            inventoryItem.ExpiryDetails = new ExpiryDetails()
            {
                Id = inventoryItem.ExpiryDetails.Id,
                ExpiryDate = itemDto.ExpiryDate,
                ManufacturingDate = itemDto.ManufacturingDate
            };

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteInventoryItemAsync(string id)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(int.Parse(id));
            if (inventoryItem == null) return false;

            _context.InventoryItems.Remove(inventoryItem);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateInventoryQuantityAsync(string warehouseId, string inventoryItemId, int quantity)
        {
            var warehouseInventory = await _context.WarehouseInventories
                .FirstOrDefaultAsync(wi => wi.WarehouseId == int.Parse(warehouseId) && wi.InventoryItemId == int.Parse(inventoryItemId));

            if (warehouseInventory == null) return false;

            warehouseInventory.Quantity = quantity;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<WarehouseStockDto>> GetInventoryByWarehouseAsync(string warehouseId)
        {
            return await _context.WarehouseInventories
                .Where(wi => wi.WarehouseId == int.Parse(warehouseId))
                .Select(wi => new WarehouseStockDto
                {
                    WarehouseInventoryId = wi.WarehouseInventoryId.ToString(),
                    InventoryItem = new InventoryDto
                    {
                        Id = wi.InventoryItem.Id.ToString(),
                        Brand = wi.InventoryItem.Brand,
                        Name = wi.InventoryItem.Name,
                        Price = wi.InventoryItem.Price,
                        Features = wi.InventoryItem.Features,
                        Details = wi.InventoryItem.Details,
                        Segment = wi.InventoryItem.ItemClassification.Segment,
                        Category = wi.InventoryItem.ItemClassification.Category,
                        ClassificationType = wi.InventoryItem.ItemClassification.Type,
                        Height = wi.InventoryItem.Dimensions.Height,
                        Width = wi.InventoryItem.Dimensions.Width,
                        Length = wi.InventoryItem.Dimensions.Length,
                        Weight = wi.InventoryItem.Dimensions.Weight,
                        PackagingType = wi.InventoryItem.Packaging.Type,
                        QuantityPerPackage = wi.InventoryItem.Packaging.QuantityPerPackage,
                        CountryOfOrigin = wi.InventoryItem.OriginDetails.CountryOfOrigin,
                        ManufacturerName = wi.InventoryItem.OriginDetails.ManufacturerName,
                        ManufacturerDetails = wi.InventoryItem.OriginDetails.ManufacturerDetails,
                        SupplierName = wi.InventoryItem.OriginDetails.SupplierName,
                        SupplierContact = wi.InventoryItem.OriginDetails.SupplierContact,
                        ManufacturingDate = wi.InventoryItem.ExpiryDetails.ManufacturingDate,
                        ExpiryDate = wi.InventoryItem.ExpiryDetails.ExpiryDate
                    },
                    Quantity = wi.Quantity
                })
                .ToListAsync();
        }


        public async Task<List<TotalInventoryDto>> GetTotalInventoryAsync()
        {
            return await _context.WarehouseInventories
                .GroupBy(wi => wi.InventoryItemId)
                .Select(g => new TotalInventoryDto
                {
                    InventoryItemId = g.Key.ToString(),
                    TotalQuantity = g.Sum(wi => wi.Quantity)
                })
                .ToListAsync();
        }
    }
}
