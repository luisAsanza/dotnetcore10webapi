using ServiceContracts;
using Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
//builder.Services.Add(new ServiceDescriptor(typeof(ICitiesService), typeof(CitiesService), ServiceLifetime.Scoped));
//Short hand method version:
builder.Services.AddScoped<ICitiesService, CitiesService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
