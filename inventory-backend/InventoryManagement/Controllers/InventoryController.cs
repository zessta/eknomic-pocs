using InventoryManagement.Domain.DTO;
using InventoryManagement.Filters;
using InventoryManagement.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [UriParserAttribute]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("sku")]
        public async Task<IActionResult> GetAllInventoryItems()
        {
            var items = await _inventoryService.GetAllSKU();
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddInventoryItem([FromBody] InventoryDto itemDto, string warehouseId, int quantity)
        {
            var item = await _inventoryService.AddInventoryItemAsync(itemDto, warehouseId, quantity);
            if (item == null)
                return BadRequest("Failed to add inventory item.");

            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventoryItem(string id, [FromBody] InventoryDto itemDto)
        {
            var item = await _inventoryService.UpdateInventoryItemAsync(id, itemDto);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryItem(string id)
        {
            var success = await _inventoryService.DeleteInventoryItemAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{warehouseId}/inventoryitem/{inventoryItemId}")]
        public async Task<IActionResult> UpdateInventoryQuantity(string warehouseId, string inventoryItemId, [FromBody] int quantity)
        {
            var success = await _inventoryService.UpdateInventoryQuantityAsync(warehouseId, inventoryItemId, quantity);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{warehouseId}/inventoryitems")]
        public async Task<IActionResult> GetInventoryByWarehouse(string warehouseId)
        {
            var items = await _inventoryService.GetInventoryByWarehouseAsync(warehouseId);
            return Ok(items);
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotalInventory()
        {
            var totalInventory = await _inventoryService.GetTotalInventoryAsync();
            return Ok(totalInventory);
        }
    }
}
