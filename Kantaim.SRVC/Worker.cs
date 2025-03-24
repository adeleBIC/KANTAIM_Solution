using Kantaim.OPC.Services;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;

namespace Kantaim.SRVC
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly OpcUaService _opcUaService;
        private readonly CurrentPressCounterService _currentPressCounterService;
        private readonly ShiftService _shiftService;

        public Worker(ILogger<Worker> logger, OpcUaService opcUaService, CurrentPressCounterService currentPressCounterService, ShiftService shiftService)
        {
            _logger = logger;
            _opcUaService = opcUaService;
            _currentPressCounterService = currentPressCounterService;
            _shiftService = shiftService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }


                try
                {

                    
                    //Assignation des fins de shifts +10 minutes
                    DateTime now = DateTime.Now;
                    int dayShift = _shiftService.GetDayShift(now,false);
                    DateTime schedule = new DateTime();// = DateTime.Today.Add(configManager.GetCopySettingAsTimeSpan("timeToCopy"));

                    //Si fin de shifts
                    if (now.Hour == schedule.Hour && now.Minute == schedule.Minute)
                    {

                        //Upload des données
                        _logger.LogInformation($"Copy started");
                        //await CopyFileAsync(sourcePath, destinationPath, configManager.GetCopySettingAsBool("overwrite"));
                        _logger.LogInformation($"Copy finished");
                        await Task.Delay(700000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {

                    _logger.LogInformation(ex.Message);
                }



                await Task.Delay(20000, stoppingToken);
            }
        }
    }
}
