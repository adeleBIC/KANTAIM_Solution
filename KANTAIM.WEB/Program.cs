using KANTAIM.DAL;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Services;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddAuthorizationCore();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//builder.Services.AddantDesign();
builder.Services.AddSingleton(typeof(Repository<>));
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ProductFamilyService>();
builder.Services.AddSingleton<ColorService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<WorkshopService>();
builder.Services.AddSingleton<ShapeService>();
builder.Services.AddSingleton<ShiftService>(); 
builder.Services.AddSingleton<PressService>();
builder.Services.AddSingleton<DataProdService>();
builder.Services.AddSingleton<ContenaireTypeService>();
builder.Services.AddSingleton<MachineService>();
builder.Services.AddSingleton<ActionService>();
builder.Services.AddSingleton<ContenaireService>();
builder.Services.AddSingleton<CellService>();
builder.Services.AddSingleton<LogService>();
builder.Services.AddSingleton<CellProductService>();
builder.Services.AddSingleton<ColorProductService>();
builder.Services.AddSingleton<ScanService>();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
