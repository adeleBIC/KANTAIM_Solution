using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Ressources;
using KANTAIM.WEB.Services;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using static System.Collections.Specialized.BitVector32;


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
        [Inject] public CellService _cellService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        [Inject] public ScanService _scanService { get; set; }


        [Parameter] public int Id { get; set; }
        [Parameter] public int Number { get; set; }

        private int state;
        public Product? ProductScanner { get; set; }
        public ProdColor? ColorChoose { get; set; }
        public List<ProdColor> Colors { get; set; }
        //public IEnumerable<Cell> cells { get; set; }
        public List<Cell> cells { get; set; }
        public Log logRescent { get; set; }
        public Cell? CellScanner { get; set; }
        public string CellValue { get; set; }




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

        void cellScan(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                string[] parts = _scanService.scanCode(CellValue);
                if (parts != null)
                {
                    int.TryParse(parts[0], out int type);
                    
                    if (type == 4)
                    {
                        string X = parts[1];
                        string Y = parts[2];
                        if (int.TryParse(X, out int x) && int.TryParse(Y, out int y))
                        {
                            CellScanner = _cellService.GetByXY(x, y);
                            NavigationManager.NavigateTo($"/StockagePge/4/{CellScanner.Id}");
                        }
                    }
                    else
                    {
                        _snackService.Add("Scp scannez une cellule !", Severity.Error);
                    }
                }

            }
        }
        void GoBack()
        {
            NavigationManager.NavigateTo("/ScannerPge");
        }


    }
}