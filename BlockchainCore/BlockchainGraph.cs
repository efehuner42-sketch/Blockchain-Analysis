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
        public void BFS_TrackFundFlow(string startAddress)
        {
            // Başlangıç cüzdanı sistemde yoksa işlemi iptal et
            if (!Wallets.ContainsKey(startAddress)) return;

            Console.WriteLine($"\n--- BFS ile Fon Akışı Başlatılıyor: {startAddress} ---");

            // Fırın sırası gibi: İlk giren ilk çıkar. 
            Queue<string> queue = new Queue<string>();
            // Aynı cüzdanı tekrar tekrar taramamak için ziyaret edilenleri burada tutuyoruz.
            HashSet<string> visited = new HashSet<string>();

            // Başlangıç cüzdanını sıraya ekle ve ziyaret edildi olarak işaretle.
            queue.Enqueue(startAddress);
            visited.Add(startAddress);

            // Sıra boşalana kadar çalışmaya devam et
            while (queue.Count > 0)
            {
                // Sıradaki ilk cüzdanı al
                string currentAddress = queue.Dequeue();
                WalletNode currentNode = Wallets[currentAddress];

                // Bu cüzdandan çıkan tüm para transferlerine tek tek bak
                foreach (var tx in currentNode.OutgoingTransactions)
                {
                    Console.WriteLine($"{currentAddress} cüzdanından {tx.ToAddress} cüzdanına {tx.Amount} miktarında transfer yapıldı.");

                    // Eğer paranın gittiği cüzdana daha önce bakmadıysak, onu da sıraya ekle
                    if (!visited.Contains(tx.ToAddress))
                    {
                        visited.Add(tx.ToAddress);
                        queue.Enqueue(tx.ToAddress);
                    }
                }
            }
        }

        // 4. DFS (Derin Öncelikli Arama) - Yığıt (Stack) Mantığı
        public List<string> DFS_DeepAnalysis(string startAddress, decimal minAmount = 0)
        {
            // Berke'nin UI'da göstereceği metinleri tutacağımız liste
            List<string> logs = new List<string>();

            if (!Wallets.ContainsKey(startAddress)) 
            {
                logs.Add($"Hata: {startAddress} adresi sistemde bulunamadı.");
                return logs;
            }

            logs.Add($"--- DFS Başlatılıyor: {startAddress} (Min: {minAmount} BTC) ---");

            Stack<string> stack = new Stack<string>();
            HashSet<string> visitedTransactions = new HashSet<string>();

            stack.Push(startAddress);

            while (stack.Count > 0)
            {
                string currentAddress = stack.Pop();
                WalletNode currentNode = Wallets[currentAddress];

                foreach (var tx in currentNode.OutgoingTransactions)
                {
                    if (!visitedTransactions.Contains(tx.TransactionId))
                    {
                        // İşlemi ziyaret edildi olarak işaretle
                        visitedTransactions.Add(tx.TransactionId);

                        // Miktar ne olursa olsun, algoritmanın ilerlemesi için yığına ekle
                        stack.Push(tx.ToAddress);

                        // Berke'nin UI'ına gidecek filtrelenmiş veriyi listeye ekle
                        if (tx.Amount >= minAmount)
                        {
                            logs.Add($"{currentAddress} -> {tx.ToAddress} ({tx.Amount} BTC)");
                        }
                    }
                }
            }

            logs.Add("> DFS analizi tamamlandı.");
            return logs; // Listeyi arayüze gönder
        }
    }
}