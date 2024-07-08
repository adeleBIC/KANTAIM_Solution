using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Threading.Tasks;
using global::Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using KANTAIM.WEB;
using KANTAIM.WEB.Shared;
using MudBlazor;
using KANTAIM.DAL;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using System.Globalization;

namespace KANTAIM.WEB.Pages.Consultation.GraphicTRSPress
{
    public partial class AnneePge
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
        public List<DataProd> DataProds { get; set; }
        public DataProd dataprod { get; set; }
        MudDatePicker _picker;
        int selectedYear = 0;
        private IEnumerable<Press> pressesList;

        public int selectedPressId { get; set; }


        protected override async Task OnInitializedAsync()
        {
            selectedYear = DateTime.Now.Year;
            selectedPressId = 1;
            pressesList = _pressService.GetAll();
            await Task.Run(RefreshData);
        }

        void RefreshData()
        {
            if ((selectedYear != 0) && (selectedPressId != 0))
            {
                DataProds = _dataProdService
                    .GetAll()
                    .Where(u => u.DateProd.Year == selectedYear)
                    .Where(u => u.PressID == selectedPressId)
                    .ToList();
               
                List<int> MonthOfYear = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                Data = new DataItem[12];
                foreach (int Month in MonthOfYear)
                {
                    List<DataProd> filteredData = DataProds.Where(p => p.DateProd.Month == Month).ToList();
                    double[] tab = calculData(filteredData);
                    Data[Month - 1] = new DataItem
                    {
                        Date = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Month),
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
            }
            //Calcul pour affichage
            if (nbShifts > 0)
            {
                tab[0] = Math.Round(trsCalc / nbShifts, 2); // trsCalc
                tab[1] = Math.Round(objCalc / nbShifts, 2); // obj calc
            }

            return tab;
        }

    }
}