using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NSwag.Generation.WebApi.Tests.Attributes
{
    [TestClass]
    public class RoutePrefixTests
    {
        [RoutePrefix("api/Persons")]
        public class PersonsController : ApiController
        {
           // GET api/values
            [HttpGet]
            public IEnumerable<Person> Get()
            {
                throw new NotImplementedException();
            }

            // GET api/values/5
            [HttpGet, Route("{id}")]
            public Person Get(int id)
            {
                throw new NotImplementedException();
            }

            // POST api/values
            [HttpPost]
            public void Post([FromBody]Person value)
            {
                throw new NotImplementedException();
            }

            // PUT api/values/5
            [HttpPut, Route("{id}")]
            public void Put(int id, [FromBody]Person value)
            {
                throw new NotImplementedException();
            }

            // DELETE api/values/5
            [HttpDelete, Route("{id}")]
            public void Delete(int id)
            {
                throw new NotImplementedException();
            }

            [Route("RegexPathParameter/{deviceType:regex(^pulse-\\d{{2}})}/{deviceId:int}/energyConsumed")]
            public void RegexPathParameter(string deviceType, int deviceId)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public async Task When_controller_has_RoutePrefix_then_paths_are_correct()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var swagger = await generator.GenerateForControllerAsync<PersonsController>();

            // Assert
            Assert.IsNotNull(swagger.Paths["/api/Persons"][OpenApiOperationMethod.Get]);
            Assert.IsNotNull(swagger.Paths["/api/Persons/{id}"][OpenApiOperationMethod.Get]);
            Assert.IsNotNull(swagger.Paths["/api/Persons"][OpenApiOperationMethod.Post]);
            Assert.IsNotNull(swagger.Paths["/api/Persons/{id}"][OpenApiOperationMethod.Put]);
            Assert.IsNotNull(swagger.Paths["/api/Persons/{id}"][OpenApiOperationMethod.Delete]);
            Assert.IsTrue(swagger.Paths.Count == 3);
            Assert.IsTrue(swagger.Paths.SelectMany(p => p.Value).Count() == 6);
        }

        [TestMethod]
        public async Task When_route_contains_complex_path_parameter_then_it_is_correctly_parsed()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var swagger = await generator.GenerateForControllerAsync<PersonsController>();
            var json = swagger.ToJson(); 

            // Assert
            Assert.IsTrue(swagger.Paths.ContainsKey("/api/Persons/RegexPathParameter/{deviceType}/{deviceId}/energyConsumed"));
        }

        public class Person
        {
            public string Name { get; set; }
        }

        [RoutePrefix("api/users/{userId?}")]
        public class UsersController : ApiController
        {
            [HttpGet]
            public IEnumerable<Person> Get()
            {
                throw new NotImplementedException();
            }
            
            [HttpGet]
            public Person Get(string userId)
            {
                throw new NotImplementedException();
            }
            
            [HttpPost]
            public void Post([FromBody]Person value)
            {
                throw new NotImplementedException();
            }
            
            [HttpPut]
            public void Put(string userId, [FromBody]Person value)
            {
                throw new NotImplementedException();
            }
            
            [HttpDelete]
            public void Delete(string userId)
            {
                throw new NotImplementedException();
            }

            [HttpGet, Route("devices")]
            public IEnumerable<string> GetNestedPath(string userId)
            {
                throw new NotImplementedException();
            }

            [HttpGet, Route("devices/{deviceId}")]
            public string GetNestedRequiredPathParameter(string userId, string deviceId)
            {
                throw new NotImplementedException();
            }
        }
        
        [TestMethod]
        public async Task When_controller_has_RoutePrefix_with_optional_parameters_then_paths_are_correct()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var swagger = await generator.GenerateForControllerAsync<UsersController>();

            // Assert
            Assert.IsNotNull(swagger.Paths["/api/users"][OpenApiOperationMethod.Get]);
            Assert.IsNotNull(swagger.Paths["/api/users/{userId}"][OpenApiOperationMethod.Get]);
            Assert.IsNotNull(swagger.Paths["/api/users"][OpenApiOperationMethod.Post]);
            Assert.IsNotNull(swagger.Paths["/api/users/{userId}"][OpenApiOperationMethod.Put]);
            Assert.IsNotNull(swagger.Paths["/api/users/{userId}"][OpenApiOperationMethod.Delete]);
            Assert.IsNotNull(swagger.Paths["/api/users/{userId}/devices"][OpenApiOperationMethod.Get]);
            Assert.IsNotNull(swagger.Paths["/api/users/{userId}/devices/{deviceId}"][OpenApiOperationMethod.Get]);
            Assert.IsTrue(swagger.Paths.Count == 4);
            Assert.IsTrue(swagger.Paths.SelectMany(p => p.Value).Count() == 7);
        }
    }
}
