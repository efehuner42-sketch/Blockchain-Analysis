using System;
using System.Collections.Generic;

namespace BlockchainCore
{
    public class BlockchainGraph
    {
        // Cüzdanları adreslerine göre hızlıca bulmak için tuttuğumuz liste.
        public MyHashTable Wallets = new MyHashTable(100); // 100 çekmeceli kendi tablomuz!

        // 1. Yeni bir cüzdan ekleme metodu
        public void AddWallet(string address)
        {
            // Eğer bu cüzdan adresi sistemde yoksa, yeni bir tane oluştur ve ekle.
            if (!Wallets.ContainsKey(address))
            {
                Wallets[address] = new WalletNode(address);
            }
        }

        // 2. İki cüzdan arasına transfer (kenar) ekleme metodu
        public void AddTransaction(string fromAddress, string toAddress, string txId, decimal amount)
        {
            // Parayı gönderen ve alan cüzdanlar sistemde yoksa önce onları oluştururuz.
            AddWallet(fromAddress);
            AddWallet(toAddress);

            // İşlemi yaratıyoruz
            TransactionEdge newTx = new TransactionEdge(txId, toAddress, amount);
            
            // Gönderen cüzdanın "Giden İşlemler" listesine bu işlemi ekliyoruz.
            Wallets[fromAddress].OutgoingTransactions.Add(newTx);
        }

        // 3. BFS (Sığ Öncelikli Arama) - Kuyruk (Queue) Mantığı
        public List<string> BFS_TrackFundFlow(string startAddress)
        {
            List<string> rotam = new List<string>(); // Berke'ye gidecek liste

            // Başlangıç cüzdanı sistemde yoksa boş liste dön
            if (!Wallets.ContainsKey(startAddress)) return rotam;

            Console.WriteLine($"\n--- BFS ile Fon Akışı Başlatılıyor: {startAddress} ---");

            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue(startAddress);
            visited.Add(startAddress);

            while (queue.Count > 0)
            {
                string currentAddress = queue.Dequeue();

                // Cüzdanı sıradan çeker çekmez listemize ekliyoruz
                rotam.Add(currentAddress);

                WalletNode currentNode = Wallets[currentAddress];

                foreach (var tx in currentNode.OutgoingTransactions)
                {
                    Console.WriteLine($"{currentAddress} -> {tx.ToAddress} ({tx.Amount})");

                    if (!visited.Contains(tx.ToAddress))
                    {
                        visited.Add(tx.ToAddress);
                        queue.Enqueue(tx.ToAddress);
                    }
                }
            }

            return rotam; // En son rotayı dışarı fırlatıyoruz
        }

        // 4. DFS (Derin Öncelikli Arama) - Yığıt (Stack) Mantığı
        public List<string> DFS_DeepAnalysis(string startAddress)
        {
            List<string> rotam = new List<string>(); // Berke'ye gidecek liste

            if (!Wallets.ContainsKey(startAddress)) return rotam;

            Console.WriteLine($"\n--- DFS ile Derinlemesine Analiz Başlatılıyor: {startAddress} ---");

            Stack<string> stack = new Stack<string>();
            HashSet<string> visited = new HashSet<string>();

            stack.Push(startAddress);

            while (stack.Count > 0)
            {
                string currentAddress = stack.Pop();

                if (!visited.Contains(currentAddress))
                {
                    visited.Add(currentAddress);

                    // Cüzdanı ziyaret ettiğimiz an listemize ekliyoruz
                    rotam.Add(currentAddress);

                    WalletNode currentNode = Wallets[currentAddress];

                    foreach (var tx in currentNode.OutgoingTransactions)
                    {
                        Console.WriteLine($"Derin Analiz: {currentAddress} -> {tx.ToAddress} ({tx.Amount})");

                        if (!visited.Contains(tx.ToAddress))
                        {
                            stack.Push(tx.ToAddress);
                        }
                    }
                }
            }

            return rotam; // En son rotayı dışarı fırlatıyoruz
        }
    }
}