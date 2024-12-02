using System;

namespace ATMapp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var context = new AtmDbContext())
            {
                IUserRepository userRepository = new UserRepository(context);
                ITransactionRepository transactionRepository = new TransactionRepository(context);
                PendingTransfer pendingTransfers = new PendingTransfer(context);

                ATM atm = new ATM(userRepository, transactionRepository, pendingTransfers);
                atm.Start();
            }
        }
    }
}
