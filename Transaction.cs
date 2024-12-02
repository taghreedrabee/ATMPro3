using System;
using System.Collections.Generic;
using System.Linq;

namespace ATMapp
{
    internal class TransactionRepository : ITransactionRepository
    {
        private readonly AtmDbContext _context;

        public TransactionRepository(AtmDbContext context)
        {
            _context = context;
        }

        public void AddTransaction(TransactionInfo transaction)
        {
            _context.Transactions.Add(transaction);
            _context.SaveChanges();
        }

        public IEnumerable<TransactionInfo> GetTransactionsByUserId(int userId)
        {
            return _context.Transactions.Where(t => t.UserId == userId).ToList();
        }

        public IEnumerable<TransactionInfo> GetFinancialTransactions(int userId)
        {
            return _context.Transactions
                .Where(t => t.UserId == userId && t.TransactionType == "Financial")
                .ToList();
        }

        public IEnumerable<TransactionInfo> GetManagerialTransactions(int userId)
        {
            return _context.Transactions
                .Where(t => t.UserId == userId && t.TransactionType == "Managerial")
                .ToList();
        }

        public void DeleteTransaction(int transactionId)
        {
            var transaction = _context.Transactions.FirstOrDefault(t => t.TransactionId == transactionId);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                _context.SaveChanges();
            }
        }
    }
}