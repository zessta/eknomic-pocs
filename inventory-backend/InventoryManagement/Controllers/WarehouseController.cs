using InventoryManagement.Models;
using InventoryManagement.Models.Entities;
using InventoryManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseController(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWarehouses()
        {
            var warehouses = await _warehouseRepository.GetAllWarehousesAsync();
            var warehouseDtos = warehouses.Select(w => new WarehouseDto
            {
                WarehouseId = w.WarehouseId,
                Location = w.Location,
                InventoryItems = w.WarehouseInventories.Select(wi => new InventoryItemDto
                {
                    Id = wi.InventoryItem.Id,
                    Name = wi.InventoryItem.Name,
                    Brand = wi.InventoryItem.Brand,
                    Price = wi.InventoryItem.Price
                }).ToList()
            }).ToList();

            return Ok(warehouseDtos);
        }

        [HttpPost]
        public async Task<IActionResult> AddWarehouse(WarehouseDto warehouseDto)
        {
            var warehouse = new Warehouse
            {
                Location = warehouseDto.Location,
                Manager = warehouseDto.Manager
            };

            await _warehouseRepository.AddWarehouseAsync(warehouse);

            return Ok(warehouse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var warehouse = await _warehouseRepository.GetWarehouseByIdAsync(id);
            if (warehouse == null)
                return NotFound();

            await _warehouseRepository.DeleteWarehouseAsync(id);

            return Ok();
        }
    }
}
