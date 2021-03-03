using System.Linq;
using Postman.Data;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Postman.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<MailController> _logger;
        private readonly HttpClient _client;

        public MailController(AppDbContext dbContext, ILogger<MailController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _client = new HttpClient();
            _dbContext.Database.EnsureCreated();
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(string))]
        public async Task<IActionResult> Get()
        {
            try 
            {
                await Task.WhenAll(_dbContext.Mails.OrderBy(x => x.Receiver).ToList().Select(x =>
                {
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:{x.Address}"))
                    {
                        requestMessage.Headers.Add("mail-message", x.Message);
                        _logger.LogInformation($"Sending mail to {x.Receiver}");
                        return _client.SendAsync(requestMessage);
                    }
                }));
            }
            catch(HttpRequestException e) 
            {
                _logger.LogError(e.Message);
            }
            
            return Ok("Sent");
        }
    }
}
