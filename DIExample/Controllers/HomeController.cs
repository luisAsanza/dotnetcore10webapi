using Autofac;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace DIExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICitiesService _citiesService;
        //To add child scope service using dotnet core IoC
        private readonly IServiceScopeFactory _serviceScopeFactory;
        //To add child scope using Autofac
        private readonly ILifetimeScope _lifetimeScope;
        //
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ICitiesService citiesService, IServiceScopeFactory serviceScopeFactory, 
            ILifetimeScope lifetimeScope, IWebHostEnvironment webHostEnvironment)
        {
            _citiesService = citiesService;
            _serviceScopeFactory = serviceScopeFactory;
            _lifetimeScope = lifetimeScope;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("/")]
        [Route("some-route")]
        public IActionResult Index()
        {
            var cities = _citiesService.GetCities();
            List<string> citiesInChildScope = new List<string>();

            //Create a child scope service using dotnet IoC
            //using (IServiceScope scope = _serviceScopeFactory.CreateScope()) {
            //    var citiesService = scope.ServiceProvider.GetService<ICitiesService>();
            //    citiesInChildScope = citiesService == null ? new List<string>() :  citiesService.GetCities();
            //}

            //Create a child scope service using AUTOFAC
            using (ILifetimeScope lifetimeScope = _lifetimeScope.BeginLifetimeScope())
            {
                //Inject citiesService
                ICitiesService citiesService = lifetimeScope.Resolve<ICitiesService>();
                citiesInChildScope = citiesService.GetCities();
            }

            ViewBag.Environment = $"{_webHostEnvironment.EnvironmentName} {_webHostEnvironment.ApplicationName} {_webHostEnvironment.ContentRootPath} {_webHostEnvironment.ContentRootFileProvider}";


            return View(citiesInChildScope);
        }

        [Route("some-route")]
        public IActionResult Other()
        {
            return View();
        }
    }
}
