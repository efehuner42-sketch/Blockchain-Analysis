using System;
using System.Collections.Generic;

namespace BlockchainCore
{
    public class BlockchainGraph
    {
        public MyHashTable Wallets = new MyHashTable(100);

        public void AddWallet(string address)
        {
            if (!Wallets.ContainsKey(address))
                Wallets[address] = new WalletNode(address);
        }

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

        public void AddTransaction(string fromAddress, string toAddress, string txId, decimal amount)
        {
            AddWallet(fromAddress);
            AddWallet(toAddress);

            TransactionEdge newTx = new TransactionEdge(txId, toAddress, amount);
            Wallets[fromAddress].OutgoingTransactions.Add(newTx);
        }

        // --- BFS: EN KISA YOL TAKİBİ (MIN AMOUNT FİLTRELİ) ---
        public List<string> BFS_TrackFundFlow(string startAddress, string targetAddress, decimal minAmount = 0)
        {
            List<string> path = new List<string>();
            if (!Wallets.ContainsKey(startAddress) || !Wallets.ContainsKey(targetAddress)) return path;

            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();
            Dictionary<string, string> parentMap = new Dictionary<string, string>();

            queue.Enqueue(startAddress);
            visited.Add(startAddress);
            bool found = false;

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();

                if (current == targetAddress)
                {
                    found = true;
                    break;
                }

                WalletNode node = (WalletNode)Wallets[current];
                foreach (var tx in node.OutgoingTransactions)
                {
                    // KRİTİK KONTROL: Eğer transfer miktarı bütçeden küçükse bu yolu es geç!
                    if (tx.Amount >= minAmount && !visited.Contains(tx.ToAddress))
                    {
                        visited.Add(tx.ToAddress);
                        parentMap[tx.ToAddress] = current;
                        queue.Enqueue(tx.ToAddress);
                    }
                }
            }

            if (found)
            {
                string curr = targetAddress;
                while (curr != startAddress)
                {
                    path.Add(curr);
                    curr = parentMap[curr];
                }
                path.Add(startAddress);
                path.Reverse();
            }

            return path;
        }

        // --- DFS: DERİNLEMESİNE ANALİZ (MIN AMOUNT FİLTRELİ) ---
        public List<string> DFS_DeepAnalysis(string startAddress, string targetAddress, decimal minAmount = 0)
        {
            List<string> finalPath = new List<string>();
            if (!Wallets.ContainsKey(startAddress) || !Wallets.ContainsKey(targetAddress)) return finalPath;

            Stack<List<string>> stack = new Stack<List<string>>();
            HashSet<string> visited = new HashSet<string>();

            stack.Push(new List<string> { startAddress });

            while (stack.Count > 0)
            {
                List<string> currentPath = stack.Pop();
                string current = currentPath[currentPath.Count - 1];

                if (current == targetAddress)
                {
                    return currentPath;
                }

                if (!visited.Contains(current))
                {
                    visited.Add(current);
                    WalletNode node = (WalletNode)Wallets[current];

                    foreach (var tx in node.OutgoingTransactions)
                    {
                        // KRİTİK KONTROL: Eğer transfer miktarı bütçeden küçükse bu yolu es geç!
                        if (tx.Amount >= minAmount && !visited.Contains(tx.ToAddress))
                        {
                            List<string> newPath = new List<string>(currentPath);
                            newPath.Add(tx.ToAddress);
                            stack.Push(newPath);
                        }
                    }
                }
            }

            return finalPath;
        }
    }
}