using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KANTAIM.WEB.Pages.Kanban
{
    public partial class FindProductPge
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] public ColorService _colorService { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public ColorProductService _colorProductServiceService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        
        
        [Parameter] public int Id { get; set; }
        [Parameter] public int Number { get; set; }

        private int state;
        public Product? ProductScanner { get; set; }
        public ProdColor? ColorChoose { get; set; }
        public List<ProdColor> Colors { get; set; }
        //public IEnumerable<Cell> cells { get; set; }
        public List<Cell> cells { get; set; }
        public Log logRescent { get; set; }



        protected override void OnInitialized()
        {
            ProductScanner = _productService.GetByNumber(Number);
            Colors = new List<ProdColor>();
            
            foreach (ColorProduct colorProduct in _colorProductServiceService.GetAllPerProduct(ProductScanner.Id))
            {
                Colors.Add(_colorService.GetById(colorProduct.ColorID));
            }
            if(Colors.Count() == 0)
            {
                findCells();
            }
            //base.OnInitialized();

        }

        void findCells()
        {
            cells = new List<Cell>();
            foreach (Container container in _contenaireService.Cache.Where(c => c.CellStock != null))
            {
                logRescent = _logService.GetByContenaireByActionId(container.Id,2);
                if (logRescent != null && logRescent.ProductID == ProductScanner.Id )
                {
                    if(ColorChoose == null || logRescent.ProdColorID == ColorChoose.Id)
                    {
                        if (!cells.Any(c => c.Id == container.CellStock.Id)) // Assurez-vous de comparer avec une propriété unique
                        {
                            cells.Add(container.CellStock);
                        }
                    } 
                    //cells.Append(container.CellStock);
                }
            }
        }

        void ColorSelected(int colorid)
        {
            ColorChoose = _colorService.GetById(colorid);
            findCells();
        }

        void GoBack()
        {
            NavigationManager.NavigateTo("/ScannerPge");
        }


    }
}