using Microsoft.AspNetCore.Mvc;

namespace BlockchainAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetDummyWallets()
        {
            var dummyWallets = new[]
            {
                new { Id = 1, Address = "0xA1B2C3...", Balance = 150.5 },
                new { Id = 2, Address = "0x9F8E7D...", Balance = 42.0 },
                new { Id = 3, Address = "0x112233...", Balance = 0.5 }
            };

            return Ok(dummyWallets);
        }
    }
}