using InventoryManagement.Domain.Events;
using InventoryManagement.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransitController : ControllerBase
    {
        private readonly ITransitService _transitService;
        public TransitController(ITransitService transitService) 
        {
            _transitService = transitService;
        }

        [HttpPost]
        public async Task<IActionResult> TransferInventory(TransferEvent transferEvent)
        {
            var transitResponse = await _transitService.TransitInventory(transferEvent);
            return Ok(transitResponse);
        }
    }
}
