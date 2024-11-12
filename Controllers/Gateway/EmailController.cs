using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ApiGateway.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using ApiGateway.Services;

namespace ApiGateway.Controllers.Gateway
{
    //test api gateway

    [ApiController]
    [Route("gateway/[controller]")]
    public class EmailController : ControllerBase
    {
        // GET: gateway/email/send
        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveEmail()
        {
            // Retrieve the user from HttpContext (set in the middleware)
            var user = HttpContext.Items["User"] as User;

            if (user == null)
            {
                return Unauthorized();
            }

            // Proceed with receiving email logic
            // ...

            return Ok("Email received successfully.");
        }

        // Post: gateway/email/send
        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailModel model)
        {
            // Retrieve the user from HttpContext (set in the middleware)
            var user = HttpContext.Items["User"] as User;

            if (user == null)
            {
                return Unauthorized();
            }

            // Proceed with sending email logic
            // ...

            var emailModel = new EmailModel
            {
                To = model.To,
                From = model.From,
                Subject = model.Subject,
                Body = model.Body
            };

            // dont return object return that client can see dont return emailModel
            return Ok(emailModel);
        }

    }


    //[ApiController]
    //[Route("gateway/[controller]")]
    //public class EmailController : ControllerBase
    //{
    //    private readonly RabbitMQPublisherService _rabbitMQPublisherService;

    //    public EmailController(RabbitMQPublisherService rabbitMQPublisherService)
    //    {
    //        _rabbitMQPublisherService = rabbitMQPublisherService;
    //    }

    //    // POST: gateway/email/send
    //    [HttpPost("send")]
    //    public async Task<IActionResult> SendEmail([FromBody] EmailModel request)
    //    {
    //        // Retrieve the user from HttpContext (set in the middleware)
    //        var user = HttpContext.Items["User"] as User;

    //        if (user == null)
    //        {
    //            return Unauthorized();
    //        }

    //        // Validate the request model
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        // Optionally, you might include user info in the email data or message properties
    //        var emailMessage = new EmailModel
    //        {
    //            To = request.To,
    //            From = request.From,
    //            Subject = request.Subject,
    //            Body = request.Body,
    //        };

    //        // Publish the email message to the RabbitMQ queue
    //        await _rabbitMQPublisherService.PublishEmailMessageAsync(emailMessage);

    //        // Return an appropriate response
    //        return Ok(new { Message = "Email request accepted for processing." });
    //    }
    //}
}