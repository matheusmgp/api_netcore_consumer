using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using orderConsumerAPI.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace orderConsumerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RabbitMQController : ControllerBase
    {
        private readonly IRabbitMQService _rabbitMqService;

        public RabbitMQController(IRabbitMQService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Pong");
        }

        [HttpGet("StopService")]
        public IActionResult StopService()
        {
            _rabbitMqService.StopAsync(new System.Threading.CancellationToken());
            return Ok("Service Stopped!");
        }

        [HttpGet("StartService")]
        public IActionResult StartService()
        {
            _rabbitMqService.StartAsync(new System.Threading.CancellationToken());
            return Ok("Service Started!");
        }
    }
}
