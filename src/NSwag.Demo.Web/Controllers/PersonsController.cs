using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using NSwag.Annotations;
using NSwag.Demo.Web.Models;
using NSwag.Generation.WebApi;

namespace NSwag.Demo.Web.Controllers
{
    public class MyClass
    {
        public string Foo { get; set; }
    }

    [RoutePrefix("api/Person")]
    public class PersonsController : ApiController
    {
        [HttpPut]
        [Route("xyz/{data}")]
        public string Xyz(MyClass data)
        {
            return "abc";
        }

        // GET: api/Person
        [Obsolete]
        public IEnumerable<Person> Get()
        {
            return new Person[]
            {
                new Person { FirstName = "Foo", LastName = "Bar"},
                new Person { FirstName = "Rico", LastName = "Suter"},
            };
        }

        // GET: api/Person/5
        /// <summary>Gets a person.</summary>
        /// <param name="id">The ID of
        /// the person.</param>
        /// <returns>The person.</returns>
        [ResponseType(typeof(Person))]
        [ResponseType("500", typeof(PersonNotFoundException))]
        [Route("{id}")]
        public HttpResponseMessage Get(int id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new Person { FirstName = "Rico", LastName = "Suter" });
        }

        // POST: api/Person
        /// <summary>Creates a new person.</summary>
        /// <param name="value">The person.</param>
        [HttpPost, Route("")]
        public void Post([FromBody]Person value)
        {
        }

        // PUT: api/Person/5
        /// <summary>Updates the existing person.</summary>
        /// <param name="id">The ID.</param>
        /// <param name="value">The person.</param>
        public void Put(int id, [FromBody]Person value)
        {
        }

        // DELETE: api/Person/5
        [Route("{id}")]
        public void Delete(int id)
        {
        }

        [HttpGet]
        [Route("Calculate/{a}/{b}")]
        [Description("Calculates the sum of a, b and c.")]
        public int Calculate(int a, int b, [Required]int c)
        {
            return a + b + c;
        }

        [HttpGet]
        [OpenApiIgnore]
        public async Task<HttpResponseMessage> Swagger()
        {
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = Configuration.Routes.First(r => !string.IsNullOrEmpty(r.RouteTemplate)).RouteTemplate
            });
            var document = await generator.GenerateForControllerAsync(GetType());
            return new HttpResponseMessage { Content = new StringContent(document.ToJson(), Encoding.UTF8) };
        }
    }

    public class PersonNotFoundException : Exception
    {
        public PersonNotFoundException()
        {
        }

        public int PersonId { get; set; }
    }
}
