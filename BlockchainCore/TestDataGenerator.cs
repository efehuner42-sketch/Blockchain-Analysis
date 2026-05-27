using System;
using System.Collections.Generic;

namespace BlockchainCore
{
    public class TestDataGenerator
    {
        // 1. Sahte bir blokzincir ağı oluşturan metod
        public static BlockchainGraph GenerateTestData()
        {
            BlockchainGraph graph = new BlockchainGraph();

            Console.WriteLine("--- Sentetik Veri (Test Verisi) Üretiliyor ---");

            // Sahte Cüzdan Adresleri
            string efeCuzdan = "Cuzdan_Efe";
            string muratCuzdan = "Cuzdan_Murat";
            string fusunCuzdan = "Cuzdan_Fusun";
            string borsaCuzdan = "Cuzdan_Borsa_Binance";

            // Sisteme Cüzdanları Ekle
            graph.AddWallet(efeCuzdan);
            graph.AddWallet(muratCuzdan);
            graph.AddWallet(fusunCuzdan);
            graph.AddWallet(borsaCuzdan);

            // Sahte Para Transferleri (Kenarlar) Ekle
            // Efe, Murat'a 50 coin gönderiyor
            graph.AddTransaction(efeCuzdan, muratCuzdan, "TX_001_EfeMurat", 50.0m);
            // Efe, Füsun'a 30 coin gönderiyor
            graph.AddTransaction(efeCuzdan, fusunCuzdan, "TX_002_EfeFusun", 30.0m);
            // Murat, Borsaya 40 coin gönderiyor
            graph.AddTransaction(muratCuzdan, borsaCuzdan, "TX_003_MuratBorsa", 40.0m);
            // Füsun, Borsaya 20 coin gönderiyor
            graph.AddTransaction(fusunCuzdan, borsaCuzdan, "TX_004_FusunBorsa", 20.0m);

            Console.WriteLine("Test verileri başarıyla oluşturuldu!\n");
            return graph;
        }

        // 2. Tüm sistemin kusursuz çalıştığını test eden metod
        public static void RunAllTests()
        {
            // Yukarıdaki metotla içi dolu sahte grafımızı alıyoruz
            BlockchainGraph testGraph = GenerateTestData();

            // ARAMA ALGORİTMALARI TESTİ
            // Efe'nin parası nerelere dağılmış izleyelim
            testGraph.BFS_TrackFundFlow("Cuzdan_Efe");
            testGraph.DFS_DeepAnalysis("Cuzdan_Efe");

            // MERKLE AĞACI TESTİ
            Console.WriteLine("\n--- Merkle Tree Güvenlik Testi ---");
            MerkleTree tree = new MerkleTree();
            
            // Sistemdeki işlem ID'lerini bir listeye koy
            List<string> islemListesi = new List<string> 
            { 
                "TX_001_EfeMurat", 
                "TX_002_EfeFusun", 
                "TX_003_MuratBorsa", 
                "TX_004_FusunBorsa" 
            };
            
            // Ağacı inşa et ve şifreyi bul
            tree.BuildTree(islemListesi);
            Console.WriteLine($"4 İşlemin birleşmesiyle oluşan aşılmaz Merkle Root Şifresi: \n{tree.MerkleRoot}\n");
        }
    }
}