using InventoryManagement.Models.DTO;
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
            return Ok(warehouses);
        }

        [HttpPost]
        public async Task<IActionResult> AddWarehouse(Warehouse warehouse)
        {
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
