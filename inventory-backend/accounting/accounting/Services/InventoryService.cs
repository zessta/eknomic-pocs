using accounting.DTOs;
using accounting.Entities;
using accounting.Enums;
using accounting.Repositories.Interfaces;
using accounting.Services.Interfaces;
using System.Security.Principal;

namespace accounting.Services
{
    public class InventoryService(IInventoryRepository inventoryRepository) : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository = inventoryRepository;

        public async Task<bool> InitiateStockTransfer(TransferRequestDTO transferRequest,TransactionDescription description)
        {
            var(fromAccountId, toAccountId) = await VerifyAccounts(transferRequest);
            if (fromAccountId == Guid.Empty) return false;

            var masterTransferRequest = new MasterTransactionDTO
            {
                FromAccountId = fromAccountId,
                ToAccountId = toAccountId,
                TransactionQuantity = transferRequest.TransactionQuantity
            };
            var (masterTransaction, fromAccount) = await _inventoryRepository.InsertMasterTransaction(masterTransferRequest);
            var transitAccount = await _inventoryRepository.GetTransitAccount(fromAccount.ProductId);

            return await ProcessFromTransaction(masterTransaction.MasterTransactionId, fromAccount, transitAccount, description, transferRequest.TransactionQuantity);
        }

        private async Task<(Guid, Guid)> VerifyAccounts(TransferRequestDTO transferRequest)
        {
            var (fromAccount, toAccount) = await _inventoryRepository.GetAccounts(transferRequest);

            if (fromAccount is null) return (Guid.Empty, Guid.Empty);

            toAccount ??= await _inventoryRepository.CreateAccount(transferRequest.ToSiteId, transferRequest.ProductId, AccountType.Store);
            return (fromAccount.AccountId, toAccount.AccountId);
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
                ClosingBalance = fromAccount.AvailableQuantity - quantity,
                TransactionQuantity = quantity
            };

            var transitTransaction = new TransactionDTO
            {
                EntryId = Guid.NewGuid(),
                AccountId = transitAccount.AccountId,
                AccountType = transitAccount.AccountType,
                Description = TransactionDescription.Transfer_In,
                TransferType = TransferType.CR,
                TransactionId = transactionId,
                MasterTransactionId = masterTransactionId,
                ClosingBalance = transitAccount.AvailableQuantity + quantity,
                TransactionQuantity = quantity
            };

            return await _inventoryRepository.InitiateFromTransaction(fromTransaction, transitTransaction);
        }
        
        public async Task<bool> DeliverStock(Guid masterTransactionId)
        {
            var(masterTransaction, toAccount) = await _inventoryRepository.GetMasterTransaction(masterTransactionId);
            var transitAccount = await _inventoryRepository.GetTransitAccount(toAccount.ProductId);


            return await ProcessToTransaction(toAccount, transitAccount, masterTransaction);
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
                ClosingBalance = toAccount.AvailableQuantity + masterTransaction.TransactionQuantity,
                TransactionQuantity = masterTransaction.TransactionQuantity
            };

            var transitTransaction = new TransactionDTO
            {
                EntryId = Guid.NewGuid(),
                AccountId = transitAccount.AccountId,
                AccountType = transitAccount.AccountType,
                Description = TransactionDescription.Transfer_Out,
                TransferType = TransferType.DR,
                TransactionId = transactionId,
                MasterTransactionId = masterTransaction.MasterTransactionId,
                ClosingBalance = transitAccount.AvailableQuantity - masterTransaction.TransactionQuantity,
                TransactionQuantity = masterTransaction.TransactionQuantity
            };

            return await _inventoryRepository.InitiateToTransaction(toTransaction, transitTransaction);
        }
    }
}
