using InventoryManagement.Data;
using InventoryManagement.Models.Entities;
using InventoryManagement.Models;
using InventoryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly InventoryDbContext _context;

        public InventoryRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryItemDto>> GetAllInventoryItemsAsync()
        {
            return await _context.InventoryItems
                .Select(i => new InventoryItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Brand = i.Brand,
                    Price = i.Price,
                    Features = i.Features,
                    Details = i.Details,
                    ItemClassification = i.ItemClassification,
                    Packaging = i.Packaging,
                    OriginDetails = i.OriginDetails,
                    Dimensions = i.Dimensions,
                    ExpiryDetails = i.ExpiryDetails
                })
                .ToListAsync();
        }

        public async Task<InventoryItem> AddInventoryItemAsync(InventoryItemDto item, int warehouseId, int quantity)
        {
            // Creating new ItemClassification
            var itemClassification = new ItemClassification()
            {
                Category = item.ItemClassification.Category,
                Segment = item.ItemClassification.Segment,
                Type = item.ItemClassification.Type
            };

            // Creating new Packaging
            var packaging = new Packaging()
            {
                Type = item.Packaging.Type,
                QuantityPerPackage = item.Packaging.QuantityPerPackage
            };

            // Creating new OriginDetails
            var originDetails = new OriginDetails()
            {
                CountryOfOrigin = item.OriginDetails.CountryOfOrigin,
                ManufacturerDetails = item.OriginDetails.ManufacturerDetails,
                ManufacturerName = item.OriginDetails.ManufacturerName,
                SupplierContact = item.OriginDetails.SupplierContact,
                SupplierName = item.OriginDetails.SupplierName
            };

            // Creating new Dimensions
            var dimensions = new Dimensions()
            {
                Height = item.Dimensions.Height,
                Length = item.Dimensions.Length,
                Weight = item.Dimensions.Weight,
                Width = item.Dimensions.Width
            };

            // Creating new ExpiryDetails
            var expiryDetails = new ExpiryDetails()
            {
                ExpiryDate = item.ExpiryDetails.ExpiryDate,
                ManufacturingDate = item.ExpiryDetails.ManufacturingDate
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
                WarehouseId = warehouseId,
                InventoryItemId = inventoryItem.Id,
                Quantity = quantity
            };

            // Add WarehouseInventory to the context
            await _context.WarehouseInventories.AddAsync(warehouseInventory);
            await _context.SaveChangesAsync();
            return inventoryItem;
        }



        public async Task<InventoryItem> UpdateInventoryItemAsync(int id, InventoryItemDto itemDto)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(id);
            if (inventoryItem == null) return null;

            inventoryItem.Name = itemDto.Name;
            inventoryItem.Brand = itemDto.Brand;
            inventoryItem.Price = itemDto.Price;
            inventoryItem.Features = itemDto.Features;
            inventoryItem.Details = itemDto.Details;
            inventoryItem.ItemClassification = itemDto.ItemClassification;
            inventoryItem.Packaging = itemDto.Packaging;
            inventoryItem.OriginDetails = itemDto.OriginDetails;
            inventoryItem.Dimensions = itemDto.Dimensions;
            inventoryItem.ExpiryDetails = itemDto.ExpiryDetails;

            await _context.SaveChangesAsync();
            return inventoryItem;
        }

        public async Task<bool> DeleteInventoryItemAsync(int id)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(id);
            if (inventoryItem == null) return false;

            _context.InventoryItems.Remove(inventoryItem);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateInventoryQuantityAsync(int warehouseId, int inventoryItemId, int quantity)
        {
            var warehouseInventory = await _context.WarehouseInventories
                .FirstOrDefaultAsync(wi => wi.WarehouseId == warehouseId && wi.InventoryItemId == inventoryItemId);

            if (warehouseInventory == null) return false;

            warehouseInventory.Quantity = quantity;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<WarehouseInventoryDto>> GetInventoryByWarehouseAsync(int warehouseId)
        {
            return await _context.WarehouseInventories
                .Where(wi => wi.WarehouseId == warehouseId)
                .Select(wi => new WarehouseInventoryDto
                {
                    WarehouseInventoryId = wi.WarehouseInventoryId,
                    InventoryItem = new InventoryItemDto
                    {
                        Id = wi.InventoryItem.Id,
                        Name = wi.InventoryItem.Name,
                        Brand = wi.InventoryItem.Brand,
                        Price = wi.InventoryItem.Price,
                        Features = wi.InventoryItem.Features,
                        Details = wi.InventoryItem.Details,
                        ItemClassification = wi.InventoryItem.ItemClassification,
                        Packaging = wi.InventoryItem.Packaging,
                        OriginDetails = wi.InventoryItem.OriginDetails,
                        Dimensions = wi.InventoryItem.Dimensions,
                        ExpiryDetails = wi.InventoryItem.ExpiryDetails
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
                    InventoryItemId = g.Key,
                    TotalQuantity = g.Sum(wi => wi.Quantity)
                })
                .ToListAsync();
        }
    }
}
