using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Filters;
using InventoryManagement.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [UriParserAttribute]
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
        public async Task<IActionResult> AddWarehouse(WarehouseDto warehouse)
        {
            await _warehouseRepository.AddWarehouseAsync(warehouse);
            return Ok(warehouse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(string id)
        {
            var warehouse = await _warehouseRepository.GetWarehouseByIdAsync(id);
            if (warehouse == null)
                return NotFound();

            await _warehouseRepository.DeleteWarehouseAsync(id);

            return Ok();
        }
    }
}
