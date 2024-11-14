using accounting.Data;
using accounting.DTOs;
using accounting.Entities;
using accounting.Repositories.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace accounting.Repositories
{
    public class InventoryRepository(AppDbContext context, IMapper mapper) : IInventoryRepository
    {
        private readonly AppDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<AccountDTO> GetTransitAccount(Guid productId)
        {
            var transitAccount = await _context.Accounts.Where(prop => prop.ProductId == productId).FirstOrDefaultAsync();
            return _mapper.Map<AccountDTO>(transitAccount);
        }

        public async Task<bool> InitiateFromTransaction(TransactionDTO fromTransactionDTO, TransactionDTO transitTransactionDTO)
        {
            var fromTransaction = _mapper.Map<Transaction>(fromTransactionDTO);
            var transitTransaction = _mapper.Map<Transaction>(transitTransactionDTO);
            var fromAccount = await _context.Accounts.FirstOrDefaultAsync(prop => prop.AccountId == fromTransactionDTO.AccountId);
            var transitAccount = await _context.Accounts.FirstOrDefaultAsync(prop => prop.AccountId == transitTransactionDTO.AccountId);

            fromAccount.AvailableQuantity = fromTransactionDTO.ClosingBalance;
            transitAccount.AvailableQuantity = transitTransactionDTO.ClosingBalance;

            _context.Transactions.Add(fromTransaction);
            _context.Transactions.Add(transitTransaction);
            _context.Accounts.Update(fromAccount);
            _context.Accounts.Update(transitAccount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(MasterTransactionDTO, AccountDTO)> InsertMasterTransaction(TransferRequestDTO transferRequest)
        {
            var masterTransaction = _mapper.Map<MasterTransaction>(transferRequest);
            await _context.MasterTransactions.AddAsync(masterTransaction);
            await _context.SaveChangesAsync();

            var masterTransactionDTO = _mapper.Map<MasterTransactionDTO>(masterTransaction);
            var fromAccountDTO = _mapper.Map<AccountDTO>(masterTransaction.FromAccount);
            return ( masterTransactionDTO, fromAccountDTO);
        }

        public async Task<(MasterTransactionDTO, AccountDTO)> GetMasterTransaction(Guid masterTransactionId)
        {
            var masterTransaction = await _context.MasterTransactions.FirstOrDefaultAsync(prop => prop.MasterTransactionId == masterTransactionId);
            var masterTransactionDTO = _mapper.Map<MasterTransactionDTO>(masterTransaction);
            var toAccountDTO = _mapper.Map<AccountDTO>(masterTransaction.ToAccount);
            return ( masterTransactionDTO, toAccountDTO);
        }

        public async Task<bool> InitiateToTransaction(TransactionDTO toTransactionDTO, TransactionDTO transitTransactionDTO)
        {
            var toTransaction = _mapper.Map<Transaction>(toTransactionDTO);
            var transitTransaction = _mapper.Map<Transaction>(transitTransactionDTO);
            var toAccount = await _context.Accounts.FirstOrDefaultAsync(prop => prop.AccountId == toTransactionDTO.AccountId);
            var transitAccount = await _context.Accounts.FirstOrDefaultAsync(prop => prop.AccountId == transitTransactionDTO.AccountId);

            toAccount.AvailableQuantity = toTransactionDTO.ClosingBalance;
            transitAccount.AvailableQuantity = transitTransactionDTO.ClosingBalance;

            _context.Transactions.Add(toTransaction);
            _context.Transactions.Add(transitTransaction);
            _context.Accounts.Update(toAccount);
            _context.Accounts.Update(transitAccount);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
