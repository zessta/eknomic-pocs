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
            
            var itemClassification = new ItemClassification()
            {
                Category = itemDto.Category,
                Segment = itemDto.Segment,
                Type = itemDto.ClassificationType
            };
            await session.StoreAsync(itemClassification);
         
            var packaging = new Packaging()
            {
                Type = itemDto.PackagingType,
                QuantityPerPackage = itemDto.QuantityPerPackage
            };
            await session.StoreAsync(packaging);
         
            var originDetails = new OriginDetails()
            {
                CountryOfOrigin = itemDto.CountryOfOrigin,
                ManufacturerDetails = itemDto.ManufacturerDetails,
                ManufacturerName = itemDto.ManufacturerName,
                SupplierContact = itemDto.SupplierContact,
                SupplierName = itemDto.SupplierName
            };
            await session.StoreAsync(originDetails);
         
            var dimensions = new Dimensions()
            {
                Height = itemDto.Height,
                Length = itemDto.Length,
                Weight = itemDto.Weight,
                Width = itemDto.Width
            };
            await session.StoreAsync(dimensions);
         
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
            await session.SaveChangesAsync();
         
            var warehouseInventory = new WarehouseInventory()
            {
                WarehouseId = warehouseId,
                InventoryItemId = inventoryItem.Id,
                Quantity = quantity
            };
            await session.StoreAsync(warehouseInventory);
            await session.SaveChangesAsync();
         
            return inventoryItem.Id;
        }

        public async Task<bool> DeleteInventoryItemAsync(string id)
        {
            using var session = _context.AsyncSession;
            var inventoryItem = await session.LoadAsync<InventoryItem>(id);
            if (inventoryItem == null)
            {
                return false;
            }
            session.Delete(inventoryItem);
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
                var inv = session.Query<InventoryItem>().Where(x => x.Id == wi.InventoryItemId.ToString())
                .Include(p => p.ItemClassificationId)
                .Include(p => p.PackagingId)
                .Include(p => p.OriginDetailsId)
                .Include(p => p.DimensionsId)
                .Include(p => p.ExpiryDetailsId)
                .FirstOrDefault();

                var IC = session.Load<ItemClassification>(inv.ItemClassificationId);
                var Pkg = session.Load<Packaging>(inv.PackagingId);
                var OrgD = session.Load<OriginDetails>(inv.OriginDetailsId);
                var Dmn = session.Load<Dimensions>(inv.DimensionsId);
                var Exp = session.Load<ExpiryDetails>(inv.ExpiryDetailsId);

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
                        Details = inv.Details,
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

            inventory.Name = itemDto.Name;
            inventory.Brand = itemDto.Brand;
            inventory.Price = itemDto.Price;
            inventory.Features = itemDto.Features;
            inventory.Details = itemDto.Details;

            if (!string.IsNullOrEmpty(inventory.ItemClassificationId))
            {
                var itemClassification = await session.LoadAsync<ItemClassification>(inventory.ItemClassificationId);
                if (itemClassification != null)
                {
                    itemClassification.Category = itemDto.Category;
                    itemClassification.Segment = itemDto.Segment;
                    itemClassification.Type = itemDto.ClassificationType;
                }
            }

            if (!string.IsNullOrEmpty(inventory.PackagingId))
            {
                var packaging = await session.LoadAsync<Packaging>(inventory.PackagingId);
                if (packaging != null)
                {
                    packaging.QuantityPerPackage = itemDto.QuantityPerPackage;
                    packaging.Type = itemDto.PackagingType;
                }
            }

            if (!string.IsNullOrEmpty(inventory.OriginDetailsId))
            {
                var originDetails = await session.LoadAsync<OriginDetails>(inventory.OriginDetailsId);
                if (originDetails != null)
                {
                    originDetails.CountryOfOrigin = itemDto.CountryOfOrigin;
                    originDetails.ManufacturerDetails = itemDto.ManufacturerDetails;
                    originDetails.ManufacturerName = itemDto.ManufacturerName;
                    originDetails.SupplierContact = itemDto.SupplierContact;
                    originDetails.SupplierName = itemDto.SupplierName;
                }
            }

            if (!string.IsNullOrEmpty(inventory.DimensionsId))
            {
                var dimensions = await session.LoadAsync<Dimensions>(inventory.DimensionsId);
                if (dimensions != null)
                {
                    dimensions.Height = itemDto.Height;
                    dimensions.Width = itemDto.Width;
                    dimensions.Length = itemDto.Length;
                    dimensions.Weight = itemDto.Weight;
                }
            }

            if (!string.IsNullOrEmpty(inventory.ExpiryDetailsId))
            {
                var expiryDetails = await session.LoadAsync<ExpiryDetails>(inventory.ExpiryDetailsId);
                if (expiryDetails != null)
                {
                    expiryDetails.ExpiryDate = itemDto.ExpiryDate;
                    expiryDetails.ManufacturingDate = itemDto.ManufacturingDate;
                }
            }

            // Save all changes to the session
            await session.SaveChangesAsync();
            return true;

        }

        public async Task<bool> UpdateInventoryQuantityAsync(string warehouseId, string inventoryItemId, int quantity)
        {
            using var session = _context.AsyncSession;
            
            var warehouseInventory = await session
                .Query<WarehouseInventory>()
                .FirstOrDefaultAsync(wi => wi.WarehouseId == warehouseId && wi.InventoryItemId == inventoryItemId);

            if (warehouseInventory == null)
            {
                return false;
            }

            warehouseInventory.Quantity += quantity;

            await session.SaveChangesAsync();

            return true;
        }
    }
}
