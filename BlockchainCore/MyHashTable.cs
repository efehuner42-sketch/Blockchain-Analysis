#nullable disable // C#'ın aşırı korumacı "boş (null) olabilir" uyarılarını kapatır

using System;
using System.Collections.Generic;

namespace BlockchainCore
{
    public class HashNode
    {
        public string Key { get; set; }
        public WalletNode Value { get; set; }

        public HashNode(string key, WalletNode value)
        {
            Key = key;
            Value = value;
        }
    }

    public class MyHashTable
    {
        private readonly List<HashNode>[] _buckets;
        private readonly int _size;

        public MyHashTable(int size = 100)
        {
            _size = size;
            _buckets = new List<HashNode>[_size];
            for (int i = 0; i < _size; i++)
            {
                _buckets[i] = new List<HashNode>();
            }
        }

        private int GetHash(string key)
        {
            int hashValue = 0;
            foreach (char c in key)
            {
                hashValue += c;
            }
            return hashValue % _size;
        }

        public void Add(string key, WalletNode value)
        {
            int index = GetHash(key);
            List<HashNode> bucket = _buckets[index];

            foreach (var node in bucket)
            {
                if (node.Key == key)
                {
                    node.Value = value;
                    return;
                }
            }
            bucket.Add(new HashNode(key, value));
        }

        public WalletNode Get(string key)
        {
            int index = GetHash(key);
            List<HashNode> bucket = _buckets[index];

            foreach (var node in bucket)
            {
                if (node.Key == key)
                {
                    return node.Value;
                }
            }
            return null;
        }

        public bool ContainsKey(string key)
        {
            return Get(key) != null;
        }

        // *** İŞTE BURASI EKSİKTİ (CS0021 Hatasının Çözümü) ***
        // Cüzdanlara köşeli parantez [] ile erişebilmek için (Indexer)
        public WalletNode this[string key]
        {
            get { return Get(key); }
            set { Add(key, value); }
        }
    }
}