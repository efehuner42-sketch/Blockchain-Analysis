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
        public void DFS_DeepAnalysis(string startAddress)
        {
            // Başlangıç cüzdanı sistemde yoksa işlemi iptal et
            if (!Wallets.ContainsKey(startAddress)) return;

            Console.WriteLine($"\n--- DFS ile Derinlemesine Analiz Başlatılıyor: {startAddress} ---");

            // Bulaşık yığını gibi: En son giren ilk çıkar.
            Stack<string> stack = new Stack<string>();
            HashSet<string> visited = new HashSet<string>();

            // Başlangıç cüzdanını yığına ekle
            stack.Push(startAddress);

            // Yığın boşalana kadar çalış
            while (stack.Count > 0)
            {
                // Yığının en üstündeki (en son eklenen) cüzdanı al
                string currentAddress = stack.Pop();

                // Eğer bu cüzdana daha önce geldiysek atla
                if (!visited.Contains(currentAddress))
                {
                    // Cüzdanı ziyaret edildi olarak işaretle
                    visited.Add(currentAddress);
                    WalletNode currentNode = Wallets[currentAddress];

                    // Bu cüzdandan çıkan tüm işlemlere bak
                    foreach (var tx in currentNode.OutgoingTransactions)
                    {
                        Console.WriteLine($"Derin Analiz: {currentAddress} -> {tx.ToAddress} ({tx.Amount})");
                        
                        // Gidilecek yeni cüzdanı yığına ekle (böylece bir sonraki adımda hemen ona geçilecek)
                        if (!visited.Contains(tx.ToAddress))
                        {
                            stack.Push(tx.ToAddress);
                        }
                    }
                }
            }
        }
    }
}