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
            if (!Wallets.ContainsKey(address))
            {
                Wallets[address] = new WalletNode(address);
            }
        }

        // Dinamik Cüzdan Silme Metodu
        public void RemoveWallet(string address)
        {
            if (Wallets.ContainsKey(address))
            {
                foreach (string key in Wallets.Keys)
                {
                    Wallets[key].OutgoingTransactions.RemoveAll(tx => tx.ToAddress == address);
                }
                Wallets.Remove(address);
            }
        }

        // 2. İki cüzdan arasına transfer (kenar) ekleme metodu
        public void AddTransaction(string fromAddress, string toAddress, string txId, decimal amount)
        {
            AddWallet(fromAddress);
            AddWallet(toAddress);

            TransactionEdge newTx = new TransactionEdge(txId, toAddress, amount);
            Wallets[fromAddress].OutgoingTransactions.Add(newTx);
        }

        // 3. BFS (Sığ Öncelikli Arama) - HEDEFLİ (Target) ARAMA MANTIĞI
        public List<string> BFS_TrackFundFlow(string startAddress, string targetAddress)
        {
            List<string> rotam = new List<string>();

            if (!Wallets.ContainsKey(startAddress) || !Wallets.ContainsKey(targetAddress))
                return rotam;

            Console.WriteLine($"\n--- BFS ile Fon Akışı Başlatılıyor: {startAddress} -> {targetAddress} ---");

            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();
            Dictionary<string, string> parentMap = new Dictionary<string, string>(); // Rotayı geriye sarmak için

            queue.Enqueue(startAddress);
            visited.Add(startAddress);

            bool found = false;

            while (queue.Count > 0)
            {
                string currentAddress = queue.Dequeue();

                // Hedefi bulduk mu?
                if (currentAddress == targetAddress)
                {
                    found = true;
                    break;
                }

                WalletNode currentNode = (WalletNode)Wallets[currentAddress];

                foreach (var tx in currentNode.OutgoingTransactions)
                {
                    Console.WriteLine($"{currentAddress} cüzdanından {tx.ToAddress} cüzdanına {tx.Amount} BTC transfer yapıldı.");

                    if (!visited.Contains(tx.ToAddress))
                    {
                        visited.Add(tx.ToAddress);
                        parentMap[tx.ToAddress] = currentAddress; // Nereden geldiğimizi not alıyoruz
                        queue.Enqueue(tx.ToAddress);
                    }
                }
            }

            // Hedefi bulduysak, ekmek kırıntılarını tersten okuyarak rotayı çiz
            if (found)
            {
                string curr = targetAddress;
                while (curr != startAddress)
                {
                    rotam.Add(curr);
                    curr = parentMap[curr];
                }
                rotam.Add(startAddress);
                rotam.Reverse(); // Listeyi Başlangıç -> Hedef yönüne çevir
            }

            return rotam; // Saf ID rotasını gönder
        }

        // 4. DFS (Derin Öncelikli Arama) - HEDEFLİ (Target) REKÜRSİF MANTIK
        public List<string> DFS_DeepAnalysis(string startAddress, string targetAddress, decimal minAmount = 0)
        {
            List<string> rotam = new List<string>();
            HashSet<string> visited = new HashSet<string>();

            if (!Wallets.ContainsKey(startAddress) || !Wallets.ContainsKey(targetAddress))
            {
                Console.WriteLine($"Hata: Başlangıç veya hedef adresi sistemde bulunamadı.");
                return rotam;
            }

            Console.WriteLine($"--- DFS Başlatılıyor: {startAddress} -> {targetAddress} (Min: {minAmount} BTC) ---");

            DFS_Helper(startAddress, targetAddress, visited, rotam, minAmount);
            return rotam;
        }

        // DFS için yardımcı metot (Rotayı doğru çizebilmek için Backtracking kullanır)
        private bool DFS_Helper(string current, string targetAddress, HashSet<string> visited, List<string> path, decimal minAmount)
        {
            visited.Add(current);
            path.Add(current);

            if (current == targetAddress)
                return true;

            WalletNode wallet = (WalletNode)Wallets[current];
            foreach (var tx in wallet.OutgoingTransactions)
            {
                if (!visited.Contains(tx.ToAddress) && tx.Amount >= minAmount)
                {
                    Console.WriteLine($"{current} -> {tx.ToAddress} ({tx.Amount} BTC)");
                    if (DFS_Helper(tx.ToAddress, targetAddress, visited, path, minAmount))
                        return true;
                }
            }

            // Çıkmaz sokağa girdiysek, bu adımı rotadan sil (Backtrack)
            path.RemoveAt(path.Count - 1);
            return false;
        }
    }
}