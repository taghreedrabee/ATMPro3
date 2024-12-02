using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATMapp
{
    public class User : IUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public UserType Type { get; set; }
        public double Balance { get; set; }

        public ICollection<TransactionInfo> Transactions { get; set; }
        public ICollection<PendingTransfer> SentTransfers { get; set; }
        public ICollection<PendingTransfer> ReceivedTransfers { get; set; }

        public User()
        {
            Transactions = new List<TransactionInfo>();
            SentTransfers = new List<PendingTransfer>();
            ReceivedTransfers = new List<PendingTransfer>();
        }

    }
}