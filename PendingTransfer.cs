using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ATMapp
{
    public class PendingTransfer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransferId { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public int RecipientId { get; set; }
        public double Amount { get; set; }
        public DateTime DateTime { get; set; }

        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }
        [ForeignKey("RecipientId")]
        public virtual User Recipient { get; set; }

        public PendingTransfer() { }

        public PendingTransfer(int transferId, int senderId, string senderUsername, int recipientId, double amount)
        {
            TransferId = transferId;
            SenderId = senderId;
            SenderUsername = senderUsername;
            RecipientId = recipientId;
            Amount = amount;
            DateTime = DateTime.Now;
        }

        private readonly AtmDbContext _context;

        public PendingTransfer(AtmDbContext context)
        {
            _context = context;
        }

        public void AddPendingTransfer(PendingTransfer pendingTransfer)
        {
            _context.PendingTransfers.Add(pendingTransfer);
            _context.SaveChanges();
        }

        public IEnumerable<PendingTransfer> GetPendingTransfersBySenderId(int senderId)
        {
            return _context.PendingTransfers.Where(t => t.SenderId == senderId).ToList();
        }

        public IEnumerable<PendingTransfer> GetPendingTransfersByRecipientId(int recipientId)
        {
            return _context.PendingTransfers.Where(t => t.RecipientId == recipientId).ToList();
        }

        public void DeletePendingTransfer(int transferId)
        {
            var pendingTransfer = _context.PendingTransfers.FirstOrDefault(t => t.TransferId == transferId);
            if (pendingTransfer != null)
            {
                _context.PendingTransfers.Remove(pendingTransfer);
                _context.SaveChanges();
            }
        }
    }
}