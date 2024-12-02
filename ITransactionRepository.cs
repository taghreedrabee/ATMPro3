using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMapp
{
    internal interface ITransactionRepository
    {
        void AddTransaction(TransactionInfo transaction);
        void DeleteTransaction(int transactionId);
        IEnumerable<TransactionInfo> GetTransactionsByUserId(int userId);
        IEnumerable<TransactionInfo> GetFinancialTransactions(int userId);
        IEnumerable<TransactionInfo> GetManagerialTransactions(int userId);
    }

   

}
