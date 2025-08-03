using Microsoft.AspNetCore.Mvc;

namespace MtgManager.API
{
    [ApiController]
    [Route("[controller]")]
    public class APIController : ControllerBase
    {
        private readonly ILogger<APIController> _logger;

        public APIController(ILogger<APIController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<int> Get()
        {
            
            var rand = new Random();
            return Enumerable.Range(1, 10).Select(x => rand.Next(0, x));
        }
    }
}
