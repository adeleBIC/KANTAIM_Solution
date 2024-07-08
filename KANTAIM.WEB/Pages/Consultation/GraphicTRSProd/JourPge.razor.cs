using Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using System.Linq;

namespace KANTAIM.WEB.Pages.Consultation.GraphicTRSProd
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
        [Inject] public ProductFamilyService _productfamilyService { get; set; }
        [Inject] public ProductService _productService { get; set; }
        public List<DataProd> DataProds { get; set; }
        MudDatePicker _picker;
        DateTime? date;
        private IEnumerable<ProductFamily> productfamiliesList;
        private IEnumerable<Product> productList;
        public double[][] ChartData = new double[2][];


        public int SelectedProductId { get; set; }

        private int selectedProductFamilyId;

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

        Product prodDefault = new Product()
        {
            Id = 0,
            Name = "All"
        };

        public int SelectedProductFamilyId
        {
            get { return selectedProductFamilyId; }
            set
            {
                productList = _productService.GetAllPerProductFamily(value);
                productList.Concat(new List<Product> { prodDefault });
                selectedProductFamilyId = value;
                SelectedProductId = 0;
            }
        }



        protected override async Task OnInitializedAsync()
        {
            date = DateTime.Today.AddDays(-1);
            selectedProductFamilyId = 1;
            productfamiliesList = _productfamilyService.GetAll();
            productList = _productService.GetAllPerProductFamily(selectedProductFamilyId);
            await Task.Run(RefreshData);
        }
        void RefreshData()
        {
            if (date != null)
            {
                date = OnDateChanged(date.Value);

                DateTime test = date.Value.AddDays(6);

                productList = _productService.GetAllPerProductFamily(selectedProductFamilyId);
                var productIDs = productList.Select(p => p.Id).ToList();

                DataProds = _dataProdService.GetAll().Where(u => u.DateProd >= date & u.DateProd <= test).Where(u => productIDs.Contains(u.ProductID)).ToList();


                if (SelectedProductId != 0)
                    DataProds = DataProds.Where(u => u.ProductID == SelectedProductId).ToList();

                List<string> daysOfWeek = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

                Data = new DataItem[7];
                int i = 0;
                foreach (string day in daysOfWeek)
                {
                    List<DataProd> filteredData = DataProds.Where(p => p.DateProd.DayOfWeek.ToString() == day).ToList();
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
            else
            {
                tab[0] = 0;
                tab[1] = 0;
            }

            return tab;
        }
    }
}
