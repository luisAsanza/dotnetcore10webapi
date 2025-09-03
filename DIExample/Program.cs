using Autofac;
using Autofac.Extensions.DependencyInjection;
using DIExample.Configuration;
using ServiceContracts;
using Services;
using Services.Handlers;
using Services.Providers.Finnhub;

var builder = WebApplication.CreateBuilder(args);

//USE SERVICE PROVIDER FACTORY TO USE AUTOFAC IoC CONTAINER
//builder .Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());


builder.Services.AddControllersWithViews();
builder.Services.AddTransient<LoggingHandler>();
builder.Services.AddTransient<SimpleRetryHandler>();
builder.Services.AddHttpClient("FinnhubClient").AddHttpMessageHandler<LoggingHandler>().AddHttpMessageHandler<SimpleRetryHandler>();

//Register a service to supply an object of type WeatherApiOptions with weatherapi configurations loaded.
builder.Services.Configure<WeatherApiOptions>(builder.Configuration.GetSection("weatherapi"));

builder.Services.AddOptions<FinnhubOptions>()
    .Bind(builder.Configuration.GetSection("finnhubapi"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

//Load configuration from a custom json file
builder.Configuration.AddJsonFile("MyOwnConfig.json", optional: true, reloadOnChange: true);

//INJECT DEPENDENCIES USING DOTNETCORE IoC CONTAINER
builder.Services.Add(new ServiceDescriptor(typeof(ICitiesService), typeof(CitiesService), ServiceLifetime.Scoped));
//Short hand method version:
builder.Services.AddScoped<ICitiesService, CitiesService>();

//Register Stock service
builder.Services.AddScoped<IFinnhubService, FinnhubService>();

////INJECT DEPENDENCIES USING AUTOFAC
//builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
//{
//    //Add Transient
//    //containerBuilder.RegisterType<CitiesService>().As<ICitiesService>().InstancePerDependency();

//    //Add Scoped
//    containerBuilder.RegisterType<CitiesService>().As<ICitiesService>().InstancePerLifetimeScope();

//    //Add Singleton
//    //containerBuilder.RegisterType<CitiesService>().As<ICitiesService>().SingleInstance();
//});

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}

app.UseStaticFiles();
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.Map("config-example", async context =>
    {
        //GetValue method returns an object. You can use GetValue<string> to return an string directly.
        await context.Response.WriteAsync(app.Configuration.GetValue<string>("myKey") + "\n");
        await context.Response.WriteAsync(app.Configuration["myKey"] + "\n");
        //Get value from configuration, if it does not exist then return default value
        await context.Response.WriteAsync(app.Configuration.GetValue<int>("x", 10).ToString());
    });
});


app.MapControllers();

app.Run();
