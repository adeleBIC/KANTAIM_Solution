using Kantaim.OPC.Services;
using Kantaim.SRVC;
using KANTAIM.DAL.Services;
using KANTAIM.DAL;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(typeof(Repository<>));
        services.AddSingleton<CurrentPressCounterService>();
        services.AddSingleton<DataProdService>();
        services.AddSingleton<WorkshopService>();
        services.AddSingleton<PressService>();
        services.AddSingleton<ShapeService>();
        services.AddSingleton<ProductService>();
        services.AddSingleton<ProductFamilyService>();
        services.AddSingleton<ShiftService>();
        services.AddSingleton<DevModeService>();

        services.AddTransient<OpcUaService>();
        services.AddHostedService<Worker>();
    })
    .UseWindowsService()
    .Build();

host.Run();
