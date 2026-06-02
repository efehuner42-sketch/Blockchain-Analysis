using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BlockchainAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public AIController()
        {
            // Docker ağında Python servisimizin adı 'ai-service' ve portu 8000 olarak ayarlandı
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://ai-service:8000/")
            };
        }

        // GET ve POST rotalarımız karışmasın diye özel bir uç nokta belirliyoruz
        [HttpPost("sentetik-veri-uret")]
        public async Task<IActionResult> GenerateData([FromBody] AIDataRequest request)
        {
            try
            {
                // C#'taki nesnemizi JSON formatına (Python'un anlayacağı dile) çeviriyoruz
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                // Python AI servisimize POST isteğini yolluyoruz
                HttpResponseMessage response = await _httpClient.PostAsync("api/ai/generate-synthetic-data", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    // Python'dan gelen başarılı cevabı (sentetik cüzdan listesini) okuyup Berke'ye iletiyoruz
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    return Ok(JsonDocument.Parse(jsonResult));
                }

                return StatusCode((int)response.StatusCode, new { message = "AI servisinden hata döndü." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Bağlantı hatası: Docker içindeki AI servisine ulaşılamadı. Hata: {ex.Message}" });
            }
        }
    }

    // Python'daki FastAPI'nin beklediği veri yapısı (DataRequest modeli)
    public class AIDataRequest
    {
        public int wallet_count { get; set; }
        public int transaction_count { get; set; }
    }
}