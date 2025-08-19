using Autofac;
using Autofac.Extensions.DependencyInjection;
using ServiceContracts;
using Services;

var builder = WebApplication.CreateBuilder(args);

//USE SERVICE PROVIDER FACTORY TO USE AUTOFAC IoC CONTAINER
builder .Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());


builder.Services.AddControllersWithViews();


//INJECT DEPENDENCIES USING DOTNETCORE IoC CONTAINER
//builder.Services.Add(new ServiceDescriptor(typeof(ICitiesService), typeof(CitiesService), ServiceLifetime.Scoped));
//Short hand method version:
//builder.Services.AddScoped<ICitiesService, CitiesService>();

//INJECT DEPENDENCIES USING AUTOFAC
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    //Add Transient
    //containerBuilder.RegisterType<CitiesService>().As<ICitiesService>().InstancePerDependency();

    //Add Scoped
    containerBuilder.RegisterType<CitiesService>().As<ICitiesService>().InstancePerLifetimeScope();

    //Add Singleton
    //containerBuilder.RegisterType<CitiesService>().As<ICitiesService>().SingleInstance();
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
