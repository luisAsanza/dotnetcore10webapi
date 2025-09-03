//using Autofac;
using DIExample.Configuration;
using DIExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceContracts;
using Services;
using Services.Providers.Finnhub;

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
        private readonly IOptions<WeatherApiOptions> _weatherApiOptions;
        //IOptionsMonitor gets refreshed everytime configuration changes.
        private readonly IOptionsMonitor<FinnhubOptions> _finnhubOptions;
        private readonly IConfiguration _configuration;
        //For demo purposes
        private const string _stockSymbol = "MSFT";

        public HomeController(ICitiesService citiesService, IServiceScopeFactory serviceScopeFactory, 
            IWebHostEnvironment webHostEnvironment,
            IFinnhubService myServiceWithHttpClient,
            IOptions<WeatherApiOptions> weatherApiOptions,
            IOptionsMonitor<FinnhubOptions> finnhubOptions,
            IConfiguration configuration)
        {
            _citiesService = citiesService;
            _serviceScopeFactory = serviceScopeFactory;
            //_lifetimeScope = lifetimeScope;
            _webHostEnvironment = webHostEnvironment;
            _myServiceWithHttpClient = myServiceWithHttpClient;
            _weatherApiOptions = weatherApiOptions;
            _finnhubOptions = finnhubOptions;
            _configuration = configuration;
        }

        [Route("/")]
        [Route("some-route")]
        public async Task<IActionResult> Index([FromServices] IConfiguration configuration)
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

            var msftQuote = await _myServiceWithHttpClient.GetQuote(_stockSymbol);
            ViewBag.StockSymbol = _stockSymbol;
            ViewBag.StockPrice = msftQuote["c"].ToString();

            return View(citiesInChildScope);
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Route("stock-price")]
        public async Task<IActionResult> GetStockPrice(string stockSymbol)
        {
            var msftQuote = await _myServiceWithHttpClient.GetQuote(stockSymbol);

            var model = new StockPriceModel()
            {
                StockSymbol = stockSymbol,
                StockPrice = msftQuote["c"].ToString(),
                SuggestedRefreshMs = _finnhubOptions.CurrentValue.SuggestedRefreshMs,
                AsOf = DateTime.UtcNow
            };

            return Json(model);
        }

        /// <summary>
        /// Test method to throw an error and see Error page when working on Development Environment
        /// </summary>
        /// <returns></returns>
        [Route("some-route")]
        public IActionResult Other()
        {
            return View();
        }
    }
}
