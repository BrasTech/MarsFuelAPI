using MarsFuelAPI.Classes;
using MarsFuelAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarsFuelAPI.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class StateController : ControllerBase
    {
        private readonly BackgroundService _service;

        public StateController(BackgroundService service)
        {
            _service = service;
        }

        [HttpPost("config")]
        public IActionResult Config([FromBody] InputData inputData)
        {
            ConfigureService(inputData);
            return Ok();
        }
        [HttpGet("stop")]
        public IActionResult Stop()
        {
            StopService();

            return Ok();
        }

        [HttpPost("init")]
        public IActionResult Init([FromBody] List<MonthInit> monthInits)
        {
            InitService(monthInits);
            return Ok();
        }

        [HttpPut("change-month")]
        public IActionResult Change([FromBody] MonthModificator monthModificator)
        {
            _service.ChangeMonthData(monthModificator);
            return Ok();
        }
        [HttpPost("addMonth")]
        public IActionResult AddMonth([FromQuery] double value)
        {
            _service.AddMonthData(value);
            return Ok();
        }
        private void InitService(List<MonthInit> monthInits)
        {
            if (_service.isRunning)
                StopService();
            _ = _service.Start(monthInits);
        }
        private void StopService()
        {
            _service.Stop();
        }
        private void ConfigureService(InputData inputData)
        {
            if (_service.isRunning)
                StopService();
            _service.Configure(inputData);
        }
    }
}
