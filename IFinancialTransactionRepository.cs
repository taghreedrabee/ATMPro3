using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMapp
{
    internal interface IFinancialTransactionRepository
    {
        void AddTransaction(TransactionInfo transaction);
        IEnumerable<TransactionInfo> GetTransactionsByUserId(int userId);
        void DeleteTransaction(int transactionId);
    }
}
