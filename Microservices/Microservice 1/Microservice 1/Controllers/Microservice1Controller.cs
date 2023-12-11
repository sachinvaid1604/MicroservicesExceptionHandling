using Microsoft.AspNetCore.Mvc;

namespace ExceptionHandlingInMicroservices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Microservice1Controller : ControllerBase
    {
        public Microservice1Controller()
        {
        }

        [HttpGet(Name = "CallFirstMicroService")]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Make a request to Microservice 2
                var response = await new HttpClient().GetAsync("https://localhost:7057/Microservice2");


                //  response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode) { return Ok("Request processed successfully"); } //Do something with the response code 200}

                // Deserialize the response content to a custom object
                var responseObject = await response.Content.ReadAsAsync<ResponseClass>();

                // Process the responseObject if needed

                return StatusCode(responseObject.StatusCode, responseObject);

            }
            catch (HttpRequestException ex)
            {
                // Log the exception or handle it accordingly
                // ...
                return StatusCode(500);
              
            }
            catch (Exception ex)
            {
                // Log the unexpected exception or handle it accordingly
                return StatusCode(500);
            }
        }
    }

    public class ResponseClass
    {
        public string Content { get; set; }
        public string Message { get; set; }

        public int StatusCode { get; set; }
    }

}
