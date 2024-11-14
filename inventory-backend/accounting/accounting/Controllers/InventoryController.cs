using accounting.DTOs;
using accounting.Enums;
using accounting.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace accounting.Controllers
{
    [Route("api/inventory")]
    [ApiController]
    public class InventoryController(IInventoryService inventoryService) : ControllerBase
    {
        private readonly IInventoryService _inventoryService = inventoryService;

        //AddStock  --> from source to store
        //    insert master transaction
        //    source to transit
        //        Load to transit
        //    Unload from transit
        //        transit to store

        //TransferStock --> from one store to another
        //    store to transit
        //        Load to transit
        //    Unload from transit
        //        transit to store

        //ReturnStock --> from one store to another
        //    store to transit
        //        Load to transit
        //    Unload from transit
        //        transit to store

        [HttpPost("source-transfer")]
        public IActionResult SourceTransfer(TransferRequestDTO transferRequest)
        {
            return Ok(_inventoryService.InitiateStockTransfer(transferRequest, TransactionDescription.Source_Distribution));
        }

        [HttpPost("transfer")]
        public IActionResult StockTransfer(TransferRequestDTO transferRequest)
        {
            return Ok(_inventoryService.InitiateStockTransfer(transferRequest, TransactionDescription.Transfer_Out));
        }

        [HttpPost("return")]
        public IActionResult StockReturn(TransferRequestDTO returnRequest)
        {
            return Ok(_inventoryService.InitiateStockTransfer(returnRequest, TransactionDescription.Return));
        }
        
        [HttpPost("inbound")]
        public IActionResult StockInBound(Guid masterTransactionId)
        {
            return Ok(_inventoryService.DeliverStock(masterTransactionId));
        }
    }
}
