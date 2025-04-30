using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using KANTAIM.DAL;
using KANTAIM.DAL.Services;
using KANTAIM.APK.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;


namespace KANTAIM.APK
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
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
            builder.Services.AddSingleton<JSService>();
            builder.Services.AddSingleton<ProfilService>();
            builder.Services.AddSingleton<RackService>();
            builder.Services.AddSingleton<RackProfilService>();
            builder.Services.AddSingleton<ProfilSessionService>();

            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
            });
            return builder.Build();
        }
    }
}
