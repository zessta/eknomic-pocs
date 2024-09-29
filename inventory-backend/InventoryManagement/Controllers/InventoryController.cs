using InventoryManagement.Models;
using InventoryManagement.Models.Entities;
using InventoryManagement.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryController(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        // Get all inventory items
        [HttpGet]
        public async Task<IActionResult> GetAllInventoryItems()
        {
            var items = await _inventoryRepository.GetAllInventoryItemsAsync();
            return Ok(items);
        }

        // Add a new inventory item
        [HttpPost]
        public async Task<IActionResult> AddInventoryItem([FromBody] InventoryItemDto itemDto, int warehouseId, int quantity)
        {
            // Add the inventory item and associate it with the warehouse
            var item = await _inventoryRepository.AddInventoryItemAsync(itemDto, warehouseId, quantity);
            if (item == null)
                return BadRequest("Failed to add inventory item.");

            return Ok(item);
        }

        // Update an existing inventory item
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventoryItem(int id, [FromBody] InventoryItemDto itemDto)
        {
            var item = await _inventoryRepository.UpdateInventoryItemAsync(id, itemDto);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // Delete an inventory item
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryItem(int id)
        {
            var success = await _inventoryRepository.DeleteInventoryItemAsync(id);
            if (!success)
                return NotFound();

            return NoContent(); // 204 No Content
        }

        // Update inventory quantity for a specific item in a warehouse
        [HttpPut("{warehouseId}/inventoryitem/{inventoryItemId}")]
        public async Task<IActionResult> UpdateInventoryQuantity(int warehouseId, int inventoryItemId, [FromBody] int quantity)
        {
            var success = await _inventoryRepository.UpdateInventoryQuantityAsync(warehouseId, inventoryItemId, quantity);
            if (!success)
                return NotFound();

            return NoContent(); // 204 No Content
        }

        // Get all inventory items for a specific warehouse
        [HttpGet("{warehouseId}/inventoryitems")]
        public async Task<IActionResult> GetInventoryByWarehouse(int warehouseId)
        {
            var items = await _inventoryRepository.GetInventoryByWarehouseAsync(warehouseId);
            return Ok(items);
        }

        // Get total inventory across all warehouses
        [HttpGet("total")]
        public async Task<IActionResult> GetTotalInventory()
        {
            var totalInventory = await _inventoryRepository.GetTotalInventoryAsync();
            return Ok(totalInventory);
        }
    }
}
