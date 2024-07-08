
using global::Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KANTAIM.WEB.Pages.Consultation.GraphicTRSPress
{
    public partial class MoisPge
    {
        class DataItem
        {
            public string Date { get; set; }
            public double ValueTRS { get; set; }
            public double ValueOBJ { get; set; }
        }
        DataItem[] Data = new DataItem[] {
        new DataItem
        {
            Date = "",
            ValueTRS = 0,
            ValueOBJ = 0
        }
        };

        bool open = false;
        void ToggleDrawer()
        {
            open = !open;
        }
        [Inject] public DataProdService _dataProdService { get; set; }
        [Inject] public ShiftService _shiftService { get; set; }
        [Inject] public PressService _pressService { get; set; }
        public List<DataProd>? DataProds { get; set; }
        public DataProd dataprod { get; set; }
        MudDatePicker _picker;
        DateTime? date;
        private IEnumerable<Press> pressesList;
        public double[][] ChartData = new double[2][];


        public int selectedPressId { get; set; }
        protected override async Task OnInitializedAsync()
        {
            date = DateTime.Today.AddDays(-1);
            selectedPressId = 1;
            pressesList = _pressService.GetAll();
            await Task.Run(RefreshData);
        }


        static int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

        

        public ChartOptions chartOptions = new ChartOptions();

        void RefreshData()
        {
            if ((date != null) && (selectedPressId != 0))
            {

                daysInMonth = DateTime.DaysInMonth(date.Value.Year, date.Value.Month);

                DataProds = _dataProdService
                    .GetAll()
                    .Where(u => u.PressID == selectedPressId)
                    .Where(u => u.DateProd.Year == date.Value.Year)
                    .Where(u => u.DateProd.Month == date.Value.Month)
                    .ToList();

                Data = new DataItem[daysInMonth];

                for (int day = 1; day <= daysInMonth; day++)
                {
                    List<DataProd> filteredData = DataProds.Where(p => p.DateProd.Day == day).ToList();
                    double[] tab = calculData(filteredData);
                    Data[day-1] = new DataItem
                    {
                        Date = day.ToString(),
                        ValueTRS = tab[0],
                        ValueOBJ = tab[1]
                    };
                }

            }

        }
        private double[] calculData(List<DataProd> list)
        {
            double[] tab = new double[2];
            //Récupération du nombre de shift
            int nbShifts = list.Count();
            double trsCalc = 0;
            double objCalc = 0;

            if (nbShifts > 0) // Si au moins 1 Shift
            {
                trsCalc = (double)list.Sum(l => l.TRS);
                objCalc = (double)list.Sum(l => l.Objective);

                tab[0] = Math.Round(trsCalc / nbShifts, 2); // trsCalc
                tab[1] = Math.Round(objCalc / nbShifts, 2); // obj calc
            }
            //Calcul pour affichage
            

            return tab;
        }

    }
}