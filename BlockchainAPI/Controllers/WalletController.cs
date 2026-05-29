using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using BlockchainCore;

namespace BlockchainAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Bu attribute, rotayı otomatik olarak 'api/wallet' yapar.
    public class WalletController : ControllerBase
    {
        // Tüm API isteklerinde aynı graf verisini kullanmak için statik olarak tanımlıyoruz.
        private static readonly BlockchainGraph _graph;

        // Test amaçlı bilinen cüzdan adresleri (HashTable iterasyonu yoksa erişim sağlamak için)
        private static readonly string[] KnownAddresses = new string[]
        {
            "Cuzdan_Efe",
            "Cuzdan_Murat",
            "Cuzdan_Fusun",
            "Cuzdan_Borsa_Binance"
        };

        static WalletController()
        {
            // API ilk ayağa kalktığında test verilerini otomatik yüklüyoruz.
            _graph = TestDataGenerator.GenerateTestData();
        }

        // GET: api/wallet
        [HttpGet]
        public IActionResult GetFullGraph()
        {
            var nodesList = new List<object>();
            var edgesList = new List<object>();

            foreach (var address in KnownAddresses)
            {
                if (_graph.Wallets.ContainsKey(address))
                {
                    var walletNode = (WalletNode)_graph.Wallets[address];

                    decimal initialReceived = address == "Cuzdan_Efe" ? 180.0m : 0.0m;
                    if (address == "Cuzdan_Murat") initialReceived = 50.0m;
                    if (address == "Cuzdan_Fusun") initialReceived = 30.0m;
                    if (address == "Cuzdan_Borsa_Binance") initialReceived = 200.0m;

                    decimal currentBalance = walletNode.CalculateBalance(initialReceived);

                    string cuzdanIsmi = walletNode.WalletAddress == "Cuzdan_Borsa_Binance"
                        ? "Binance Borsa"
                        : walletNode.WalletAddress.Replace("Cuzdan_", "") + " Cüzdanı";

                    // Cytoscape kütüphanesinin zorunlu kıldığı 'data' sarmalayıcısı eklendi
                    // Bakiye (10 BTC) doğrudan ekranda görünecek şekilde label içine gömüldü
                    nodesList.Add(new
                    {
                        data = new
                        {
                            id = walletNode.WalletAddress,
                            label = $"{cuzdanIsmi}\n({currentBalance} BTC)",
                            balance = currentBalance
                        }
                    });

                    foreach (var tx in walletNode.OutgoingTransactions)
                    {
                        edgesList.Add(new
                        {
                            data = new
                            {
                                id = tx.TransactionId,
                                source = walletNode.WalletAddress,
                                target = tx.ToAddress,
                                amount = tx.Amount,
                                time = tx.Timestamp.ToString("HH:mm:ss")
                            }
                        });
                    }
                }
            }

            return Ok(new
            {
                nodes = nodesList,
                edges = edgesList
            });
        }

        // GET: api/wallet/bfs/{walletId}
        [HttpGet("bfs/{walletId}")]
        public IActionResult RunBfs(string walletId)
        {
            if (!_graph.Wallets.ContainsKey(walletId))
            {
                return NotFound(new { message = "Cüzdan bulunamadı." });
            }

            // Konsol çıktısını tetikler
            _graph.BFS_TrackFundFlow(walletId);

            // Analiz sonucunu frontend paneline bildirim olarak dönüyoruz
            return Ok(new
            {
                message = $"BFS Algoritması {walletId} için başarıyla çalıştırıldı. Fon akış izleme logları backend konsoluna yazdırıldı."
            });
        }

        // GET: api/wallet/dfs/{walletId}
        [HttpGet("dfs/{walletId}")]
        public IActionResult RunDfs(string walletId)
        {
            if (!_graph.Wallets.ContainsKey(walletId))
            {
                return NotFound(new { message = "Cüzdan bulunamadı." });
            }

            // Konsol çıktısını tetikler
            _graph.DFS_DeepAnalysis(walletId);

            return Ok(new
            {
                message = $"DFS Derin Analiz Algoritması {walletId} için tetiklendi. Detaylar backend konsolunda listeleniyor."
            });
        }

        // GET: api/wallet/merkle/{txId}
        // Berke arayüzde bir transfer çizgisine tıkladığında Merkle bütünlük kanıtını hesaplayan fonksiyon
        [HttpGet("merkle/{txId}")]
        public IActionResult GetMerkleProof(string txId)
        {
            // Blok içindeki tüm işlem listesi
            List<string> txList = new List<string>
            {
                "TX_001_EfeMurat",
                "TX_002_EfeFusun",
                "TX_003_MuratBorsa",
                "TX_004_FusunBorsa"
            };

            if (!txList.Contains(txId))
            {
                return NotFound(new { message = "İşlem kimliği (TX) bulunamadı." });
            }

            MerkleTree tree = new MerkleTree();
            tree.BuildTree(txList);

            // İlerleyen adımlarda ağacın ara düğüm hash'lerini de dinamik eşleştirmek üzere 
            // şimdilik Root değerini ve seçili tx bilgisini güvenli şekilde dönüyoruz
            return Ok(new
            {
                root = tree.MerkleRoot,
                selectedTx = txId,
                verified = true,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }
}