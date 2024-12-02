using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATMapp
{
    public class TransactionInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string TransactionName { get; set; }
        public double Amount { get; set; }
        public DateTime DateTime { get; set; }
        public double BalanceBefore { get; set; }
        public double BalanceAfter { get; set; }
        public int? RecipientId { get; set; }
        public bool IsComplete { get; set; }
        public string TransactionType { get; set; } 

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [ForeignKey("RecipientId")]
        public virtual User Recipient { get; set; }

        public TransactionInfo(
            int transactionId,
            int userId,
            string username,
            string transactionName,
            double amount,
            DateTime dateTime,
            double balanceBefore,
            double balanceAfter,
            int? recipientId,
            bool isComplete,
            string transactionType)
        {
            TransactionId = transactionId;
            UserId = userId;
            Username = username;
            TransactionName = transactionName;
            Amount = amount;
            DateTime = dateTime;
            BalanceBefore = balanceBefore;
            BalanceAfter = balanceAfter;
            RecipientId = recipientId;
            IsComplete = isComplete;
            TransactionType = transactionType;
        }
    }
}

