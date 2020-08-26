using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AzSendGridAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private readonly SendGridClientOptions _defaultOptions = new SendGridClientOptions();

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

       
       [HttpPost]
        public async Task SendEmail(string _from, string _to, string _subject="test subject",string _content="test content", string _htmlcontent="test html content")
        {
            //Sendgrid client options
            var options= new SendGridClientOptions{
                Host = _configuration.GetSection("SendGrid").GetSection("host").Value,
                ApiKey = _configuration.GetSection("SendGrid").GetSection("apikey").Value,
                Auth = new AuthenticationHeaderValue("Bearer", _configuration.GetSection("SendGrid").GetSection("bearertoken").Value)
              };

            //Initializes new instance of sendgrid client
            dynamic client = new SendGridClient(options);
            
            //Initializes new instance of emailaddress
            var from = new EmailAddress(_from);
            var to = new EmailAddress(_to);
            
            //Sends a single simple mail
            var mail = MailHelper.CreateSingleEmail(from, to, _subject, _content, _htmlcontent);

            //Ensure to deliver the mail
            mail.MailSettings = new MailSettings() { SandboxMode = new SandboxMode() };
            mail.MailSettings.SandboxMode.Enable = false; 
            
            //Sendemail async operation
            var response = await client.SendEmailAsync(mail);
            Console.Write("Send grid email response : " + response);
         }
    }
}
