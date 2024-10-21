using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Raven.Entities;
using InventoryManagement.Infrastructure.Data.Raven;
using InventoryManagement.Infrastructure.Repositories.Interfaces;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;


namespace InventoryManagement.Infrastructure.Repositories.Raven
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly RavenDbContext _context;
        public InventoryRepository(RavenDbContext context) 
        {
            _context = context;
        }
        public async Task<string> AddInventoryItemAsync(InventoryDto itemDto, string warehouseId, int quantity)
        {
            using var session = _context.AsyncSession;
            
                // Create new ItemClassification document
                var itemClassification = new ItemClassification()
                {
                    Category = itemDto.Category,
                    Segment = itemDto.Segment,
                    Type = itemDto.ClassificationType
                };
                await session.StoreAsync(itemClassification);

                // Create new Packaging document
                var packaging = new Packaging()
                {
                    Type = itemDto.PackagingType,
                    QuantityPerPackage = itemDto.QuantityPerPackage
                };
                await session.StoreAsync(packaging);

                // Create new OriginDetails document
                var originDetails = new OriginDetails()
                {
                    CountryOfOrigin = itemDto.CountryOfOrigin,
                    ManufacturerDetails = itemDto.ManufacturerDetails,
                    ManufacturerName = itemDto.ManufacturerName,
                    SupplierContact = itemDto.SupplierContact,
                    SupplierName = itemDto.SupplierName
                };
                await session.StoreAsync(originDetails);

                // Create new Dimensions document
                var dimensions = new Dimensions()
                {
                    Height = itemDto.Height,
                    Length = itemDto.Length,
                    Weight = itemDto.Weight,
                    Width = itemDto.Width
                };
                await session.StoreAsync(dimensions);

                // Create new ExpiryDetails document
                var expiryDetails = new ExpiryDetails()
                {
                    ExpiryDate = itemDto.ExpiryDate,
                    ManufacturingDate = itemDto.ManufacturingDate
                };
                await session.StoreAsync(expiryDetails);

                // Save all the above documents to get their IDs
                await session.SaveChangesAsync();

                // Create new InventoryItem with reference IDs
                var inventoryItem = new InventoryItem()
                {
                    Brand = itemDto.Brand,
                    Name = itemDto.Name,
                    Price = itemDto.Price,
                    Features = itemDto.Features,
                    Details = itemDto.Details,
                    ItemClassificationId = itemClassification.Id,
                    PackagingId = packaging.Id,
                    OriginDetailsId = originDetails.Id,
                    DimensionsId = dimensions.Id,
                    ExpiryDetailsId = expiryDetails.Id
                };
                await session.StoreAsync(inventoryItem);

                // Save InventoryItem and get its ID
                await session.SaveChangesAsync();

                // Associate InventoryItem with Warehouse
                var warehouseInventory = new WarehouseInventory()
                {
                    WarehouseId = warehouseId,
                    InventoryItemId = inventoryItem.Id,
                    Quantity = quantity
                };
                await session.StoreAsync(warehouseInventory);

                // Save the warehouse association
                await session.SaveChangesAsync();

                return inventoryItem.Id;
            
        }

        public async Task<bool> DeleteInventoryItemAsync(string id)
        {
            using var session = _context.AsyncSession;
            // Load the InventoryItem by its ID
            var inventoryItem = await session.LoadAsync<InventoryItem>(id);
            if (inventoryItem == null)
            {
                return false;
            }

            // Remove the InventoryItem by deleting it from the session
            session.Delete(inventoryItem);

            // Save the changes to apply the deletion
            await session.SaveChangesAsync();
            return true;
        }

        public async Task<List<InventoryDto>> GetAllInventoryItemsAsync()
        {
            using var session = _context.Session;
            var inventoriesWithSnaps = session.Query<InventoryItem>()
                .Include(p => p.ItemClassificationId)
                .Include(p => p.PackagingId)
                .Include(p => p.OriginDetailsId)
                .Include(p => p.DimensionsId)
                .Include(p => p.ExpiryDetailsId)
                .ToList();

            var inventories = inventoriesWithSnaps.Select(iv => {
                var IC = session.Load<ItemClassification>(iv.ItemClassificationId);
                var Pkg = session.Load<Packaging>(iv.PackagingId);
                var OrgD = session.Load<OriginDetails>(iv.OriginDetailsId);
                var Dmn = session.Load<Dimensions>(iv.DimensionsId);
                var Exp = session.Load<ExpiryDetails>(iv.ExpiryDetailsId);

                return new InventoryDto()
                {
                    Id = iv.Id,
                    Brand = iv.Brand,
                    Name = iv.Name,
                    Price = iv.Price,
                    Features = iv.Features,
                    Details = iv.Details,
                    Segment = IC.Segment,
                    Category = IC.Category,
                    ClassificationType = IC.Type,
                    Height = Dmn.Height,
                    Width = Dmn.Width,
                    Length = Dmn.Length,
                    Weight = Dmn.Weight,
                    PackagingType = Pkg.Type,
                    QuantityPerPackage = Pkg.QuantityPerPackage,
                    CountryOfOrigin = OrgD.CountryOfOrigin,
                    ManufacturerName = OrgD.ManufacturerName,
                    ManufacturerDetails = OrgD.ManufacturerDetails,
                    SupplierName = OrgD.SupplierName,
                    SupplierContact = OrgD.SupplierContact,
                    ManufacturingDate = Exp.ManufacturingDate,
                    ExpiryDate = Exp.ExpiryDate
                };
            }).ToList();


            return inventories;
        }

        public async Task<List<WarehouseStockDto>> GetInventoryByWarehouseAsync(string warehouseId)
        {
            using var session = _context.Session;

            var warehouseInventory = session.Query<WarehouseInventory>().Where( x => x.WarehouseId == warehouseId).Include(x => x.WarehouseId).Include( x => x.InventoryItemId)
                .ToList();
            

            return warehouseInventory.Select(wi =>
            {
                var wh = session.Load<Warehouse>(wi.WarehouseId.ToString());
                var inv = session.Load<InventoryItem>(wi.InventoryItemId.ToString());
                return new WarehouseStockDto
                {
                    WarehouseInventoryId = wi.Id,
                    InventoryItem = new InventoryDto
                    {
                        Id = wi.InventoryItemId,
                        Brand = inv.Brand,
                        Name = inv.Name,
                        Price = inv.Price,
                        Features = inv.Features,
                        Details = inv.Details
                        //Segment = wi.InventoryItem.ItemClassification.Segment,
                        //Category = wi.InventoryItem.ItemClassification.Category,
                        //ClassificationType = wi.InventoryItem.ItemClassification.Type,
                        //Height = wi.InventoryItem.Dimensions.Height,
                        //Width = wi.InventoryItem.Dimensions.Width,
                        //Length = wi.InventoryItem.Dimensions.Length,
                        //Weight = wi.InventoryItem.Dimensions.Weight,
                        //PackagingType = wi.InventoryItem.Packaging.Type,
                        //QuantityPerPackage = wi.InventoryItem.Packaging.QuantityPerPackage,
                        //CountryOfOrigin = wi.InventoryItem.OriginDetails.CountryOfOrigin,
                        //ManufacturerName = wi.InventoryItem.OriginDetails.ManufacturerName,
                        //ManufacturerDetails = wi.InventoryItem.OriginDetails.ManufacturerDetails,
                        //SupplierName = wi.InventoryItem.OriginDetails.SupplierName,
                        //SupplierContact = wi.InventoryItem.OriginDetails.SupplierContact,
                        //ManufacturingDate = wi.InventoryItem.ExpiryDetails.ManufacturingDate,
                        //ExpiryDate = wi.InventoryItem.ExpiryDetails.ExpiryDate
                    },
                    Quantity = wi.Quantity
                };
            }).ToList();

        }

        public async Task<List<TotalInventoryDto>> GetTotalInventoryAsync()
        {
            using var session = _context.AsyncSession;
            var warehouseInventories = await session
            .Query<WarehouseInventory>()
            .ToListAsync();

            // Perform client-side grouping and aggregation
            var totalInventory = warehouseInventories
                .GroupBy(wi => wi.InventoryItemId)
                .Select(g => new TotalInventoryDto
                {
                    InventoryItemId = g.Key,
                    TotalQuantity = g.Sum(wi => wi.Quantity)
                })
                .ToList();

            return totalInventory;
        }

        public async Task<bool> UpdateInventoryItemAsync(string id, InventoryDto itemDto)
        {
            using var session = _context.AsyncSession;
            var inventory = await session.LoadAsync<InventoryItem>(id);
            var inventoryItem = new InventoryItem
            {
                Id = id,
                Name = itemDto.Name,
                Brand = itemDto.Brand,
                Price = itemDto.Price,
                Features = itemDto.Features,
                Details = itemDto.Details,
                ItemClassificationId = inventory.ItemClassificationId,
                PackagingId = inventory.PackagingId,
                OriginDetailsId = inventory.OriginDetailsId,
                DimensionsId = inventory.DimensionsId,
                ExpiryDetailsId = inventory.ExpiryDetailsId
            };

            // Store the new InventoryItem object (this will update the existing document)
            await session.StoreAsync(inventoryItem);

            // Update related entities in a similar manner
            if (!string.IsNullOrEmpty(inventory.ItemClassificationId))
            {
                await session.StoreAsync(new ItemClassification
                {
                    Id = inventory.ItemClassificationId,
                    Category = itemDto.Category,
                    Segment = itemDto.Segment,
                    Type = itemDto.ClassificationType
                });
            }

            if (!string.IsNullOrEmpty(inventory.PackagingId))
            {
                await session.StoreAsync(new Packaging
                {
                    Id = inventory.PackagingId,
                    QuantityPerPackage = itemDto.QuantityPerPackage,
                    Type = itemDto.PackagingType
                });
            }

            if (!string.IsNullOrEmpty(inventory.OriginDetailsId))
            {
                await session.StoreAsync(new OriginDetails
                {
                    Id = inventory.OriginDetailsId,
                    CountryOfOrigin = itemDto.CountryOfOrigin,
                    ManufacturerDetails = itemDto.ManufacturerDetails,
                    ManufacturerName = itemDto.ManufacturerName,
                    SupplierContact = itemDto.SupplierContact,
                    SupplierName = itemDto.SupplierName
                });
            }

            if (!string.IsNullOrEmpty(inventory.DimensionsId))
            {
                await session.StoreAsync(new Dimensions
                {
                    Id = inventory.DimensionsId,
                    Height = itemDto.Height,
                    Width = itemDto.Width,
                    Length = itemDto.Length,
                    Weight = itemDto.Weight
                });
            }

            if (!string.IsNullOrEmpty(inventory.ExpiryDetailsId))
            {
                await session.StoreAsync(new ExpiryDetails
                {
                    Id = inventory.ExpiryDetailsId,
                    ExpiryDate = itemDto.ExpiryDate,
                    ManufacturingDate = itemDto.ManufacturingDate
                });
            }

            // Save all changes to the session
            await session.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateInventoryQuantityAsync(string warehouseId, string inventoryItemId, int quantity)
        {
            using var session = _context.AsyncSession;
            // Query to find the WarehouseInventory document by WarehouseId and InventoryItemId
            var warehouseInventory = await session
                .Query<WarehouseInventory>()
                .FirstOrDefaultAsync(wi => wi.WarehouseId == warehouseId && wi.InventoryItemId == inventoryItemId);

            // If no document is found, return false
            if (warehouseInventory == null)
            {
                return false;
            }

            // Update the quantity
            warehouseInventory.Quantity = quantity;

            // Save the changes back to RavenDB
            await session.SaveChangesAsync();

            return true;
        }
    }
}
