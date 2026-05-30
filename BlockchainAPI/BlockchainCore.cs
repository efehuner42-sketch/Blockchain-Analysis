#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BlockchainCore
{
    public class BlockchainGraph
    {
        public Hashtable Wallets = new Hashtable();

        public void AddWallet(string address)
        {
            if (!Wallets.ContainsKey(address))
            {
                Wallets[address] = new WalletNode(address);
            }
        }

        public void AddTransaction(string fromAddress, string toAddress, string txId, decimal amount)
        {
            AddWallet(fromAddress);
            AddWallet(toAddress);
            TransactionEdge newTx = new TransactionEdge(txId, toAddress, amount);
            ((WalletNode)Wallets[fromAddress]).OutgoingTransactions.Add(newTx);
        }

        // 3. BFS (Sığ Öncelikli Arama)
        public List<string> BFS_TrackFundFlow(string startAddress)
        {
            List<string> rotam = new List<string>();
            if (!Wallets.ContainsKey(startAddress)) return rotam;

            Console.WriteLine($"\n--- BFS ile Fon Akışı Başlatılıyor: {startAddress} ---");
            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue(startAddress);
            visited.Add(startAddress);

            while (queue.Count > 0)
            {
                string currentAddress = queue.Dequeue();
                rotam.Add(currentAddress); // Cüzdan listeye ekleniyor

                // HATA BURADAYDI: (WalletNode) dönüşümü (casting) eklendi
                WalletNode currentNode = (WalletNode)Wallets[currentAddress];

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
            return rotam;
        }

        // 4. DFS (Derin Öncelikli Arama)
        public List<string> DFS_DeepAnalysis(string startAddress)
        {
            List<string> rotam = new List<string>();
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
                    rotam.Add(currentAddress); // Cüzdan listeye ekleniyor

                    // HATA BURADAYDI: (WalletNode) dönüşümü (casting) eklendi
                    WalletNode currentNode = (WalletNode)Wallets[currentAddress];

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
            return rotam;
        }
    }

    public class MerkleTree
    {
        public string MerkleRoot { get; private set; } = string.Empty;

        public void BuildTree(List<string> transactions)
        {
            if (transactions == null || transactions.Count == 0) return;
            List<string> currentLayer = new List<string>(transactions);

            while (currentLayer.Count > 1)
            {
                List<string> nextLayer = new List<string>();
                for (int i = 0; i < currentLayer.Count; i += 2)
                {
                    string left = currentLayer[i];
                    string right = (i + 1 < currentLayer.Count) ? currentLayer[i + 1] : left;
                    nextLayer.Add(Hash(left + right));
                }
                currentLayer = nextLayer;
            }
            MerkleRoot = currentLayer[0];
        }

        private string Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }
    }

    public class TransactionEdge
    {
        public string TransactionId { get; set; }
        public string ToAddress { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }

        public TransactionEdge(string id, string to, decimal amount)
        {
            TransactionId = id; ToAddress = to; Amount = amount; Timestamp = DateTime.Now;
        }
    }

    public class WalletNode
    {
        public string WalletAddress { get; set; }
        public List<TransactionEdge> OutgoingTransactions { get; set; }

        public WalletNode(string address)
        {
            WalletAddress = address; OutgoingTransactions = new List<TransactionEdge>();
        }

        public decimal CalculateBalance(decimal totalReceived)
        {
            decimal totalSent = 0;
            foreach (var tx in OutgoingTransactions) totalSent += tx.Amount;
            return totalReceived - totalSent;
        }
    }

    public class TestDataGenerator
    {
        public static BlockchainGraph GenerateTestData()
        {
            BlockchainGraph graph = new BlockchainGraph();
            string efe = "Cuzdan_Efe", murat = "Cuzdan_Murat", fusun = "Cuzdan_Fusun", borsa = "Cuzdan_Borsa_Binance";

            graph.AddWallet(efe); graph.AddWallet(murat); graph.AddWallet(fusun); graph.AddWallet(borsa);

            graph.AddTransaction(efe, murat, "TX_001_EfeMurat", 50.0m);
            graph.AddTransaction(efe, fusun, "TX_002_EfeFusun", 30.0m);
            graph.AddTransaction(murat, borsa, "TX_003_MuratBorsa", 40.0m);
            graph.AddTransaction(fusun, borsa, "TX_004_FusunBorsa", 20.0m);

            return graph;
        }
    }
}