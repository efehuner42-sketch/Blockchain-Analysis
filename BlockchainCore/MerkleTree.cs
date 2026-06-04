#nullable disable // Uyarıları gizlemek için eklendi

using System;
using System.Collections.Generic;
using System.Security.Cryptography; 
using System.Text;

namespace BlockchainCore
{
    public class MerkleTree
    {
        public string MerkleRoot { get; private set; } = string.Empty;

        public void FusunGunBuildTree(List<string> transactions)
        {
            if (transactions == null || transactions.Count == 0)
            {
                MerkleRoot = string.Empty;
                return;
            }

            List<string> currentLayer = new List<string>(transactions);

            while (currentLayer.Count > 1)
            {
                List<string> nextLayer = new List<string>();

                for (int i = 0; i < currentLayer.Count; i += 2)
                {
                    string left = currentLayer[i];
                    string right = (i + 1 < currentLayer.Count) ? currentLayer[i + 1] : left;

                    string combinedHash = FusunGunHash(left + right);
                    nextLayer.Add(combinedHash);
                }
                currentLayer = nextLayer;
            }

            MerkleRoot = currentLayer[0];
        }

        private string FusunGunHash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}