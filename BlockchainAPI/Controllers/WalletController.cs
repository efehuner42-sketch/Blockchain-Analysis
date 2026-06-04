using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using BlockchainCore;

namespace BlockchainAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        public static BlockchainGraph _graph = TestDataGenerator.GenerateTestData();

        // GET: api/wallet
        [HttpGet]
        public IActionResult GetFullGraph()
        {
            var nodesList = new List<object>();
            var edgesList = new List<object>();

            // --- GELEN PARALARI (INCOMING) HESAPLAYAN KİLİT KISIM ---
            var incomingAmounts = new Dictionary<string, decimal>();
            foreach (var address in _graph.Wallets.Keys)
            {
                var wNode = (WalletNode)_graph.Wallets[address];
                foreach (var tx in wNode.OutgoingTransactions)
                {
                    if (!incomingAmounts.ContainsKey(tx.ToAddress))
                        incomingAmounts[tx.ToAddress] = 0.0m;
                    
                    incomingAmounts[tx.ToAddress] += tx.Amount;
                }
            }
            // --------------------------------------------------------

            foreach (var address in _graph.Wallets.Keys)
            {
                if (_graph.Wallets.ContainsKey(address))
                {
                    var walletNode = (WalletNode)_graph.Wallets[address];

                    decimal initialReceived = address == "Cuzdan_Efe" ? 180.0m : 0.0m;
                    if (address == "Cuzdan_Murat") initialReceived = 50.0m;
                    if (address == "Cuzdan_Fusun") initialReceived = 30.0m;
                    if (address == "Cuzdan_Borsa_Binance") initialReceived = 200.0m;

                    // Bakiye = Başlangıç + Gelenler - Gidenler
                    decimal totalIncoming = incomingAmounts.ContainsKey(address) ? incomingAmounts[address] : 0.0m;
                    decimal totalOutgoing = 0.0m;
                    foreach (var tx in walletNode.OutgoingTransactions)
                    {
                        totalOutgoing += tx.Amount;
                    }

                    decimal currentBalance = initialReceived + totalIncoming - totalOutgoing;

                    // Cüzdan isimlerini ekrana daha şık yazdırmak için formatlıyoruz
                    string cuzdanIsmi = "";
                    if (walletNode.WalletAddress == "Cuzdan_Borsa_Binance")
                    {
                        cuzdanIsmi = "Binance Borsa";
                    }
                    else if (walletNode.WalletAddress.StartsWith("0x"))
                    {
                        cuzdanIsmi = "AI_" + walletNode.WalletAddress.Substring(2, 4).ToUpper();
                    }
                    else
                    {
                        cuzdanIsmi = walletNode.WalletAddress.Replace("Cuzdan_", "") + " Cüzdanı";
                    }

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

            return Ok(new { nodes = nodesList, edges = edgesList });
        }

        // GET: api/wallet/bfs/{walletId}/{targetId}?minAmount=50
        [HttpGet("bfs/{walletId}/{targetId}")]
        public IActionResult RunBfs(string walletId, string targetId, [FromQuery] decimal minAmount = 0)
        {
            if (!_graph.Wallets.ContainsKey(walletId) || !_graph.Wallets.ContainsKey(targetId))
            {
                return NotFound(new { message = "Başlangıç veya Hedef cüzdan bulunamadı." });
            }

            List<string> traversalPath = _graph.BFS_TrackFundFlow(walletId, targetId, minAmount);
            return Ok(new { path = traversalPath });
        }

        // GET: api/wallet/dfs/{walletId}/{targetId}?minAmount=50
        [HttpGet("dfs/{walletId}/{targetId}")]
        public IActionResult RunDfs(string walletId, string targetId, [FromQuery] decimal minAmount = 0)
        {
            if (!_graph.Wallets.ContainsKey(walletId) || !_graph.Wallets.ContainsKey(targetId))
            {
                return NotFound(new { message = "Başlangıç veya Hedef cüzdan bulunamadı." });
            }

            List<string> traversalPath = _graph.DFS_DeepAnalysis(walletId, targetId, minAmount);
            return Ok(new { path = traversalPath });
        }

        // GET: api/wallet/merkle/{txId}
        [HttpGet("merkle/{txId}")]
        public IActionResult GetMerkleProof(string txId)
        {
            List<string> allTxIds = new List<string>();
            bool txExists = false;

            // Dinamik Veri Toplama: Sistemdeki (AI dahil) tüm transferleri bul
            foreach (var address in _graph.Wallets.Keys)
            {
                var walletNode = (WalletNode)_graph.Wallets[address];
                foreach (var tx in walletNode.OutgoingTransactions)
                {
                    allTxIds.Add(tx.TransactionId);
                    if (tx.TransactionId == txId) txExists = true;
                }
            }

            // Seçilen işlem grafikte yoksa hata dön
            if (!txExists)
            {
                return NotFound(new { message = "İşlem kimliği (TX) bulunamadı veya sahte işlem!" });
            }

            // Merkle Ağacı kuralı: Yaprak (İşlem) sayısı tek ise, son işlemi kopyalayarak çift yap
            if (allTxIds.Count % 2 != 0 && allTxIds.Count > 0)
            {
                allTxIds.Add(allTxIds[allTxIds.Count - 1]);
            }

            // Ağacı İnşa Et ve Kökü Bul
            MerkleTree tree = new MerkleTree();
            tree.BuildTree(allTxIds);

            return Ok(new
            {
                root = tree.MerkleRoot,
                selectedTx = txId,
                verified = true,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        // POST: api/wallet/ekle/{walletId}
        [HttpPost("ekle/{walletId}")]
        public IActionResult AddWallet(string walletId)
        {
            if (_graph.Wallets.ContainsKey(walletId))
            {
                return BadRequest(new { message = "Bu cüzdan zaten sistemde kayıtlı." });
            }

            _graph.AddWallet(walletId);
            return Ok(new { message = $"{walletId} başarıyla eklendi." });
        }

        // DELETE: api/wallet/sil/{walletId}
        [HttpDelete("sil/{walletId}")]
        public IActionResult DeleteWallet(string walletId)
        {
            if (!_graph.Wallets.ContainsKey(walletId))
            {
                return NotFound(new { message = "Silinecek cüzdan bulunamadı." });
            }

            _graph.RemoveWallet(walletId);
            return Ok(new { message = $"{walletId} başarıyla silindi." });
        }
    }
}
