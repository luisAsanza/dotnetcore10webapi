//using Autofac;
using DIExample.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceContracts;
using Services;

namespace DIExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICitiesService _citiesService;
        //To add child scope service using dotnet core IoC
        private readonly IServiceScopeFactory _serviceScopeFactory;
        //To add child scope using Autofac
        //private readonly ILifetimeScope _lifetimeScope;
        //Service IWebHostEnvironment can be used to retrieve information about current Environment
        private readonly IWebHostEnvironment _webHostEnvironment;
        //Stock service
        private readonly IFinnhubService _myServiceWithHttpClient;

        public HomeController(ICitiesService citiesService, IServiceScopeFactory serviceScopeFactory, 
            IWebHostEnvironment webHostEnvironment,
            IFinnhubService myServiceWithHttpClient)
        {
            _citiesService = citiesService;
            _serviceScopeFactory = serviceScopeFactory;
            //_lifetimeScope = lifetimeScope;
            _webHostEnvironment = webHostEnvironment;
            _myServiceWithHttpClient = myServiceWithHttpClient;
        }

        [Route("/")]
        [Route("some-route")]
        public async Task<IActionResult> Index([FromServices] IConfiguration configuration, 
            [FromServices] IOptions<WeatherApiOptions> weatherOptionsFromService)
        {
            var cities = _citiesService.GetCities();
            List<string> citiesInChildScope = new List<string>();

            //Create a child scope service using dotnet IoC
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                var citiesService = scope.ServiceProvider.GetService<ICitiesService>();
                citiesInChildScope = citiesService == null ? new List<string>() : citiesService.GetCities();
            }

            //Create a child scope service using AUTOFAC
            //using (ILifetimeScope lifetimeScope = _lifetimeScope.BeginLifetimeScope())
            //{
            //    //Inject citiesService
            //    ICitiesService citiesService = lifetimeScope.Resolve<ICitiesService>();
            //    citiesInChildScope = citiesService.GetCities();
            //}

            ViewBag.Environment = $"{_webHostEnvironment.EnvironmentName} {_webHostEnvironment.ApplicationName} {_webHostEnvironment.ContentRootPath} {_webHostEnvironment.ContentRootFileProvider}";
            ViewBag.ConfigurationExample1 = configuration.GetValue<string>("MyKey");
            ViewBag.ConfigurationExample2 = configuration.GetValue<int>("x", 100);

            //Use ":" to get nested properties
            //ViewBag.WeatherApiKey = configuration.GetValue<string>("weatherapi:ClientID");
            //ViewBag.WeatherSecret = configuration.GetValue<string>("weatherapi:clientsecret");

            //Use GetSection to read keys only once
            //var weatherConfiguratuionSection = configuration.GetSection("weatherapi");
            //ViewBag.WeatherApiKey = weatherConfiguratuionSection["ClientID"];
            //ViewBag.WeatherSecret = weatherConfiguratuionSection["ClientSecret"];

            //Use class to load options into an object. Get<type> creates a new instance. Bind uses an existing instance
            //WeatherApiOptions weatherOptions = configuration.GetSection("weatherapi").Get<WeatherApiOptions>();
            //ViewBag.WeatherApiKey = weatherOptions.ClientID;
            //ViewBag.WeatherSecret = weatherOptions.ClientSecret;

            //Weather options object injected as a service
            //ViewBag.WeatherApiKey = weatherOptionsFromService.Value.ClientID;
            //ViewBag.WeatherSecret = weatherOptionsFromService.Value.ClientSecret;

            //Read configuration from custom json file
            ViewBag.WeatherApiKey = configuration.GetValue<string>("weatherapi:ClientID");
            ViewBag.WeatherSecret = configuration.GetValue<string>("weatherapi:ClientTest");

            var msftQuote = await _myServiceWithHttpClient.GetQuote("MSFT");
            ViewBag.CurrentPrice = msftQuote["c"].ToString();

            return View(citiesInChildScope);
        }

        [Route("some-route")]
        public IActionResult Other()
        {
            return View();
        }
    }
}
