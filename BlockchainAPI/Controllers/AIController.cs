using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlockchainCore;

namespace BlockchainAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public AIController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://ai-service:8000/");
        }

        [HttpPost("sentetik-veri-uret")]
        public async Task<IActionResult> GenerateData([FromBody] AIDataRequest request)
        {
            try
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response = await _httpClient.PostAsync("api/ai/generate-synthetic-data", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();

                    // Python'dan gelen yanıtı C# sınıf yapısına çözümlüyoruz
                    var aiResponse = JsonSerializer.Deserialize<PythonAIResponse>(jsonResult);

                    if (aiResponse != null && aiResponse.status == "success")
                    {
                        // Hafızada tertemiz, yepyeni bir graf nesnesi başlatıyoruz
                        var newGraph = new BlockchainGraph();

                        // 1. Yapay zekanın ürettiği tüm cüzdanları C# grafına ekle
                        foreach (var walletAddress in aiResponse.generated_wallets)
                        {
                            newGraph.AddWallet(walletAddress);
                        }

                        // 2. Yapay zekanın ürettiği tüm transferleri (kenarları) C# grafına ekle
                        foreach (var tx in aiResponse.generated_transactions)
                        {
                            newGraph.AddTransaction(
                                tx.sender_address, 
                                tx.receiver_address, 
                                tx.tx_id, 
                                tx.amount
                            );
                        }

                        WalletController._graph = newGraph;
                    }

                    return Ok(JsonDocument.Parse(jsonResult).RootElement);
                }

                return StatusCode((int)response.StatusCode, new { message = "AI servisinden hata döndü." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Bağlantı hatası: Docker içindeki AI servisine ulaşılamadı. Hata: {ex.Message}" });
            }
        }
    }

    // --- C# İÇİN JSON DESERİALİZE MODELLERİ ---
    public class AIDataRequest
    {
        public int wallet_count { get; set; }
        public int transaction_count { get; set; }
    }

    public class PythonAIResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public List<string> generated_wallets { get; set; }
        public List<AITransactionDetail> generated_transactions { get; set; }
    }

    public class AITransactionDetail
    {
        public string tx_id { get; set; }
        public string sender_address { get; set; }
        public string receiver_address { get; set; }
        public decimal amount { get; set; }
        public long timestamp { get; set; }
    }
}
