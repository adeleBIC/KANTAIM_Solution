using Kantaim.OPC.Services;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using Opc.Ua;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime;

namespace Kantaim.SRVC
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly OpcUaService _opcUaService;
        private readonly CurrentPressCounterService _currentPressCounterService;
        private readonly DataProdService _dataProdService;
        private readonly ShiftService _shiftService;
        private readonly DevModeService _devModeService;

        public Worker(ILogger<Worker> logger, OpcUaService opcUaService, CurrentPressCounterService currentPressCounterService, DataProdService dataProdService, ShiftService shiftService, DevModeService devModeService)
        {
            _logger = logger;
            _opcUaService = opcUaService;
            _currentPressCounterService = currentPressCounterService;
            _dataProdService = dataProdService;
            _shiftService = shiftService;
            _devModeService = devModeService;
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
                    _devModeService.DevMode = false;
                    //Assignation des fins de shifts
                    DateTime now = DateTime.Now;
                    //now = new DateTime(2025, 11, 20, 14, 30, 30);

                    _shiftService.ResetCache();

                    TimeSpan?[] schedule = _shiftService.GetShiftsForDay(now.DayOfWeek, true);

                    if (schedule.Length > 2 && schedule.Any(s => s.HasValue &&
                            now.Hour == s.Value.Hours && now.Minute == s.Value.Minutes))
                    {
                        //Upload des données
                        _logger.LogInformation($"🤖 Import started");
                        await UpdatePressCounter(now);
                        _logger.LogInformation($"✅ Import finished");
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

        private class PressCounter
        {
            public int PressId { get; set; }
            public Press Press { get; set; }
            public int ConsignNumber { get; set; }
            public long Value { get; set; }
        }

        private async Task UpdatePressCounter(DateTime dateprod)
        {
            //IConfigurationRoot config = new ConfigurationBuilder()
            //.AddJsonFile("appsettings.json")
            //.Build();

            //var opcSettings = config.GetSection("OpcSettingsWorkShop").GetChildren();
            //string[] endpointUrls = opcSettings.Select(x => x.Value).ToArray();


            List<PressCounter> listCounter = new List<PressCounter>();
            try
            {
                foreach (Workshop ws in _currentPressCounterService.GetAllWorkshop())
                {
                    string endPointUrl = $"opc.tcp://{ws.IPAdressConsign}:51210";
                    if (await _opcUaService.ConnectAsync(endPointUrl))
                    {
                        List<Press> presses = _currentPressCounterService.GetAllPressPerWorkshopInclude(ws.Id).Where(p => p.Active).ToList();

                        foreach (Press p in presses)
                        {
                            int value = 0;
                            switch (ws.Generation)
                            {
                                case 1:
                                    value = int.TryParse(await _opcUaService.ReadNodeValueAsync($"ns=2;s=Presse_{p.ConsignNumber}:Cycle_Count_Good"), out value) ? value : 0;
                                    break;
                                case 2:
                                    value = int.TryParse(await _opcUaService.ReadNodeValueAsync($"ns=2;s=TCP.Presse_{p.ConsignNumber}.Presse_{p.ConsignNumber}:Cycle_Count_Good"), out value) ? value : 0;
                                    break;
                                default:
                                    break;
                            }

                            listCounter.Add(new PressCounter
                            {
                                PressId = p.Id,
                                Press = p,
                                ConsignNumber = p.ConsignNumber,
                                Value = value
                            });
                        }
                    }
                    _opcUaService.Disconnect();
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur OPC : {ex.Message}");
                return;
            }

            try
            {
                int dayShift = _shiftService.GetDayShift(dateprod, true);
                int prevDayShift = _shiftService.GetPreviousDayShift(dateprod, true);
                int prevWeekShift = _shiftService.GetWeekShift(dateprod, prevDayShift, true);
                DateTime? datePrevProd = _shiftService.GetPreviousShiftDate(dateprod, true) ?? dateprod;

                foreach (PressCounter pc in listCounter)
                {
                    
                    CurrentPressCounter counter = _currentPressCounterService.GetByPressId(pc.PressId);
                    if (counter != null)
                    {
                        counter.PreviousCounter = counter.CurrentCounter;
                        counter.CurrentCounter = pc.Value;
                        if (dayShift == 1) counter.StartJourneyCounter = pc.Value;
                    }
                    else counter = new CurrentPressCounter() { PressID = pc.PressId, CurrentCounter = pc.Value, PreviousCounter = 0, StartJourneyCounter = 0 };

                    _currentPressCounterService.UpSert(counter);

                    int realCounter = (int)((counter.CurrentCounter - counter.PreviousCounter) * pc.Press.Shape.UsedMark);
                    decimal realTRS = Math.Round((realCounter / (pc.Press.Shape.TotalMark * ((pc.Press.Shape.OpenTime * 3600) * (decimal)(1 / pc.Press.Shape.Cycle))) * 100), 2);

                    if (realTRS < 120 && realCounter > 1000)
                    {
                        DataProd data = new DataProd()
                        {
                            NumDayShift = prevDayShift,
                            NumWeekShift = prevWeekShift,
                            Counter = realCounter,
                            TRS = realTRS,
                            OpenTime = pc.Press.Shape.OpenTime,
                            Objective = pc.Press.Shape.Objective,
                            ObjOk = realTRS > pc.Press.Shape.Objective,
                            DateProd = datePrevProd.Value,
                            DateExtract = dateprod,
                            PressID = pc.PressId,
                            ProductID = pc.Press.Shape.ProductID
                        };

                        _dataProdService.UpSert(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur Sauvegarde Données: {ex.Message}");
                return;
            }
        }

    }
}
