using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MainServer.Controllers
{
    [Route("agents")]
    [ApiController]
    public class AgentsController:Controller
    {
        private readonly Manager _manager;
        private readonly ILogger _logger;
        public AgentsController(Manager manager,ILogger<AgentsController> logger)
        {
            _logger = logger;
            _manager = manager;
        }
        
        [HttpGet("{id:guid}")]
        public IActionResult SetStatus(Guid id, [FromQuery]string status)
        {
            _logger.LogInformation($"{id} status {status}");
            _manager.Update(id, status);
            
            return Ok();
        }
        
        [HttpGet()]
        public async Task<IActionResult> Start([FromQuery]string serverType,[FromQuery]int podCount)
        {
            await _manager.DeployPodAsync(serverType, podCount);
            
            return Ok();
        }
        
        [HttpGet("clear")]
        public async Task<IActionResult> Clear()
        {
            await _manager.ClearAsync();
            
            return Ok();
        }
    }
}