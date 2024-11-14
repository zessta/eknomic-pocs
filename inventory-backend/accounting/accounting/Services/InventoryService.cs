using accounting.DTOs;
using accounting.Entities;
using accounting.Enums;
using accounting.Repositories.Interfaces;
using accounting.Services.Interfaces;

namespace accounting.Services
{
    public class InventoryService(IInventoryRepository inventoryRepository) : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository = inventoryRepository;

        public async Task<bool> InitiateStockTransfer(TransferRequestDTO transferRequest,TransactionDescription description)
        {
            var (masterTransaction, fromAccount) = await _inventoryRepository.InsertMasterTransaction(transferRequest);
            var transitAccount = await _inventoryRepository.GetTransitAccount(fromAccount.ProductId);

            return await ProcessFromTransaction(masterTransaction.MasterTransactionId, fromAccount, transitAccount, description, transferRequest.TransactionQuantity);
        }

        private async Task<bool> ProcessFromTransaction(Guid masterTransactionId, AccountDTO fromAccount, AccountDTO transitAccount, TransactionDescription description, int quantity)
        {
            var transactionId = Guid.NewGuid().ToString();
            var fromTransaction = new TransactionDTO
            {
                EntryId = Guid.NewGuid(),
                AccountId = fromAccount.AccountId,
                AccountType = fromAccount.AccountType,
                Description = description,
                TransferType = TransferType.DR,
                TransactionId = transactionId,
                MasterTransactionId = masterTransactionId,
                ClosingBalance = fromAccount.AvailableQuantity - quantity
            };

            var transitTransaction = new TransactionDTO
            {
                EntryId = Guid.NewGuid(),
                AccountId = transitAccount.AccountId,
                AccountType = transitAccount.AccountType,
                Description = description,
                TransferType = TransferType.CR,
                TransactionId = transactionId,
                MasterTransactionId = masterTransactionId,
                ClosingBalance = transitAccount.AvailableQuantity + quantity
            };

            return await _inventoryRepository.InitiateFromTransaction(fromTransaction, transitTransaction);
        }
        
        public async Task<bool> DeliverStock(Guid masterTransactionId)
        {
            var(masterTransaction, toAccount) = await _inventoryRepository.GetMasterTransaction(masterTransactionId);
            var transitAccount = await _inventoryRepository.GetTransitAccount(toAccount.ProductId);


            return false;
        }

        private async Task<bool> ProcessToTransaction(AccountDTO toAccount, AccountDTO transitAccount, MasterTransactionDTO masterTransaction)
        {
            var transactionId = Guid.NewGuid().ToString();
            var toTransaction = new TransactionDTO
            {
                EntryId = Guid.NewGuid(),
                AccountId = toAccount.AccountId,
                AccountType = toAccount.AccountType,
                Description = TransactionDescription.Transfer_In,
                TransferType = TransferType.CR,
                TransactionId = transactionId,
                MasterTransactionId = masterTransaction.MasterTransactionId,
                ClosingBalance = toAccount.AvailableQuantity + masterTransaction.TransactionQuantity
            };

            var transitTransaction = new TransactionDTO
            {
                EntryId = Guid.NewGuid(),
                AccountId = transitAccount.AccountId,
                AccountType = transitAccount.AccountType,
                Description = TransactionDescription.Transfer_In,
                TransferType = TransferType.DR,
                TransactionId = transactionId,
                MasterTransactionId = masterTransaction.MasterTransactionId,
                ClosingBalance = transitAccount.AvailableQuantity - masterTransaction.TransactionQuantity
            };

            return await _inventoryRepository.InitiateToTransaction(toTransaction, transitTransaction);
        }
    }
}
