namespace NSwag.SwaggerGeneration.WebApi.Versioned.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using Annotations;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using Microsoft.Web.Http.Routing;
    using Microsoft.Web.Http.Versioning;
    using Xunit;

    public class VersionedWebApiToSwaggerGenerationTests
    {
        #region tests

        [Fact]
        public async Task Should_only_generate_operations_for_one_version_when_selecting_one_version()
        {
            // Arrange
            var httpConfig = new HttpConfiguration();
            httpConfig.AddApiVersioning();
            httpConfig.MapHttpAttributeRoutes();

            var apiExplorer = httpConfig.AddVersionedApiExplorer();

            var settings = new VersionedWebApiToSwaggerGeneratorSettings();
            settings.ApiVersions.Add(new ApiVersion(1, 0));

            var generator = new VersionedWebApiToSwaggerGenerator(apiExplorer, settings);

            // Act
            var document = await generator.GenerateAsync().ConfigureAwait(false);

            // Assert
            var operations = document.Operations.ToArray();

            Assert.Contains(operations, o => o.Path.Equals("/api/versionedtest/getv1"));
            Assert.DoesNotContain(operations, o => o.Path.Equals("/api/versionedtest/getv2"));
        }

        [Fact]
        public async Task Should_not_generate_for_operations_with_swaggerignore_attribute()
        {
            // Arrange
            var httpConfig = new HttpConfiguration();
            httpConfig.AddApiVersioning();
            httpConfig.MapHttpAttributeRoutes();

            var apiExplorer = httpConfig.AddVersionedApiExplorer();

            var settings = new VersionedWebApiToSwaggerGeneratorSettings();
            settings.ApiVersions.Add(new ApiVersion(1, 0));

            var generator = new VersionedWebApiToSwaggerGenerator(apiExplorer, settings);

            // Act
            var document = await generator.GenerateAsync().ConfigureAwait(false);

            // Assert
            var operations = document.Operations.ToArray();

            Assert.DoesNotContain(operations, o => o.Path.Equals("/api/withignoredoperation/get"));
        }

        [Fact]
        public async Task Should_not_generate_for_controller_with_swaggerignore_attribute()
        {
            // Arrange
            var httpConfig = new HttpConfiguration();
            httpConfig.AddApiVersioning();
            httpConfig.MapHttpAttributeRoutes();

            var apiExplorer = httpConfig.AddVersionedApiExplorer();

            var settings = new VersionedWebApiToSwaggerGeneratorSettings();
            settings.ApiVersions.Add(new ApiVersion(1, 0));

            var generator = new VersionedWebApiToSwaggerGenerator(apiExplorer, settings);

            // Act
            var document = await generator.GenerateAsync().ConfigureAwait(false);

            // Assert
            var operations = document.Operations.ToArray();

            Assert.DoesNotContain(operations, o => o.Path.Equals("/api/ignoredcontroller/get"));
        }

        [Fact]
        public async Task Should_generate_operations_for_all_versions_when_setting_multiple_versions_while_using_url_segment_versioning()
        {
            // Arrange
            var httpConfig = new HttpConfiguration();
            httpConfig.AddApiVersioning(options =>
            {
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            httpConfig.Routes.MapHttpRoute(
                "VersionedGetUrl",
                "api/v{version}/{controller}/get",
                defaults: null,
                constraints: new
                {
                    version = new ApiVersionRouteConstraint(),
                } );

            var apiExplorer = httpConfig.AddVersionedApiExplorer();

            var settings = new VersionedWebApiToSwaggerGeneratorSettings();
            settings.ApiVersions.Add(new ApiVersion(3, 0));
            settings.ApiVersions.Add(new ApiVersion(4, 0));

            var generator = new VersionedWebApiToSwaggerGenerator(apiExplorer, settings);

            // Act
            var document = await generator.GenerateAsync().ConfigureAwait(false);

            // Assert
            var operations = document.Operations.ToArray();

            Assert.Contains(operations, o => o.Path.Equals("/api/v3.0/VersionBySegment/get"));
            Assert.Contains(operations, o => o.Path.Equals("/api/v4.0/VersionBySegment/get"));
            Assert.DoesNotContain(operations, o => o.Path.Equals("/api/v2.0/VersionBySegment/get"));
        }

        #endregion

        #region controllers
        [RoutePrefix("api/versionedtest")]
        [ApiVersion("1.0")]
        [ApiVersion("2.0")]
        public class VersionedTestController : ApiController
        {
            [MapToApiVersion("1.0")]
            [Route("getv1")]
            public IEnumerable<TestModel> GetV1()
            {
                return new List<TestModel> { new TestModel() };
            }

            [MapToApiVersion("2.0")]
            [Route("getv2")]
            public IEnumerable<TestModel> GetV2()
            {
                return new List<TestModel> { new TestModel() };
            }
        }

        [RoutePrefix("api/withignoredoperation")]
        [ApiVersion("1.0")]
        public class WithIgnoredOperationController : ApiController
        {
            [SwaggerIgnore]
            [Route("get")]
            public IEnumerable<TestModel> Get()
            {
                return new List<TestModel> { new TestModel() };
            }
        }

        [RoutePrefix("api/ignoredcontroller")]
        [ApiVersion("1.0")]
        [SwaggerIgnore]
        public class IgnoredController : ApiController
        {
            [Route("get")]
            public IEnumerable<TestModel> Get()
            {
                return new List<TestModel> { new TestModel() };
            }
        }

        /* Use versions unused in other tests to not create ambiguous routes in test that do not use versioning by url segment.
        Users that want to create a swagger document for controllers with methods that map multiple versions to the same route
        (e.g. when using versioning by header or query) should register a swagger generator per version.
        */
        [ApiVersion("3.0")]
        [ApiVersion("4.0")]
        public class VersionBySegmentController : ApiController
        {
            [Route("get")]
            public IEnumerable<TestModel> Get()
            {
                return new List<TestModel> { new TestModel() };
            }
        }

        public class TestModel {}

        #endregion
    }
}