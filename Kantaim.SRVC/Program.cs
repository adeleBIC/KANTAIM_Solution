using Kantaim.OPC.Services;
using Kantaim.SRVC;
using KANTAIM.DAL.Services;
using KANTAIM.DAL;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Interface;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(typeof(Repository<>));
        services.AddSingleton<CurrentPressCounterService>();
        services.AddSingleton<ShiftService>();

        services.AddTransient<OpcUaService>();
        services.AddHostedService<Worker>();
    })
    .UseWindowsService()
    .Build();

host.Run();
