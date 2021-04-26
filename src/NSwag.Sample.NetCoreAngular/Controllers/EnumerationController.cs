using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Sample.Common;

namespace NSwag.Sample.NetCoreAngular.Controllers
{
    [Route("api/[controller]")]
    public class EnumerationController : Controller
    {
        [HttpGet]
        [Route("[action]")]
        public Task<IEnumerable<FileType>> ReverseQueryEnumList([FromQuery]IEnumerable<FileType> fileTypes)
        {
            return Task.FromResult(fileTypes.Reverse());
        }
    }
}