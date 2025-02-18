
using global::Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;

namespace KANTAIM.WEB.Pages.Consultation.GraphicTRSProd
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
        [Inject] public PressService _pressService { get; set; }
        [Inject] public ProductFamilyService _productfamilyService { get; set; }
        [Inject] public ProductService _productService { get; set; }
        public List<DataProd> DataProds { get; set; }
        MudDatePicker _picker;
        DateTime? date;
        private IEnumerable<Press> pressesList;
        private IEnumerable<ProductFamily> productfamiliesList;
        private IEnumerable<Product> productList;
        public double[][] ChartData = new double[2][];


        public int SelectedProductId { get; set; }

        private int selectedProductFamilyId;

        Product prodDefault = new Product()
        {
            Id = 0,
            Name = "Aucune"
        };

        public int SelectedProductFamilyId
        {
            get { return selectedProductFamilyId; }
            set
            {
                productList = _productService.GetAllPerProductFamily(value);
                productList = productList.Concat(new List<Product> { prodDefault }).ToList();
                selectedProductFamilyId = value;
                SelectedProductId = 0;
            }
        }


        protected override async Task OnInitializedAsync()
        {
            date = DateTime.Today.AddDays(-1);
            selectedProductFamilyId = 1;
            pressesList = _pressService.GetAll();
            productfamiliesList = _productfamilyService.GetAll();
            productList = _productService.GetAllPerProductFamily(selectedProductFamilyId);
            productList = productList.Concat(new List<Product> { prodDefault }).ToList();
            await Task.Run(RefreshData);
        }


        static int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

        public ChartOptions chartOptions = new ChartOptions();

        void RefreshData()
        {
            if ((date != null))
            {
                productList = _productService.GetAllPerProductFamily(selectedProductFamilyId);
                productList = productList.Concat(new List<Product> { prodDefault }).ToList();
                var productIDs = productList.Select(p => p.Id).ToList();

                DataProds = _dataProdService.GetAll().Where(u => u.DateProd.Year == date.Value.Year).Where(u => u.DateProd.Month == date.Value.Month).Where(u => productIDs.Contains(u.ProductID)).ToList();

                daysInMonth = DateTime.DaysInMonth(date.Value.Year, date.Value.Month);

                if (SelectedProductId != 0)
                    DataProds = DataProds.Where(u => u.ProductID == SelectedProductId).ToList();
                

                Data = new DataItem[daysInMonth];
                for (int day = 1; day <= daysInMonth; day++)
                {
                    List<DataProd> filteredData = DataProds.Where(p => p.DateProd.Day == day).ToList();
                    double[] tab = calculData(filteredData);
                    Data[day - 1] = new DataItem
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
            }
            if (nbShifts > 0)
            {
                tab[0] = Math.Round(trsCalc / nbShifts, 2); // trsCalc
                tab[1] = Math.Round(objCalc / nbShifts, 2); // obj calc
            }

            return tab;
        }
    }
}