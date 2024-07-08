

using global::Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace KANTAIM.WEB.Pages.Consultation.TabPress
{

    public partial class JourPge
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

        List<string> Shifts = new List<string>() { "Matin", "Après-Midi", "Nuit", "Total" };
        List<string> pressNumber = new List<string>(); 
        int nombrePress;

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
            pressesList = _pressService.GetAll().OrderBy(p => p.Number).ToList();
            nombrePress = pressesList.Count();
            Data = new DataItem[nombrePress][];
            //await Task.Run(RefreshData);
            RefreshData();
        }

        void RefreshData()
        {
            if ((date != null))
            {
                //date = OnDateChanged(date.Value);
                pressNumber = new List<string>();
                Data = new DataItem[nombrePress][];
                var pressesList = _pressService.GetAll().OrderBy(p => p.Number).ToList();

                // Get all DataProds for the target date and join them with Presses
                var dataProdsWithPresses = _dataProdService.GetAll()
                    .Where(u => u.DateProd == date)
                    .Join(pressesList,
                          dataProd => dataProd.PressID,
                          press => press.Id,
                          (dataProd, press) => new { DataProd = dataProd, Press = press })
                    .OrderBy(p => p.Press.Number)
                    .ToList();
                
                int Length = 0, j;
                int nom=0;

                foreach (var prod in dataProdsWithPresses)
                {
                    if (Data[Length] == null)
                    {   
                        nom = prod.Press.Number;
                        pressNumber.Add(nom.ToString());
                        Data[Length] = new DataItem[4];
                        Data[Length][prod.DataProd.NumDayShift-1] = new DataItem
                        {
                            Counter = prod.DataProd.Counter,
                            ValueTRS = (double)prod.DataProd.TRS,
                            comment = prod.DataProd.Comment,
                            objOK = prod.DataProd.ObjOk
                        };
                    } else
                    {
                       if(nom != prod.Press.Number)
                        {
                            Length++;
                            nom = prod.Press.Number;
                            pressNumber.Add(nom.ToString());
                            Data[Length] = new DataItem[4];
                            Data[Length][prod.DataProd.NumDayShift -1] = new DataItem
                            {
                                Counter = prod.DataProd.Counter,
                                ValueTRS = (double)prod.DataProd.TRS,
                                comment = prod.DataProd.Comment,
                                objOK = prod.DataProd.ObjOk
                            };
                        } else {
                            Data[Length][prod.DataProd.NumDayShift - 1] = new DataItem
                            {
                                Counter = prod.DataProd.Counter,
                                ValueTRS = (double)prod.DataProd.TRS,
                                comment = prod.DataProd.Comment,
                                objOK = prod.DataProd.ObjOk
                            };
                        }
                        
                    }
                }
                pressNumber.Add("Total");
                Length++;

                for (int i = 0; i < Length; i++)
                {
                    double sumTRS = 0;
                    int countTRS = 0;
                    int sumCounter = 0;
                    int countCounter = 0;
                    StringBuilder combinedComment = new StringBuilder();

                    // Iterate through the shifts (0, 1, 2) to calculate sum and count of non-null values
                    for ( j = 0; j < 3; j++)
                    {
                        if (Data[i] != null && Data[i][j] != null)
                        {
                            sumTRS += Data[i][j].ValueTRS;
                            countTRS++;
                            sumCounter += Data[i][j].Counter;
                            countCounter++;
                        }
                    }

                    // Calculate average TRS and Counter values for the 3 shifts
                    double avgTRS = countTRS > 0 ? Math.Round(sumTRS / countTRS, 2) : 0;
                  

                    // Assign the average values to Data[i][3]
                    if (Data[i] != null)
                    {
                        Data[i][3] = new DataItem
                        {
                            ValueTRS = avgTRS,
                            Counter = sumCounter,
                            comment = null, // You can assign comment as per your requirement
                            objOK = avgTRS>=80   // You can assign objOK as per your requirement
                        };
                    }

                    
                }
                if (Length > 1)
                {
                    TotalCalcul(Data, Length, 4);
                }
            }

        }

        void TotalCalcul(DataItem[][] data, int ligne, int colonne)
        {
            int Counter = 0;
            int n = 0;
            double AvgTrs = 0;
            Data[ligne] = new DataItem[colonne];
            for (int i = 0; i < colonne; i++)
            {
                Counter = 0;
                n = 0;
                AvgTrs = 0;
                for (int j = 0; j < ligne - 1; j++)
                {
                    if (data[j][i] != null)
                    {
                        Counter += data[j][i].Counter;
                        n++;
                        AvgTrs += data[j][i].ValueTRS;
                    }

                }
                if (n != 0)
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
