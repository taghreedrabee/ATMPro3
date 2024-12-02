using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;


namespace ATMapp
{
    internal class ManagerialTransactionRepository : IManagerialTransactionRepository
    {
        private readonly AtmDbContext _context;

        public ManagerialTransactionRepository(AtmDbContext context)
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
            return _context.Transactions
                .Where(t => t.UserId == userId && t.TransactionName == "Managerial")
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
