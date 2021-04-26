using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace NSwag.Sample.NetCoreAngular.Controllers
{
    [Route("api/[controller]")]
    public class FileController : Controller
    {
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
    }
}