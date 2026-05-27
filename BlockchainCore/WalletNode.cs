using System;
using System.Collections.Generic;

namespace BlockchainCore
{
    public class WalletNode
    {
        public string WalletAddress { get; set; }
        
        // Bu cüzdandan çıkan işlemlerin (kenarların) listesi
        public List<TransactionEdge> OutgoingTransactions { get; set; } 

        public WalletNode(string address)
        {
            WalletAddress = address;
            OutgoingTransactions = new List<TransactionEdge>();
        }

        // Bakiye hesaplama (Gelen ve giden transferlere göre)
        public decimal CalculateBalance(decimal totalReceived)
        {
            decimal totalSent = 0;
            foreach(var tx in OutgoingTransactions)
            {
                totalSent += tx.Amount;
            }
            return totalReceived - totalSent;
        }
    }
}