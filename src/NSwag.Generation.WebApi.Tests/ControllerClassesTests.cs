using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSwag.Generation.WebApi;

namespace NSwag.Generation.WebApi.Tests
{
    public class ControllerBase
    {
    }

    public class Controller : ControllerBase
    {
    }

    public class MyAspCore : Controller
    {
    }

    public class MyController
    {
    }

    public class MyWebApi : ApiController
    {
    }

    public class MyLegacyMvcController : IController
    {
        public void Execute(RequestContext requestContext)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class MyAbstractController
    {
    }

    [TestClass]
    public class ControllerClassesTests
    {
        [TestMethod]
        public void When_class_inherits_from_ControllerBase_then_it_is_found()
        {
            // Arrange


            // Act
            var controllerClasses = WebApiOpenApiDocumentGenerator.GetControllerClasses(typeof(ControllerClassesTests).Assembly);

            // Assert
            Assert.IsTrue(controllerClasses.Contains(typeof(MyAspCore)));
        }

        [TestMethod]
        public void When_class_ends_with_Controller_then_it_is_found()
        {
            // Arrange


            // Act
            var controllerClasses = WebApiOpenApiDocumentGenerator.GetControllerClasses(typeof(ControllerClassesTests).Assembly);

            // Assert
            Assert.IsTrue(controllerClasses.Contains(typeof(MyController)));
        }

        [TestMethod]
        public void When_class_inherits_from_ApiController_then_it_is_found()
        {
            // Arrange


            // Act
            var controllerClasses = WebApiOpenApiDocumentGenerator.GetControllerClasses(typeof(ControllerClassesTests).Assembly);

            // Assert
            Assert.IsTrue(controllerClasses.Contains(typeof(MyWebApi)));
        }

        [TestMethod]
        public void When_class_is_MVC_controller_then_it_is_ignored()
        {
            // Arrange


            // Act
            var controllerClasses = WebApiOpenApiDocumentGenerator.GetControllerClasses(typeof(ControllerClassesTests).Assembly);

            // Assert
            Assert.IsFalse(controllerClasses.Contains(typeof(MyLegacyMvcController)));
        }

        [TestMethod]
        public void When_class_is_abstract_then_it_is_ignored()
        {
            // Arrange


            // Act
            var controllerClasses = WebApiOpenApiDocumentGenerator.GetControllerClasses(typeof(ControllerClassesTests).Assembly);

            // Assert
            Assert.IsFalse(controllerClasses.Contains(typeof(MyAbstractController)));
        }
    }
}
