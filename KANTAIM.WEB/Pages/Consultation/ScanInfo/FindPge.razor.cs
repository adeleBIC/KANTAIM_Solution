using Azure;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Ressources;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using static KANTAIM.WEB.Pages.Kanban.FindProductPge;
using static MudBlazor.Colors;

namespace KANTAIM.WEB.Pages.Consultation.ScanInfo
{
    public partial class FindPge
    {
        [Inject] public ProductFamilyService _productfamilyService { get; set; }
        private List<ProductFamily> _productfamilyList { get; set; }
        [Inject] public ProductService _productService { get; set; }
        private List<Product> _productList { get; set; }
        [Inject] public LogService _logService { get; set; }
        private List<Log> _logList { get; set; }
        [Inject] public ColorProductService _colorProductService { get; set; }
        private List<ColorProduct> _colorProductList { get; set; }
        [Inject] public CellProductService _cellProductService { get; set; }
        private List<CellProduct> _cellProductList { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        private List<Container> _contenaireList { get; set; }
        [Inject] public ColorService _colorService { get; set; }
        private List<ProdColor> _prodcolorList { get; set; }
        [Inject] public CellService _cellService { get; set; }
        private List<Cell> _cellList { get; set; }
        private List<Cell> refreshCellList { get; set; }
        [Inject] public WorkshopService _workshopService { get; set; }
        private List<Workshop> _workshopList { get; set; }

        private List<CellLog> cells { get; set; }

        private string _searchString;
        private IEnumerable<ProductFamily> productfamiliesList;
        private IEnumerable<Product> productList;
        private IEnumerable<ProdColor> colorList;
        private IEnumerable<Workshop> workshopList;
        private int selectedWorkshopId;

        public int SelectedWorkshopId
        {
            get { return selectedWorkshopId; }
            set
            {
                selectedWorkshopId = value;
                ShowInfo();
            }
        }

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
                selectedProductFamilyId = value;
                productList = productList.Concat(new List<Product> { prodDefault }).ToList();
                SelectedProductId = 0;
                ShowInfo();
            }
        }

        private Product? SelectedProduct { get; set; }
        private Log logRescent { get; set; }

        private int selectedProductId;
        public int SelectedProductId
        {
            get { return selectedProductId; }
            set
            {
                selectedProductId = value;
                SelectedProduct = productList.FirstOrDefault(p => p.Id == value) ?? prodDefault;
                colorList = new List<ProdColor> { colorDefault };
                if (SelectedProductId != 0)
                {  
                    foreach (ColorProduct colorProduct in _colorProductService.GetAllPerProduct(SelectedProductId))
                    {
                        var colorProd = _colorService.GetById(colorProduct.ColorID);
                        colorList = colorList.Append(colorProd).ToList();
                    }
                    SelectedColor = colorList.FirstOrDefault() ?? colorDefault;
                    ShowInfo();
                }
            }
        }
        ProductFamily prodFamilyDefault = new ProductFamily()
        {
            Id = 0,
            Name = "Aucune"
        };

        ProdColor colorDefault = new ProdColor()
        {
            Id = 0,
            Name = "Aucune",
            ColorNumber = ""
        };
        private ProdColor? SelectedColor { get; set; }
        private int selectedColorId;
        public int SelectedColorId
        {
            get { return selectedColorId; }
            set
            {
                selectedColorId = value;
                SelectedColor = colorList.FirstOrDefault(p => p.Id == value) ?? colorDefault;
                ShowInfo();
            }
        }
        private List<StoreInfo> storeInfos { get; set; }
        public class StoreInfo
        {
            public ProdColor ProdColor { get; set; }
            public List<Container> containers { get; set; }  
            public int ProductCount { get; set; }
        }

        public Dictionary<int, string> cellStatus { get; set; }
 
        private string GetStatusColor(int status) => status switch
        {
            0 => "#B0B0B0", // Light Gray (Undefined)
            1 => "#ADD8E6", // Light Blue (Empty)
            2 => "#FFD580", // Light Orange (InFill)
            3 => "#90EE90", // Light Green (Full)
            99 => "#FFB6C1", // Light Red/Pink (Canceled)
            _ => "#D3D3D3"  // Default Light Gray
        };
        private string GetCardStyle(int status)
        {
            return $"width: 100px; height: 25px; text-align: center; margin: 2px; background-color: {GetStatusColor(status)}; color: black; font-weight: bold;";
        }
        private string GetCircleStyle(int status)
        {
            return $"color: {GetStatusColor(status)};";
        }
        protected override async Task OnInitializedAsync()
        {
            _cellProductList = _cellProductService.GetAll().ToList();
            _contenaireList = _contenaireService.GetAll().ToList();
            _logList = _logService.GetAll().ToList();
            productfamiliesList = _productfamilyService.GetAll().ToList().Concat(new List<ProductFamily> { prodFamilyDefault });
            productList = _productService.GetAllPerProductFamily(SelectedProductFamilyId);
            productList = productList.Concat(new List<Product> { prodDefault }).ToList();
            //colorList = new List<ProdColor> { colorDefault }.ToList();
            
            _workshopList = _workshopService.GetAll().ToList();
            cellStatus = new StatusCell().Status;
            SelectedProductFamilyId = 0;
            SelectedProductId = 0;
            selectedColorId = 0;
            selectedWorkshopId = 1;

            await Task.Run(ShowInfo);
        }

        

