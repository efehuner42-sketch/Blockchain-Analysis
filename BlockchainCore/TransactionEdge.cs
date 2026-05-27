using System;

namespace BlockchainCore
{
    public class TransactionEdge
    {
        public string TransactionId { get; set; } // İşlemin benzersiz kimliği
        public string ToAddress { get; set; }     // Paranın gittiği cüzdan adresi
        public decimal Amount { get; set; }       // Transfer miktarı
        public DateTime Timestamp { get; set; }   // İşlem zamanı

        public TransactionEdge(string id, string to, decimal amount)
        {
            TransactionId = id;
            ToAddress = to;
            Amount = amount;
            Timestamp = DateTime.Now;
        }
    }
}