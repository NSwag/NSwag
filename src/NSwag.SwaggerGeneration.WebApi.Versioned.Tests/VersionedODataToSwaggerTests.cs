namespace NSwag.SwaggerGeneration.WebApi.Versioned.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.OData.UriParser;
    using Microsoft.Web.Http;
    using OData.Models;
    using Xunit;

    public class VersionedODataToSwaggerTests
    {
        #region tests

        [Fact]
        public async Task Should_generate_correct_paths_for_odata_entity()
        {
            // Arrange
            var httpConfig = new HttpConfiguration();

            httpConfig.AddApiVersioning();

            var modelBuilder = new VersionedODataModelBuilder(httpConfig);
            modelBuilder.ModelConfigurations.Add(new DefaultUserConfiguration());
            httpConfig.MapVersionedODataRoutes("odata", "api", modelBuilder.GetEdmModels());

            var apiExplorer = httpConfig.AddODataApiExplorer();
            var settings = new VersionedWebApiToSwaggerGeneratorSettings();
            settings.ApiVersions.Add(new ApiVersion(1, 0));

            var generator = new VersionedWebApiToSwaggerGenerator(apiExplorer, settings);

            // Act
            var document = await generator.GenerateAsync().ConfigureAwait(false);

            // Assert
            var operations = document.Operations.ToArray();

            Assert.Contains(operations, o => o.Path.Equals("/api/Users") && o.Method.Equals("GET"));
            Assert.Contains(operations, o => o.Path.Equals("/api/Users") && o.Method.Equals("POST"));
            Assert.Contains(operations, o => o.Path.Equals("/api/Users({key})") && o.Method.Equals("GET"));
            Assert.Contains(operations, o => o.Path.Equals("/api/Users({key})") && o.Method.Equals("PATCH"));
        }

        [Fact]
        public async Task Should_not_generate_for_properties_ignored_in_modelconfiguration()
        {
            // Arrange
            var httpConfig = new HttpConfiguration();

            httpConfig.AddApiVersioning();

            var modelBuilder = new VersionedODataModelBuilder(httpConfig);
            modelBuilder.ModelConfigurations.Add(new FullNameIgnoredUserConfiguration());
            httpConfig.MapVersionedODataRoutes("odata", "api", modelBuilder.GetEdmModels());

            var apiExplorer = httpConfig.AddODataApiExplorer();
            var settings = new VersionedWebApiToSwaggerGeneratorSettings();
            settings.ApiVersions.Add(new ApiVersion(1, 0));

            var generator = new VersionedWebApiToSwaggerGenerator(apiExplorer, settings);

            // Act
            var document = await generator.GenerateAsync().ConfigureAwait(false);

            // Assert
            var user = document.Definitions["UserModel"];
            var properties = user.Properties.Values;

            Assert.Contains(properties, p => p.Name.Equals("Name"));
            Assert.Contains(properties, p => p.Name.Equals("Email"));
            Assert.Contains(properties, p => p.Name.Equals("Id"));

            Assert.DoesNotContain(properties, p => p.Name.Equals("FullName"));
        }

        [Fact]
        public async Task Should_generate_definition_for_odata_action_parameters()
        {
            // Arrange
            var httpConfig = new HttpConfiguration();

            httpConfig.AddApiVersioning();

            var modelBuilder = new VersionedODataModelBuilder(httpConfig);
            modelBuilder.ModelConfigurations.Add(new ActionParametersConfiguration());
            httpConfig.MapVersionedODataRoutes("odata", "api", modelBuilder.GetEdmModels());

            var apiExplorer = httpConfig.AddODataApiExplorer();
            var settings = new VersionedWebApiToSwaggerGeneratorSettings();
            settings.ApiVersions.Add(new ApiVersion(2, 0));

            var generator = new VersionedWebApiToSwaggerGenerator(apiExplorer, settings);

            // Act
            var document = await generator.GenerateAsync().ConfigureAwait(false);

            // Assert
            var parameterDefinition = document.Definitions["ActionWithParametersParameters"];
            var properties = parameterDefinition.Properties.Values;

            Assert.Contains(properties, p => p.Name.Equals("IntParameter"));
            Assert.Contains(properties, p => p.Name.Equals("StringParameter"));
            Assert.Contains(properties, p => p.Name.Equals("UserParameter"));
        }

        [Fact]
        public async Task Should_generate_odata_query_parameters_for_queryable_responses()
        {
            // Arrange
            var httpConfig = new HttpConfiguration();

            httpConfig.AddApiVersioning();

            var modelBuilder = new VersionedODataModelBuilder(httpConfig);
            modelBuilder.ModelConfigurations.Add(new QueryableConfiguration());
            httpConfig.MapVersionedODataRoutes("odata", "api", modelBuilder.GetEdmModels());

            var apiExplorer = httpConfig.AddODataApiExplorer();
            var settings = new VersionedWebApiToSwaggerGeneratorSettings();
            settings.ApiVersions.Add(new ApiVersion(3, 0));

            var generator = new VersionedWebApiToSwaggerGenerator(apiExplorer, settings);

            // Act
            var document = await generator.GenerateAsync().ConfigureAwait(false);

            // Assert
            var queryableOperation = document.Operations.Single(o => o.Path.Equals("/api/Queryable"));
            var parameters = queryableOperation.Operation.Parameters;

            Assert.Contains(parameters, p => p.Name.Equals("$select"));
            Assert.Contains(parameters, p => p.Name.Equals("$expand"));
            Assert.Contains(parameters, p => p.Name.Equals("$filter"));
            Assert.Contains(parameters, p => p.Name.Equals("$count"));
            Assert.Contains(parameters, p => p.Name.Equals("$orderby"));
            Assert.Contains(parameters, p => p.Name.Equals("$skip"));
            Assert.Contains(parameters, p => p.Name.Equals("$top"));
        }

        [Fact]
        public async Task OData_path_parameters_should_be_marked_as_from_path()
        {
            // Arrange
            var httpConfig = new HttpConfiguration();

            httpConfig.AddApiVersioning();

            var modelBuilder = new VersionedODataModelBuilder(httpConfig);
            modelBuilder.ModelConfigurations.Add(new FunctionWithManyPathParametersConfiguration());
            httpConfig.MapVersionedODataRoutes("odata", "api", modelBuilder.GetEdmModels());

            var apiExplorer = httpConfig.AddODataApiExplorer();
            var settings = new VersionedWebApiToSwaggerGeneratorSettings();
            settings.ApiVersions.Add(new ApiVersion(4, 0));

            var generator = new VersionedWebApiToSwaggerGenerator(apiExplorer, settings);

            // Act
            var document = await generator.GenerateAsync().ConfigureAwait(false);

            // Assert
            var queryableOperation = document.Operations.Single(o => o.Path.Equals("/api/FunctionWithManyParameters(integerParameter={integerParameter},stringParameter='{stringParameter}')"));
            var parameters = queryableOperation.Operation.Parameters;

            Assert.Equal(SwaggerParameterKind.Path, parameters.Single(p => p.Name == "integerParameter").Kind);
            Assert.Equal(SwaggerParameterKind.Path, parameters.Single(p => p.Name == "stringParameter").Kind);
        }

        #endregion

        #region modelconfigurations

        private class DefaultUserConfiguration : IModelConfiguration
        {
            public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
            {
                builder.EntitySet<UserModel>("Users");
            }
        }

        private class QueryableConfiguration : IModelConfiguration
        {
            public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
            {
                builder.EntitySet<UserModel>("Queryable");
            }
        }

        private class FullNameIgnoredUserConfiguration : IModelConfiguration
        {
            public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
            {
                var user = builder.EntitySet<UserModel>("Users").EntityType;
                user.Ignore(u => u.FullName);
            }
        }

        private class ActionParametersConfiguration : IModelConfiguration
        {
            public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
            {
                var action = builder.Action("ActionWithParameters");
                action.Parameter<int>("IntParameter");
                action.Parameter<string>("StringParameter");
                action.Parameter<UserModel>("UserParameter");
            }
        }

        private class FunctionWithManyPathParametersConfiguration : IModelConfiguration
        {
            public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
            {
                var function = builder.Function("FunctionWithManyParameters");
                function.Parameter<int>("integerParameter");
                function.Parameter<string>("stringParameter");
                function.Returns<string>();
            }
        }

        #endregion

        #region controllers

        [ApiVersion("1.0")]
        public class UsersController : ODataController
        {
            public IQueryable<UserModel> Get()
            {
                return new List<UserModel> { new UserModel() }.AsQueryable();
            }

            public SingleResult<UserModel> Get([FromODataUri] int key)
            {
                return new SingleResult<UserModel>(new List<UserModel> { new UserModel { Id = key } }.AsQueryable());
            }

            public async Task<IHttpActionResult> Post(Delta<UserModel> user)
            {
                return Ok(user);
            }

            public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<UserModel> user)
            {
                return Ok(user);
            }
        }

        [ApiVersion("2.0")]
        public class TestController : ODataController
        {
            [HttpPost]
            public async Task<IHttpActionResult> ActionWithParameters(ODataActionParameters parameters)
            {
                return Ok();
            }
        }

        [ApiVersion("3.0")]
        public class QueryableController : ODataController
        {
            [EnableQuery]
            public IQueryable<UserModel> Get()
            {
                return new List<UserModel> { new UserModel() }.AsQueryable();
            }

            [EnableQuery]
            public SingleResult<UserModel> Get(int key)
            {
                return new SingleResult<UserModel>(new List<UserModel>{ new UserModel() }.AsQueryable());
            }
        }

        [ApiVersion("4.0")]
        public class WithManyPathParametersController : ODataController
        {
            [HttpGet]
            public async Task<string> FunctionWithManyParameters([FromODataUri] int integerParameter, [FromODataUri] string stringParameter)
            {
                return string.Empty;
            }
        }

        #endregion
    }
}