        private MudDialog dialog;
        private Cell selectedCell;

        private void ShowCellDetails(Cell cell)
        {
            selectedCell = cell;
            dialog.Show();
        }

        private void CloseDialog()
        {
            dialog.Close();
        }

        void RefreshData()
        {
            
        }
        private int cellNb;
        private int contenaireNb;
        private int productNb;

        void ShowInfo()
        {
            _cellList = _cellService.GetByWorkshop(selectedWorkshopId);
            cellNb = 0;
            contenaireNb = 0;
            productNb = 0;
            if (selectedProductFamilyId == 0)
            {
                refreshCellList = null;
                cellNb = _cellList.Count();
                contenaireNb = _contenaireList.Count(c => c.CellId!=null && c.CellStock?.WorkshopID == selectedWorkshopId);
            }
            else
            {
                if (selectedColorId != 0)
                {
                    refreshCellList = _cellList.Where(c => {
                        var logOfCell = _logList.Where(u => u.CellID == c.Id && u.Container.CellId == c.Id).OrderByDescending(u => u.EventTime).FirstOrDefault();
                        return logOfCell?.ProdColorID == selectedColorId && logOfCell?.ProductID == selectedProductId;
                    }).ToList();
                }
                else if (selectedProductId != 0)
                {
                    refreshCellList = _cellList.Where(c => _logList.Where(u => u.CellID == c.Id && u.Container.CellId == c.Id).OrderByDescending(u => u.EventTime).FirstOrDefault()?.ProductID == selectedProductId).ToList();
                }
                else
                {
                    foreach (var produit in _productService.GetAllPerProductFamily(selectedProductFamilyId))
                    {
                        if (refreshCellList == null)
                        {
                            refreshCellList = _cellList.Where(c => _logList.Where(u => u.CellID == c.Id && u.Container.CellId == c.Id).OrderByDescending(u => u.EventTime).FirstOrDefault()?.ProductID == produit.Id).ToList();
                        }
                        else
                        {
                            refreshCellList = refreshCellList.Concat(_cellList.Where(c => _logList.Where(u => u.CellID == c.Id && u.Container.CellId == c.Id).OrderByDescending(u => u.EventTime).FirstOrDefault()?.ProductID == produit.Id).ToList()).ToList();
                        }
                    }
                }
                cellNb = refreshCellList.Count();
                contenaireNb = _contenaireList.Count(c => refreshCellList.Any(cell => cell.Id == c.CellId));
                if (SelectedProduct != null)
                {

                    productNb = SelectedProduct.QuantityPerContainer * contenaireNb;
                }
            }

            //foreach (Container container in _contenaireService.GetAll().Where(c => c.CellStock != null))
            //{
            //    logRescent = _logService.GetByContenaireByOperationStatus(container.Id, OperationContainer.Store);
            //    if (logRescent != null && logRescent.ProductID == SelectedProduct.Id)
            //    {
            //        // Recherche s'il y a déjà un StoreInfo pour cette couleur de produit
            //        var storeInfo = storeInfos.FirstOrDefault(s => s.ProdColor.Id == logRescent.ProdColor.Id);

            //        if (storeInfo == null)
            //        {
            //            // Si aucun StoreInfo n'existe pour cette couleur de produit, crée un nouveau StoreInfo
            //            storeInfo = new StoreInfo
            //            {
            //                ProdColor = logRescent.ProdColor,
            //                containers = new List<Container> { container },
            //                ProductCount = 1 // Initialise avec le premier produit trouvé
            //            };
            //            storeInfos.Add(storeInfo);
            //        }
            //        else
            //        {
            //            // Si un StoreInfo existe déjà pour cette couleur, ajoute le conteneur et mets à jour le ProductCount
            //            storeInfo.containers.Add(container);
            //            storeInfo.ProductCount++;
            //        }

            //    }
            //}
        }


    }
}