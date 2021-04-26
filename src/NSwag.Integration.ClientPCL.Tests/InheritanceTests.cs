using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSwag.Integration.ClientPCL.Contracts;

namespace NSwag.Integration.ClientPCL.Tests
{
    [TestClass]
    public class InheritanceTests
    {
        [TestMethod]
        [TestCategory("integration")]
        public async Task When_Get_is_called_then_Teacher_is_returned()
        {
            // Arrange
            var personsClient = new PersonsClient(new HttpClient()) { BaseUrl = "http://localhost:13452" };

            // Act
            var result = await personsClient.GetAsync(new Guid());

            // Assert
            Assert.IsTrue(result.Result.GetType() == typeof(Teacher));
        }

        [TestMethod]
        [TestCategory("integration")]
        public async Task When_Teacher_is_sent_to_Transform_it_is_transformed_and_correctly_sent_back()
        {
            // Arrange
            var personsClient = new PersonsClient(new HttpClient()) { BaseUrl = "http://localhost:13452" };

            // Act
            var result = await personsClient.TransformAsync(new Teacher
            {
                FirstName = "foo",
                LastName = "bar",
                Course = "SE"
            });

            // Assert
            Assert.IsTrue(result.Result.GetType() == typeof(Teacher));
            var teacher = (Teacher)result.Result;
            Assert.AreEqual("FOO", teacher.FirstName);
            Assert.AreEqual("SE", teacher.Course);
        }
    }
}
