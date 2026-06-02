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
        private static readonly BlockchainGraph _graph;

        static WalletController()
        {
            _graph = TestDataGenerator.GenerateTestData();
        }

        // GET: api/wallet
        [HttpGet]
        public IActionResult GetFullGraph()
        {
            var nodesList = new List<object>();
            var edgesList = new List<object>();

            foreach (var address in _graph.Wallets.Keys)
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

        // GET: api/wallet/bfs/{walletId}/{targetId}
        [HttpGet("bfs/{walletId}/{targetId}")]
        public IActionResult RunBfs(string walletId, string targetId)
        {
            if (!_graph.Wallets.ContainsKey(walletId) || !_graph.Wallets.ContainsKey(targetId))
            {
                return NotFound(new { message = "Başlangıç veya Hedef cüzdan bulunamadı." });
            }

            List<string> traversalPath = _graph.BFS_TrackFundFlow(walletId, targetId);

            if (traversalPath.Count == 0)
            {
                return Ok(new { message = $"{walletId} cüzdanından {targetId} cüzdanına herhangi bir fon akışı bulunamadı.", path = traversalPath });
            }

            return Ok(new
            {
                message = $"BFS Algoritması çalıştı. {walletId} -> {targetId} rotası bulundu.",
                path = traversalPath
            });
        }

        // GET: api/wallet/dfs/{walletId}/{targetId}
        [HttpGet("dfs/{walletId}/{targetId}")]
        public IActionResult RunDfs(string walletId, string targetId)
        {
            if (!_graph.Wallets.ContainsKey(walletId) || !_graph.Wallets.ContainsKey(targetId))
            {
                return NotFound(new { message = "Başlangıç veya Hedef cüzdan bulunamadı." });
            }

            List<string> traversalPath = _graph.DFS_DeepAnalysis(walletId, targetId);

            if (traversalPath.Count == 0)
            {
                return Ok(new { message = $"{walletId} cüzdanından {targetId} cüzdanına derinlemesine bir bağlantı bulunamadı.", path = traversalPath });
            }

            return Ok(new
            {
                message = $"DFS Algoritması çalıştı. {walletId} -> {targetId} rotası bulundu.",
                path = traversalPath
            });
        }

        // GET: api/wallet/merkle/{txId}
        [HttpGet("merkle/{txId}")]
        public IActionResult GetMerkleProof(string txId)
        {
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