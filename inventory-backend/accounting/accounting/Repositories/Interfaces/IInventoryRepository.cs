using accounting.DTOs;
using accounting.Enums;

namespace accounting.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        Task<(MasterTransactionDTO, AccountDTO)> InsertMasterTransaction(MasterTransactionDTO masterTransferRequest);
        Task<AccountDTO> GetTransitAccount(Guid productId);
        Task<bool> InitiateFromTransaction(TransactionDTO fromTransaction, TransactionDTO transitTransaction);
        Task<(MasterTransactionDTO, AccountDTO)> GetMasterTransaction(Guid masterTransactionId);
        Task<bool> InitiateToTransaction(TransactionDTO toTransactionDTO, TransactionDTO transitTransaction);
        Task<(AccountDTO, AccountDTO)> GetAccounts(TransferRequestDTO transitRequest);
        Task<AccountDTO> CreateAccount(Guid siteId, Guid productId, AccountType accountType);
    }
}
