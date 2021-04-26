using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NSwag.Sample
{
    // Intentionally not sharing the controller source code with the 1.1 and 2.0 samples.
    // 2.1 introduces enhancements to the programming model for API but expects you to
    // write some different code.
    //
    // This sample is similar to http://petstore.swagger.io/#/
    [ApiController]
    [Route("/pet")]
    public class PetController : ControllerBase
    {
        /// <summary>
        /// Creates an order
        /// </summary>
        /// <response code="201">Order created</response>
        /// <response code="400">Order invalid</response>
        [HttpPost("createOrder")]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        public void CreateOrder()
        {

        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [return: Description("My success response description.")]
        public async Task<IActionResult> AddPet([FromBody] Pet pet)
        {
            await Task.Delay(0);
            return new EmptyResult();
        }

        [HttpPut]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditPet([FromBody] Pet pet)
        {
            if (pet.Id == 0)
            {
                return NotFound();
            }

            await Task.Delay(0);
            return new EmptyResult();
        }

        // 'status' is intended to be an optional query string parameter
        // Sample with ActionResult<T> auto-detection
        [HttpGet("findByStatus")]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<Pet>>> FindByStatus(string[] status)
        {
            await Task.Delay(0);
            return new ObjectResult(Array.Empty<Pet>());
        }

        // Included this extra action not present in http://petstore.swagger.io/#/
        // to represent an action with a required query parameter.
        [HttpGet("findByCategory")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Pet>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Pet>>> FindByCategory([BindRequired] string category)
        {
            await Task.Delay(0);
            return new ObjectResult(Array.Empty<Pet>());
        }

        [HttpGet("{petId}", Name = "FindPetById")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Pet), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Pet>> FindById(int petId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await Task.Delay(0);
            if (petId == 0)
            {
                return NotFound();
            }

            return Ok(new Pet());
        }

        [HttpPost("{petId}")]
        [Consumes("application/www-form-url-encoded")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditPet(int petId, [FromForm] Pet pet)
        {
            if (petId == 0)
            {
                return NotFound();
            }

            await Task.Delay(0);
            return new EmptyResult();
        }

        [HttpDelete("{petId}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePet(int petId)
        {
            if (petId == 0)
            {
                return NotFound();
            }

            await Task.Delay(0);
            return new EmptyResult();
        }

        [HttpPost("{petId}/uploadImage")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SerializableError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> UploadImage(int petId, IFormFile file)
        {
            if (petId == 0)
            {
                return NotFound();
            }

            await Task.Delay(0);
            return Ok(new ApiResponse());
        }

        [HttpPost("file")]
        public ActionResult GetFile()
        {
            return NoContent();
        }
    }
}