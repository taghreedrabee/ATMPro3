using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace ATMapp
{
    internal class ATM
    {

        
            private readonly IUserRepository _userRepository;
            private readonly ITransactionRepository _transactionRepository;
            private IUser _currentUser;
            private readonly PendingTransfer _pendingTransfers;

            public ATM(IUserRepository userRepository, ITransactionRepository transactionRepository, PendingTransfer pendingTransfers)
            {
                _userRepository = userRepository;
                _transactionRepository = transactionRepository;
                _pendingTransfers = pendingTransfers;
            }

            public void Start()
            {
                while (true)
                {
                    if (Login())
                    {
                        ShowMenu();
                    }
                    else
                    {
                        Console.WriteLine("Incorrect username or password. Exiting program...");
                        Thread.Sleep(3000);
                    }
                }
            }

            private bool Login()
            {
                Console.WriteLine("Welcome to the ATM!");
                Console.WriteLine("1- Login");
                Console.WriteLine("2- Create new account");
                Console.WriteLine("3- Exit");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        return LoginExistingUser();
                    case "2":
                        CreateNewUser();
                        return true;
                    case "3":
                        Console.WriteLine("Exiting program...");
                        Environment.Exit(0);
                        return false;
                    default:
                        Console.WriteLine("Invalid option.");
                        Thread.Sleep(2000);
                        return false;
                }
            }

            private void DeleteUser()
            {
                Console.Write("Enter username to delete: ");
                string username = Console.ReadLine();

                Console.Write("Enter password: ");
                string password = Console.ReadLine();

                IUser userToDelete = _userRepository.GetUserByUsername(username);
                if (userToDelete == null)
                {
                    Console.WriteLine("User not found.");
                    Thread.Sleep(3000);
                    return;
                }

                bool hasTransactions = _transactionRepository.GetTransactionsByUserId(userToDelete.UserId).Any();
                var sentPendingTransfers = _pendingTransfers.GetPendingTransfersBySenderId(userToDelete.UserId).ToList();
                var receivedPendingTransfers = _pendingTransfers.GetPendingTransfersByRecipientId(userToDelete.UserId).ToList();
                bool hasPendingTransfers = sentPendingTransfers.Any() || receivedPendingTransfers.Any();

                if (hasTransactions || hasPendingTransfers)
                {
                    Console.WriteLine("\nWarning: The following conditions were found:");
                    if (hasTransactions)
                        Console.WriteLine($" User '{username}' has previous transactions.");
                    if (hasPendingTransfers)
                        Console.WriteLine($" User has {sentPendingTransfers.Count + receivedPendingTransfers.Count} pending transfers.");

                    Console.Write("\nDo you want to proceed with the deletion? (y/n): ");
                    if (Console.ReadLine().ToLower() != "y")
                    {
                        Console.WriteLine("User deletion cancelled.");
                        Thread.Sleep(3000);
                        return;
                    }

                    Console.WriteLine("\nProcessing deletion request...");
                    Thread.Sleep(2000);

                    if (hasPendingTransfers)
                    {
                        foreach (var transfer in sentPendingTransfers)
                        {
                            userToDelete.Balance += transfer.Amount;
                            _pendingTransfers.DeletePendingTransfer(transfer.TransferId);

                            _transactionRepository.AddTransaction(new TransactionInfo(
                                0, userToDelete.UserId, userToDelete.Username,
                                "Transfer Cancelled - User Deletion", transfer.Amount,
                                DateTime.Now, userToDelete.Balance - transfer.Amount,
                                userToDelete.Balance, null, true, "Financial"
                            ));
                        }

                        foreach (var transfer in receivedPendingTransfers)
                        {
                            var sender = _userRepository.GetUserById(transfer.SenderId);
                            if (sender != null)
                            {
                                sender.Balance += transfer.Amount;
                                _userRepository.UpdateUser(sender);

                                _transactionRepository.AddTransaction(new TransactionInfo(
                                    0, sender.UserId, sender.Username,
                                    "Transfer Returned - Recipient Deleted", transfer.Amount,
                                    DateTime.Now, sender.Balance - transfer.Amount,
                                    sender.Balance, null, true, "Financial"
                                ));
                            }
                            _pendingTransfers.DeletePendingTransfer(transfer.TransferId);
                        }
                        Console.WriteLine("All pending transfers have been processed.");
                    }
                    _transactionRepository.AddTransaction(new TransactionInfo(
                        0, userToDelete.UserId, "System", "User Deletion",
                        0, DateTime.Now, 0, 0, null, true, "Managerial"
                    ));

                    var transactionsToDelete = _transactionRepository.GetTransactionsByUserId(userToDelete.UserId).ToList();
                    foreach (var transaction in transactionsToDelete)
                    {
                        _transactionRepository.DeleteTransaction(transaction.TransactionId);
                    }

                    if (_userRepository.DeleteUser(username, password))
                    {
                        Console.WriteLine("User deleted successfully.");
                    }
                    else
                    {
                        Console.WriteLine("User not found or incorrect password.");
                    }
                }
                else
                {
                    Console.WriteLine("No transactions or pending transfers found. Deleting user...");

                    _transactionRepository.AddTransaction(new TransactionInfo(
                        0, userToDelete.UserId, "System", "User Deletion",
                        0, DateTime.Now, 0, 0, null, true, "Managerial"
                    ));

                    if (_userRepository.DeleteUser(username, password))
                    {
                        Console.WriteLine("User deleted successfully.");
                    }
                }

                Thread.Sleep(3000);
            }
            private void CreateNewUser()
                    {
                        Console.Write("Enter a username for the new account: ");
                        string username = Console.ReadLine();

                        if (_userRepository.UserExists(username))
                        {
                            Console.WriteLine("Username already exists. Please choose a different one.");
                            return;
                        }

                        Console.Write("Enter a password for the new account: ");
                        string password = Console.ReadLine();

                        Console.Write("Enter your email: ");
                        string email = Console.ReadLine();

                        Console.Write("Enter your birthday (YYYY-MM-DD): ");
                        if (!DateTime.TryParse(Console.ReadLine(), out DateTime birthDate))
                        {
                            Console.WriteLine("Invalid date format. User creation failed.");
                            return;
                        }

                        Console.Write("Enter user type (VIP/Ordinary): ");
                        string userType = Console.ReadLine().ToUpper();
                        if (userType != "VIP" && userType != "ORDINARY")
                        {
                            Console.WriteLine("Invalid user type. User creation failed.");
                            return;
                        }

                        IUser newUser = new User
                        {
                            Username = username,
                            PasswordHash = password, 
                            Email = email,
                            BirthDate = birthDate,
                            Type = userType == "VIP" ? UserType.VIP : UserType.Ordinary
                        };

                        _userRepository.AddUser(newUser);

                        Console.WriteLine("New user account created successfully!");
                        _currentUser = newUser;
                        _transactionRepository.AddTransaction(new TransactionInfo(
                            0,
                            _currentUser.UserId,
                            _currentUser.Username,
                            "User Creation",
                            0,
                            DateTime.Now,
                            0,
                            0,
                            null,
                            true, 
                            "Managerial"
                        ));

                        Thread.Sleep(3000);
                    }

            private bool LoginExistingUser()
            {
                Console.Write("Enter Username: ");
                string username = Console.ReadLine();

                IUser user = _userRepository.GetUserByUsername(username);

                if (user != null)
                {
                    Console.Write("Enter Password: ");
                    string password = Console.ReadLine();

                    if (_userRepository.VerifyPassword(username, password))
                    {
                        _currentUser = user;
                        _transactionRepository.AddTransaction(new TransactionInfo(
                            0, 
                            _currentUser.UserId,
                            _currentUser.Username,
                            "Login",
                            0,
                            DateTime.Now,
                            0,
                            0,
                            null,
                            true,
                            "Managerial"
                        ));
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect password.");
                        Thread.Sleep(2000);
                    }
                }
                else
                {
                    Console.WriteLine("User does not exist. Would you like to create a new account? (y/n)");
                    string choice = Console.ReadLine();

                    if (choice.ToLower() == "y")
                    {
                        CreateNewUser();
                        return true;
                    }
                }

                return false;
            }

            private void UpdateUserInfo()
            {
                Console.Write("Enter username: ");
                string username = Console.ReadLine();
                Console.Write("Enter current password: ");
                string currentPassword = Console.ReadLine();

                if (_userRepository.VerifyPassword(username, currentPassword))
                {
                    IUser user = _userRepository.GetUserByUsername(username);
                    if (user != null)
                    {
                        Console.Write("Enter new email (press enter to keep current): ");
                        string newEmail = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(newEmail))
                        {
                            user.Email = newEmail;
                        }

                        Console.Write("Enter new password (press enter to keep current): ");
                        string newPassword = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(newPassword))
                        {
                            user.PasswordHash = newPassword; 
                        }

                        _userRepository.UpdateUser(user);
                        Console.WriteLine("User information updated successfully.");
                        _transactionRepository.AddTransaction(new TransactionInfo(
                            0, 
                            user.UserId,
                            user.Username,
                            "User Info Update",
                            0,
                            DateTime.Now,
                            0,
                            0,
                            null,
                            true,
                            "Managerial"
                        ));
                    }
                    else
                    {
                        Console.WriteLine("User not found.");
                    }
                }
                else
                {
                    Console.WriteLine("Incorrect password.");
                }

                Thread.Sleep(3000);
            }

            private void ShowMenu()
                {
                    bool running = true;

                    while (running)
                    {
                        Console.WriteLine("\nSelect an option:");
                        Console.WriteLine("1- Check Current Balance");
                        Console.WriteLine("2- Deposit");
                        Console.WriteLine("3- Withdraw");
                        Console.WriteLine("4- Transfer Money");
                        Console.WriteLine("5- View Transaction History");
                        Console.WriteLine("6- Pending Transfers");

                        if (_currentUser.Type == UserType.VIP)
                        {
                            Console.WriteLine("7- Add New User");
                            Console.WriteLine("8- Delete User");
                            Console.WriteLine("9- Update User Information");
                        }

                        Console.WriteLine("0- Exit");

                        string choice = Console.ReadLine();

                        switch (choice)
                        {
                            case "1":
                                CheckBalance();
                                break;
                            case "2":
                                Deposit();
                                break;
                            case "3":
                                Withdraw();
                                break;
                            case "4":
                                TransferMoney();
                                break;
                            case "5":
                                ViewTransactionHistory();
                                break;
                            case "6":
                                ManagePendingTransfers();
                                break;
                            case "7":
                                if (_currentUser.Type == UserType.VIP)
                                    CreateNewUser();
                                else
                                    Console.WriteLine("Invalid option, please try again.");
                                break;
                            case "8":
                                if (_currentUser.Type == UserType.VIP)
                                    DeleteUser();
                                else
                                    Console.WriteLine("Invalid option, please try again.");
                                break;
                            case "9":
                                if (_currentUser.Type == UserType.VIP)
                                    UpdateUserInfo();
                                else
                                    Console.WriteLine("Invalid option, please try again.");
                                break;
                            case "0":
                                running = false;
                                _transactionRepository.AddTransaction(new TransactionInfo(
                                    0,
                                    _currentUser.UserId,
                                    _currentUser.Username,
                                    "Logout",
                                    0,
                                    DateTime.Now,
                                    _currentUser.Balance,
                                    _currentUser.Balance,
                                    null,
                                    true,
                                    "Managerial"
                                ));
                                Console.WriteLine("Exiting program...");
                                Thread.Sleep(3000);
                                break;
                            default:
                                Console.WriteLine("Invalid option, please try again.");
                                break;
                        }
                    }
                }

            private void ManagePendingTransfers()
            {
                var userPendingTransfers = _pendingTransfers.GetPendingTransfersByRecipientId(_currentUser.UserId).ToList();

            if (!userPendingTransfers.Any())
                {
                    Console.WriteLine("No pending transfers.");
                    return;
                }

                Console.WriteLine("\nPending Transfers:");
                for (int i = 0; i < userPendingTransfers.Count; i++)
                {
                    var transfer = userPendingTransfers[i];
                    Console.WriteLine($"{i + 1}- From: {transfer.SenderUsername}, Amount: {transfer.Amount:C}," +
                        $" Date: {transfer.DateTime}");
                }

                Console.WriteLine("\nEnter the number of the transfer to accept/reject (0 to go back):");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= userPendingTransfers.Count)
                {
                    var selectedTransfer = userPendingTransfers[choice - 1];

                    Console.WriteLine("1- Accept");
                    Console.WriteLine("2- Reject");
                    Console.WriteLine("0- Back");

                    string action = Console.ReadLine();
                    switch (action)
                    {
                        case "1":
                            ProcessTransfer(selectedTransfer, true);
                            break;
                        case "2":
                            ProcessTransfer(selectedTransfer, false);
                            break;
                    }
                }
            }

            private void ProcessTransfer(PendingTransfer transfer, bool accept)
            {
                var sender = _userRepository.GetUserById(transfer.SenderId);

                if (accept)
                {
                    _currentUser.Balance += transfer.Amount;
                    _userRepository.UpdateUser(_currentUser);

                    var recipientTransaction = new TransactionInfo(
                        0,
                        _currentUser.UserId,
                        _currentUser.Username,
                        "Transfer (Received)",
                        transfer.Amount,
                        DateTime.Now,
                        _currentUser.Balance - transfer.Amount,
                        _currentUser.Balance,
                        transfer.SenderId,
                        true,
                        "Financial"
                    );

                    _transactionRepository.AddTransaction(recipientTransaction);
                    _transactionRepository.AddTransaction(new TransactionInfo(
                        0,
                        _currentUser.UserId,
                        _currentUser.Username,
                        "Transfer Accepted",
                        0,
                        DateTime.Now,
                        0,
                        0,
                        null,
                        true,
                        "Financial"
                    ));

                    Console.WriteLine($"Transfer accepted. Your new balance is: {_currentUser.Balance:C}");
                }
                else
                {
                    sender.Balance += transfer.Amount;
                    _userRepository.UpdateUser(sender);

                    var refundTransaction = new TransactionInfo(
                        0,
                        sender.UserId,
                        sender.Username,
                        "Transfer (Refunded)",
                        transfer.Amount,
                        DateTime.Now,
                        sender.Balance - transfer.Amount,
                        sender.Balance,
                        _currentUser.UserId,
                        true,
                        "Financial"
                    );

                    _transactionRepository.AddTransaction(refundTransaction);
                    _transactionRepository.AddTransaction(new TransactionInfo(
                        0,
                        _currentUser.UserId,
                        _currentUser.Username,
                        "Transfer Rejected",
                        0,
                        DateTime.Now,
                        0,
                        0,
                        null,
                        true,
                        "Financial"
                    ));

                    Console.WriteLine("Transfer rejected. Money has been returned to sender.");
                }

                _pendingTransfers.DeletePendingTransfer(transfer.TransferId);
            }

            private void ViewTransactionHistory()
            {
                var allTransactions = _transactionRepository.GetFinancialTransactions(_currentUser.UserId)
                    .OrderBy(t => t.DateTime)
                    .ToList();

                if (allTransactions.Count > 100)
                {
                
                    int transactionsToRemove = allTransactions.Count - 100;

                    var transactionsToDelete = allTransactions
                        .Take(transactionsToRemove)
                        .Select(t => t.TransactionId)
                        .ToList();

                    foreach (var transactionId in transactionsToDelete)
                    {
                        _transactionRepository.DeleteTransaction(transactionId);
                    }

                    _transactionRepository.AddTransaction(new TransactionInfo(
                        0,
                        _currentUser.UserId,
                        _currentUser.Username,
                        "Transaction History Cleanup",
                        0,
                        DateTime.Now,
                        _currentUser.Balance,
                        _currentUser.Balance,
                        null,
                        true,
                        "Financial"
                    ));

                    Console.WriteLine($"\nNotice: {transactionsToRemove} old transactions have been cleaned up to maintain the 100 transaction limit.");
                }

            
                Console.WriteLine("\nSelect transaction type to view:");
                Console.WriteLine("1- All Transactions");
                Console.WriteLine("2- Financial Transactions");
                Console.WriteLine("3- Managerial Transactions");
                Console.WriteLine("0- Back");

                string choice = Console.ReadLine();
                IEnumerable<TransactionInfo> transactions = null;
                string header = "";

                switch (choice)
                {
                    case "1":
                        transactions = _transactionRepository.GetTransactionsByUserId(_currentUser.UserId)
                            .OrderByDescending(t => t.DateTime)
                            .Take(100);  
                        header = "All Transactions";
                        break;
                    case "2":
                        transactions = _transactionRepository.GetFinancialTransactions(_currentUser.UserId)
                            .OrderByDescending(t => t.DateTime)
                            .Take(100);
                        header = "Financial Transactions";
                        break;
                    case "3":
                        transactions = _transactionRepository.GetManagerialTransactions(_currentUser.UserId)
                            .OrderByDescending(t => t.DateTime)
                            .Take(100); 
                        header = "Managerial Transactions";
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        return;
                }

                if (transactions == null || !transactions.Any())
                {
                    Console.WriteLine("No transactions found.");
                    return;
                }

                Console.WriteLine($"\n{header}:");
                Console.WriteLine("(Showing most recent 100 transactions)\n");

                foreach (var transaction in transactions)
                {
                    Console.WriteLine($"Date: {transaction.DateTime}");
                    Console.WriteLine($"Type: {transaction.TransactionName}");

                    if (transaction.TransactionName is "Deposit" or "Withdrawal" or "Transfer (Sent)" or "Transfer (Received)"
                        or "Transfer (Sent - Pending)" or "Transfer (Refunded)")
                    {
                        Console.WriteLine($"Amount: {transaction.Amount:C}");
                        Console.WriteLine($"Balance Before: {transaction.BalanceBefore:C}");
                        Console.WriteLine($"Balance After: {transaction.BalanceAfter:C}");
                    }

                    if (transaction.RecipientId.HasValue)
                    {
                        var recipient = _userRepository.GetUserById(transaction.RecipientId.Value);
                        Console.WriteLine($"Recipient: {recipient?.Username ?? "Unknown"}");
                    }

                    Console.WriteLine($"Status: {(transaction.IsComplete ? "Complete" : "Pending")}");
                    Console.WriteLine();
                }

                _transactionRepository.AddTransaction(new TransactionInfo(
                    0,
                    _currentUser.UserId,
                    _currentUser.Username,
                    $"View {header}",
                    0,
                    DateTime.Now,
                    0,
                    0,
                    null,
                    true,
                    "Financial"
                ));
            }

            private void CheckBalance()
            {
                try
                {
                    if (_currentUser == null)
                    {
                        Console.WriteLine("No user logged in.");
                        return;
                    }

                    Console.WriteLine($"Checking balance for user: {_currentUser.Username} ");

                    var returnedTransfers = _transactionRepository.GetFinancialTransactions(_currentUser.UserId)
                        .Where(t => t.TransactionName == "Transfer Returned - Recipient Deleted" ||
                                    t.TransactionName == "Transfer (Refunded)" ||
                                    (t.TransactionName == "Transfer (Sent - Pending)" &&
                                     !_pendingTransfers.GetPendingTransfersBySenderId(_currentUser.UserId)
                                         .Any(pt => pt.Amount == t.Amount &&
                                                    pt.RecipientId == t.RecipientId)))
                        .ToList();

                    double totalReturnedMoney = returnedTransfers.Sum(t => t.Amount);
                    Console.WriteLine("\n=== DETAILED BALANCE INFORMATION ===");

                    if (totalReturnedMoney > 0)
                    {
                        Console.WriteLine("\n1. Returned Money from Transfers:");
                        foreach (var transfer in returnedTransfers)
                        {
                            string status = transfer.TransactionName == "Transfer (Sent - Pending)"
                                ? "Returned (Recipient Deleted)"
                                : transfer.TransactionName;
                            Console.WriteLine($"   - {transfer.DateTime}: {status}: {transfer.Amount:C}");
                        }
                        Console.WriteLine($"   Total Returned: {totalReturnedMoney:C}");
                    }
                    else
                    {
                        Console.WriteLine("\n1. No returned money from transfers");
                    }

                    Console.WriteLine($"\n3. Current Available Balance: {_currentUser.Balance:C}");

                    _transactionRepository.AddTransaction(new TransactionInfo(
                        0,
                        _currentUser.UserId,
                        _currentUser.Username,
                        "Balance Check",
                        0,
                        DateTime.Now,
                        0,
                        0,
                        null,
                        true,
                        "Financial"
                    ));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while checking balance: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                }
            }

            private void Deposit()
                    {
                        Console.Write("Enter amount to deposit: ");
                        if (double.TryParse(Console.ReadLine(), out double amount) && amount > 0)
                        {
                            double oldBalance = _currentUser.Balance;
                            _currentUser.Balance += amount;
                            _userRepository.UpdateUser(_currentUser);

                            var transaction = new TransactionInfo(
                                0,
                                _currentUser.UserId,
                                _currentUser.Username,
                                "Deposit",
                                amount,
                                DateTime.Now,
                                oldBalance,
                                _currentUser.Balance,
                                null,
                                true,
                                "Financial"
                            );

                            _transactionRepository.AddTransaction(transaction);
                    

                            Console.WriteLine($"Deposit successful. Your new balance is: {_currentUser.Balance:C}");
                        }
                        else
                        {
                            Console.WriteLine("Invalid amount, please try again.");
                        }
                    }

            private void Withdraw()
            {
                Console.Write("Enter amount to withdraw: ");
                if (double.TryParse(Console.ReadLine(), out double amount) && amount > 0 && amount <= _currentUser.Balance)
                {
                    double oldBalance = _currentUser.Balance;
                    _currentUser.Balance -= amount;
                    _userRepository.UpdateUser(_currentUser);

                    var transaction = new TransactionInfo(
                        0,
                        _currentUser.UserId,
                        _currentUser.Username,
                        "Withdrawal",
                        amount,
                        DateTime.Now,
                        oldBalance,
                        _currentUser.Balance,
                        null,
                        true,
                        "Financial"
                    );

                    _transactionRepository.AddTransaction(transaction);
                    

                    Console.WriteLine($"Withdrawal successful. Your new balance is: {_currentUser.Balance:C}");
                }
                else
                {
                    Console.WriteLine("Insufficient balance or invalid amount.");
                }
            }

            private void TransferMoney()
            {
                try
                {
                    if (_currentUser == null)
                    {
                        Console.WriteLine("No user logged in.");
                        return;
                    }

                    Console.Write("Enter recipient's username: ");
                    string recipientUsername = Console.ReadLine();
                    IUser recipient = _userRepository.GetUserByUsername(recipientUsername);

                    if (recipient == null)
                    {
                        Console.WriteLine("Recipient not found.");
                        return;
                    }

                    if (recipient.UserId == _currentUser.UserId)
                    {
                        Console.WriteLine("Cannot transfer money to yourself.");
                        return;
                    }

                
                   

                    Console.Write("Enter amount to transfer: ");
                    if (double.TryParse(Console.ReadLine(), out double amount) && amount > 0 && amount <= _currentUser.Balance)
                    {
                        double senderOldBalance = _currentUser.Balance;
                        _currentUser.Balance -= amount;
                        _userRepository.UpdateUser(_currentUser);

                        var pendingTransfer = new PendingTransfer(
                            0,
                            _currentUser.UserId,
                            _currentUser.Username,
                            recipient.UserId,
                            amount
                        );

                        _pendingTransfers.AddPendingTransfer(pendingTransfer);

                        var senderTransaction = new TransactionInfo(
                            0,
                            _currentUser.UserId,
                            _currentUser.Username,
                            "Transfer (Sent - Pending)",
                            amount,
                            DateTime.Now,
                            senderOldBalance,
                            _currentUser.Balance,
                            recipient.UserId,
                            false,
                            "Financial"
                        );

                        _transactionRepository.AddTransaction(senderTransaction);
                        Console.WriteLine($"Transfer initiated. Amount {amount:C} is pending recipient's approval.");
                        Console.WriteLine($"Your new balance is: {_currentUser.Balance:C}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid amount or insufficient balance.");
                    }
                 }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An unexpected error occurred while transferring money: {ex.Message}");
                        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    }
            }













    }

        
    
}
        
