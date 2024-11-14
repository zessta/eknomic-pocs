using accounting.DTOs;

namespace accounting.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        Task<(MasterTransactionDTO, AccountDTO)> InsertMasterTransaction(TransferRequestDTO transferRequest);
        Task<AccountDTO> GetTransitAccount(Guid productId);
        Task<bool> InitiateFromTransaction(TransactionDTO fromTransaction, TransactionDTO transitTransaction);
        Task<(MasterTransactionDTO, AccountDTO)> GetMasterTransaction(Guid masterTransactionId);
        Task<bool> InitiateToTransaction(TransactionDTO toTransactionDTO, TransactionDTO transitTransaction);
    }
}
