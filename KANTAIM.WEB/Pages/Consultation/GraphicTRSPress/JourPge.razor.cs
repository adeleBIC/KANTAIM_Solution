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
using KANTAIM.WEB.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.IdentityModel.Tokens;
using Azure;
using System.Globalization;

namespace KANTAIM.WEB.Pages.Consultation.GraphicTRSPress
{
    public partial class JourPge
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
        [Inject] public PressService _pressService { get; set; }
        public List<DataProd> DataProds { get; set; }
        MudDatePicker _picker;
        DateTime? date;
        private IEnumerable<Press> pressesList;
        public double[][] ChartData = new double[2][];

        public int selectedPressId { get; set; }

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
            await Task.Run(RefreshData);
        }



        void RefreshData()
        {
            if ((date != null) && (selectedPressId != 0)) {
                
                date = OnDateChanged(date.Value);
                DateTime test = date.Value.AddDays(6);
                DataProds = _dataProdService.GetAll().Where(u => u.DateProd >= date & u.DateProd <= test).Where(u => u.PressID == selectedPressId).ToList();
                List<string> daysOfWeek = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

                Data = new DataItem[7];
                int i = 0;
                foreach (string day in daysOfWeek)
                {
                    List <DataProd> filteredData = DataProds.Where(p => p.DateProd.DayOfWeek.ToString() == day).ToList();
                    double[] tab = calculData(filteredData);
                    Data[i] = new DataItem
                    {
                        Date = day,
                        ValueTRS = tab[0],
                        ValueOBJ = tab[1]
                    };

                    i++;
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

            return tab;
        }

      

    }
}