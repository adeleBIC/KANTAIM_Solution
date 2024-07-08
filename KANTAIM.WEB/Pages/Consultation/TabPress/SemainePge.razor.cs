using global::Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Pages.Consultation.TabPress
{
    public partial class SemainePge
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

        public int selectedPressId { get; set; }

        List<string> Shifts = new List<string>() { "Matin", "Après-Midi", "Nuit", "Total" };

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
            selectedPressId = 1;
            pressesList = _pressService.GetAll();
            //await Task.Run(RefreshData);
            RefreshData();
        }

        void RefreshData()
        {
            if ((date != null) && (selectedPressId != 0))
            {

                date = OnDateChanged(date.Value);
                DateTime test = date.Value.AddDays(6);
                DataProds = _dataProdService.GetAll().Where(u => u.DateProd >= date & u.DateProd <= test).Where(u => u.PressID == selectedPressId).ToList();
                List<string> daysOfWeek = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

                Data = new DataItem[8][];
                daysOfWeekList = new List<string>();

                int i;
                for (i = 0; i < 7; i++)
                {
                    DateTime currentDate = date.Value.AddDays(i);
                    daysOfWeekList.Add(currentDate.ToString("dddd - dd/MM/yyyy"));
                }



                i = 0;
                foreach (string day in daysOfWeek)
                {
                    Data[i] = new DataItem[4]; // Initialize the inner array for each day
                    List<DataProd> filteredData = DataProds
                    .Where(p => p.DateProd.DayOfWeek.ToString() == day)
                    .ToList();

                    if (filteredData.Any())
                    {
                        int j;
                        int nbShifts = 0;
                        double objective = 0;
                        double ValueTRS = 0;
                        for (j = 1; j < 4; j++)
                        {
                            DataProd dataProd = filteredData.FirstOrDefault(l => l.NumDayShift == j);


                            // Check if dataProd is not null before accessing its properties
                            if (dataProd != null)
                            {
                                nbShifts++;
                                objective += (double)dataProd.Objective;
                                Data[i][j - 1] = new DataItem
                                {
                                    Counter = dataProd.Counter,
                                    ValueTRS = (double)dataProd.TRS,
                                    comment = dataProd.Comment,
                                    objOK = dataProd.ObjOk
                                };
                            }
                            else
                            {
                                // Handle the case where no data is available for the specific shift
                                Data[i][j - 1] = new DataItem
                                {
                                    Counter = 0,
                                    ValueTRS = 0,
                                    objOK = false,  // Assuming bool properties are initialized to 'false'
                                    comment = null
                                };
                            }
                        }
                        if (nbShifts != 0)
                        {
                            objective /= nbShifts;
                            ValueTRS = Math.Round((Data[i][0].ValueTRS + Data[i][1].ValueTRS + Data[i][2].ValueTRS) / nbShifts, 2);
                        }

                        Data[i][3] = new DataItem
                        {
                            Counter = Data[i][0].Counter + Data[i][1].Counter + Data[i][2].Counter,
                            ValueTRS = ValueTRS,  
                            objOK = ValueTRS >= objective,  // Assuming bool properties are initialized to 'false'
                            comment = null
                        };
                    }
                    else
                    {
                        // Handle the case where no data is available for the specific day
                        Data[i] = new DataItem[4];
                        for (int j = 0; j < 4; j++)
                        {
                            Data[i][j] = new DataItem
                            {
                                Counter = 0,
                                ValueTRS = 0,
                                objOK = false,  // Assuming bool properties are initialized to 'false'
                                comment = null
                            };
                        }
                    }

                    i++;
                }
                Data[7] = new DataItem[4];
                for (i = 0; i < 4; i++)
                {
                    int sum = 0;
                    double trsTotal = 0;
                    int daysWithData = 0;

                    for (int j = 0; j < 7; j++)
                    {
                        if (Data[j][i] != null && Data[j][i].Counter != 0)
                        {
                            daysWithData++;
                            sum += Data[j][i].Counter;
                            trsTotal += Data[j][i].ValueTRS;
                        }
                    }

                    if (daysWithData != 0)
                    {
                        double trs = Math.Round((double)(trsTotal / daysWithData), 2);
                        Data[7][i] = new DataItem
                        {
                            Counter = sum,
                            ValueTRS = trs,
                            comment = null,
                            objOK = (trs >= 80)
                        };
                    }
                    else
                    {
                        Data[7][i] = new DataItem
                        {
                            Counter = 0,
                            ValueTRS = 0,
                            comment = null,
                            objOK = false
                        };
                    }
                }

            }

        }





    }
}