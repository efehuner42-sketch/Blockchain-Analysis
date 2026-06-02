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
            List<string> rotam = new List<string>(); // Arayüz için saf ID listesi

            if (!Wallets.ContainsKey(startAddress)) return rotam;

            Console.WriteLine($"\n--- BFS ile Fon Akışı Başlatılıyor: {startAddress} ---");

            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue(startAddress);
            visited.Add(startAddress);

            while (queue.Count > 0)
            {
                string currentAddress = queue.Dequeue();
                rotam.Add(currentAddress); // Sadece ID'yi listeye ekle (Animasyon için)

                WalletNode currentNode = (WalletNode)Wallets[currentAddress];

                foreach (var tx in currentNode.OutgoingTransactions)
                {
                    // Loglar backend terminalinde akmaya devam eder
                    Console.WriteLine($"{currentAddress} cüzdanından {tx.ToAddress} cüzdanına {tx.Amount} miktarında transfer yapıldı.");

                    if (!visited.Contains(tx.ToAddress))
                    {
                        visited.Add(tx.ToAddress);
                        queue.Enqueue(tx.ToAddress);
                    }
                }
            }
            return rotam; // Saf ID listesini gönder
        }

        // 4. DFS (Derin Öncelikli Arama) - Yığıt (Stack) Mantığı
        public List<string> DFS_DeepAnalysis(string startAddress, decimal minAmount = 0)
        {
            List<string> rotam = new List<string>(); // Arayüz için saf ID listesi

            if (!Wallets.ContainsKey(startAddress)) 
            {
                Console.WriteLine($"Hata: {startAddress} adresi sistemde bulunamadı.");
                return rotam;
            }

            Console.WriteLine($"--- DFS Başlatılıyor: {startAddress} (Min: {minAmount} BTC) ---");

            Stack<string> stack = new Stack<string>();
            HashSet<string> visitedTransactions = new HashSet<string>();

            stack.Push(startAddress);

            while (stack.Count > 0)
            {
                string currentAddress = stack.Pop();
                
                // Ziyaret edilen cüzdanı arayüz rotasına ekle (Sadece saf ID)
                if (!rotam.Contains(currentAddress)) {
                    rotam.Add(currentAddress);
                }

                WalletNode currentNode = (WalletNode)Wallets[currentAddress];

                foreach (var tx in currentNode.OutgoingTransactions)
                {
                    if (!visitedTransactions.Contains(tx.TransactionId))
                    {
                        visitedTransactions.Add(tx.TransactionId);
                        stack.Push(tx.ToAddress);

                        // Filtreli log mantığı backend terminalinde çalışır
                        if (tx.Amount >= minAmount)
                        {
                            Console.WriteLine($"{currentAddress} -> {tx.ToAddress} ({tx.Amount} BTC)");
                        }
                    }
                }
            }

            Console.WriteLine("> DFS analizi tamamlandı.");
            return rotam; // Saf ID listesini gönder
        }
    }
}