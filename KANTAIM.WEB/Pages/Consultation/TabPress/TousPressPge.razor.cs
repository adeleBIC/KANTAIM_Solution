using global::Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using System;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace KANTAIM.WEB.Pages.Consultation.TabPress
{

    public partial class TousPressPge
    {
        class DataItem
        {
            public int Counter { get; set; }
            public double ValueTRS { get; set; }
            public bool objOK { get; set; }
            public string comment { get; set; }
        }

        DataItem[][] Data;


        bool open = false;
        void ToggleDrawer()
        {
            open = !open;
        }
        [Inject] public DataProdService _dataProdService { get; set; }
        [Inject] public PressService _pressService { get; set; }
        public List<DataProd> DataProds { get; set; }
        MudDatePicker _picker;
        DateTime? date;
        private IEnumerable<Press> pressesList;
        List<string> daysOfWeekList;
        public double[][] ChartData = new double[2][];
        List<string> pressNumber = new List<string>();
        int nombrePress;
        Press p;
        int Length = 0;


        private DateTime OnDateChanged(DateTime selectedDate)
        {
            if (selectedDate.DayOfWeek != DayOfWeek.Monday)
            {
                // Reset the selected date to the nearest Monday
                DateTime nearestMonday = selectedDate.AddDays(DayOfWeek.Monday - selectedDate.DayOfWeek);
                return nearestMonday;
            }
            else
            {
                return selectedDate;
            }

        }

        protected override async Task OnInitializedAsync()
        {
            date = DateTime.Today.AddDays(-1);
            int i;
            pressesList = _pressService.GetAll();
            nombrePress = pressesList.Count();
            Data = new DataItem[nombrePress+1][];
            daysOfWeekList = new List<string>();
            for (i = 0; i < 7; i++)
            {
                DateTime currentDate = date.Value.AddDays(i);
                daysOfWeekList.Add(currentDate.ToString("dddd - MM/dd/yyyy"));
            }
            //await Task.Run(RefreshData);
            RefreshData();
        }


        void RefreshData()
        {
            if ((date != null) )
            {
                date = OnDateChanged(date.Value);
                pressNumber = new List<string>();
                DateTime test = date.Value.AddDays(6);

                // Get all DataProds for the target date and join them with Presses
                var dataProdsWithPresses = _dataProdService.GetAll()
                    .Where(u => u.DateProd >= date & u.DateProd <= test)
                    .Join(pressesList,
                          dataProd => dataProd.PressID,
                          press => press.Id,
                          (dataProd, press) => new { DataProd = dataProd, Press = press })
                    .OrderBy(p => p.Press.Number)
                    .ToList();


                daysOfWeekList = new List<string>();
                int i;
                for (i = 0; i < 7; i++)
                {
                    DateTime currentDate = date.Value.AddDays(i);
                    daysOfWeekList.Add(currentDate.ToString("dddd - dd/MM/yyyy"));
                }

                int j;
                int nom = 0;
                Length = 0;
                Data = new DataItem[nombrePress][];
                foreach (var prod in dataProdsWithPresses)
                {
                    if (nom != prod.Press.Number)
                    {
                        pressNumber.Add(prod.Press.Number.ToString());
                        nom = prod.Press.Number;
                        Length++;
                    }
                }
                pressNumber.Add("Total");
                List<string> daysOfWeek = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                i = 0;
                foreach (string number in pressNumber)
                {
                    var filteredPressData = dataProdsWithPresses.FindAll(u => u.Press.Number.ToString() == number);
                    Data[i] = new DataItem[8];
                    j = 0;
                    int totalCounter = 0;
                    int nomTotalTrs = 0;
                    double AvgTotalTrs = 0;

                    foreach (string day in daysOfWeek)
                    {
                        var filteredDayData = filteredPressData.Where(p => p.DataProd.DateProd.DayOfWeek.ToString() == day).OrderBy(p => p.DataProd.NumDayShift).ToList();
                        
                        int nbShifts = filteredDayData.Count();
                        
                        if(nbShifts > 0)
                        {
                            nomTotalTrs++;

                            double trsCalc = (double)filteredDayData.Sum(l => l.DataProd.TRS);
                            int Counter = filteredDayData.Sum(l => l.DataProd.Counter);
                            double AvgTrs = Math.Round(trsCalc / nbShifts, 2);


                            StringBuilder combinedComment = new StringBuilder();
                            foreach (var prod in filteredDayData)
                            {
                                if (!string.IsNullOrEmpty(prod.DataProd.Comment))
                                {
                                    switch (prod.DataProd.NumDayShift)
                                    {
                                        
                                        case 1: combinedComment.Append("Matin : ").Append(prod.DataProd.Comment); break;
                                        case 2: combinedComment.Append("\nAprès-midi : ").Append(prod.DataProd.Comment);  break;
                                        case 3: combinedComment.Append("\nSoir : ").Append(prod.DataProd.Comment); break;
                                    }
                                }
                            }

                            string commentValue = combinedComment.Length > 0 ? combinedComment.ToString().Trim() : null;
                            Data[i][j] = new DataItem
                            {
                                Counter = Counter,
                                ValueTRS = AvgTrs,
                                comment = commentValue,
                                objOK = AvgTrs >= 80
                            };

                            totalCounter += Counter;
                            AvgTotalTrs += AvgTrs;
                            
                        }
                        
                        j++;
                    }

                    if(nomTotalTrs > 0)
                    {
                        AvgTotalTrs /= nomTotalTrs;
                    Data[i][7] = new DataItem
                    {
                        Counter = totalCounter,
                        ValueTRS = Math.Round(AvgTotalTrs, 2),
                        comment = null,
                        objOK = AvgTotalTrs >= 80
                    };
                    }
                    
                    i++;

                }
                if(Length > 1)
                {
                    TotalCalcul(Data, Length, 8);
                }
                
            }

        }

        void TotalCalcul(DataItem[][] data, int ligne, int colonne)
        {
            int Counter = 0;
            int n = 0;
            double AvgTrs = 0;
            Data[ligne] = new DataItem[8];
            for (int i = 0; i < colonne; i++)
            {
                Counter = 0;
                n = 0;
                AvgTrs = 0;
                for(int j = 0; j < ligne; j++)
                {
                    if(data[j][i] != null)
                    {
                        Counter += data[j][i].Counter;
                        n++;
                        AvgTrs += data[j][i].ValueTRS;
                    }

                }
                if(n != 0)
                {
                    AvgTrs /= n;

                    data[ligne][i] = new DataItem
                    {
                        Counter = Counter,
                        ValueTRS = Math.Round(AvgTrs, 2),
                        comment = null,
                        objOK = AvgTrs >= 80
                    };
                }
                
            }
        }


    }


}
