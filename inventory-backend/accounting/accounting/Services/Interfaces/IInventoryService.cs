using accounting.DTOs;
using accounting.Enums;

namespace accounting.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<bool> InitiateStockTransfer(TransferRequestDTO transferRequest, TransactionDescription description);
        Task<bool> DeliverStock(Guid masterTransactionId);
    }
}
