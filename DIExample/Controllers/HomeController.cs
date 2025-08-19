using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace DIExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICitiesService _citiesService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public HomeController(ICitiesService citiesService, IServiceScopeFactory serviceScopeFactory)
        {
            _citiesService = citiesService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [Route("/")]
        public IActionResult Index()
        {
            var cities = _citiesService.GetCities();
            List<string> citiesInChildScope = new List<string>();

            using (IServiceScope scope = _serviceScopeFactory.CreateScope()) {
                var citiesService = scope.ServiceProvider.GetService<ICitiesService>();
                citiesInChildScope = citiesService == null ? new List<string>() :  citiesService.GetCities();
            }


            return View(citiesInChildScope);
        }
    }
}
