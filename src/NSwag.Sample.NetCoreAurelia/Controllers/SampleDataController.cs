using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using NSwag.Sample.Common;

namespace NSwag_Sample_NetCoreAurelia.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("[action]")]
        public IEnumerable<WeatherForecast> WeatherForecasts()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index).ToString("d"),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });
        }

        [HttpGet]
        [Route("[action]")]
        public Task<FileContentResult> GetFile(string fileName)
        {
            var result = new FileContentResult(new byte[] { 1, 2, 3 }, new MediaTypeHeaderValue("application/octet-stream"))
            {
                FileDownloadName = fileName
            };
            return Task.FromResult(result);
        }

        [HttpDelete]
        [ProducesResponseType(200)]
        public Task<IActionResult> DeleteShop([FromQuery]Guid id, [FromHeader]List<string> additionalIds)
        {
            return Task.FromResult<IActionResult>(Ok());
        }
    }
}
