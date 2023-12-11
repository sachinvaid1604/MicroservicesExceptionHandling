using Microsoft.AspNetCore.Mvc;

namespace ExceptionHandlingInMicroservices.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class Microservice2Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult GetMicroservice2Data()
        {

            int numerator = 2;
            int denominator = 0;

            var valueNotFoundInRequest = numerator / denominator; // Exception Occurs here and not handled. This will be handled at the Microservice Level common Logging. 

            return Ok("Got the data");
        }
    }


    public class ResponseClass
    {
        public string Content { get; set; }
        public string Message { get; set; }

        public int StatusCode { get; set; }
    }


}
