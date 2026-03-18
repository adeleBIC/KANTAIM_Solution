using Azure;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Ressources;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using static KANTAIM.WEB.Pages.Consultation.ScanInfo.FindPge;
using static KANTAIM.WEB.Pages.Kanban.FindProductPge;
using static MudBlazor.Colors;
using System.Diagnostics;

namespace KANTAIM.WEB.Pages.Consultation.ScanInfo
{
    public partial class FindPge
    {
        [Inject] public ProductFamilyService _productfamilyService { get; set; }
        private List<ProductFamily> _productfamilyList { get; set; }
        [Inject] public ProductService _productService { get; set; }
        private List<Product> _productList { get; set; }
        //[Inject] public LogService _logService { get; set; }
        //private List<Log> _logList { get; set; }
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

        public class ProductColorNode
        {
            public ProdColor ProdColor { get; set; }
            public int TotalContainers { get; set; }
            public int QuantityPerContainer { get; set; }
            public int TotalQuantity { get; set; }
            public int SortieContainers { get; set; }
            public int SortieQuantity { get; set; }
        }

        public class ProductNode
        {
            public Product Product { get; set; }
            public List<ProductColorNode> Colors { get; set; } = new();
            public int TotalContainers => Colors.Sum(c => c.TotalContainers);
            public int TotalQuantity => Colors.Sum(c => c.TotalQuantity);
            public int SortieContainers => Colors.Sum(c => c.SortieContainers);
            public int SortieQuantity => Colors.Sum(c => c.SortieQuantity);

        }

        public class ProductFamilyNode
        {
            public ProductFamily ProductFamily { get; set; }
            public List<ProductNode> Products { get; set; } = new();
            public int TotalContainers => Products.Sum(p => p.TotalContainers);
            public int TotalQuantity => Products.Sum(p => p.TotalQuantity);
            public int SortieContainers => Products.Sum(p => p.SortieContainers);
            public int SortieQuantity => Products.Sum(p => p.SortieQuantity);


        }

        private List<ProductFamilyNode> productTree = new();

        private void initialTable()
        {
            productTree.Clear();

            var families = _productfamilyService.GetAll();
            var products = _productService.GetAll().Where(p => p.Active).ToList();
            var colorLinks = _colorProductService.GetAll();
            var colors = _colorService.GetAll();

            var stockContainers = _contenaireList
                .Where(c => c.CellID != null && c.ProductID != null && c.ProdColorID != null)
                .ToList();

            var sortieContainers = _contenaireList
                .Where(c => c.ActionID == ActionStatus.InDischarge)
                .ToList();

            var stockGroups = stockContainers
                .GroupBy(c => (c.ProductID, c.ProdColorID))
                .ToDictionary(g => g.Key, g => g.ToList());

            var sortieGroups = sortieContainers
                .GroupBy(c => (c.ProductID, c.ProdColorID))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var family in families)
            {
                var familyNode = new ProductFamilyNode
                {
                    ProductFamily = family
                };

                var familyProducts = products
                    .Where(p => p.ProductFamilyID == family.Id)
                    .ToList();

                foreach (var product in familyProducts)
                {
                    var productNode = new ProductNode
                    {
                        Product = product
                    };

                    var productColors = colorLinks
                        .Where(cp => cp.ProductID == product.Id)
                        .Select(cp => colors.FirstOrDefault(c => c.Id == cp.ColorID))
                        .Where(c => c != null)
                        .ToList();

                    foreach (var color in productColors)
                    {
                        var key = (product.Id, color.Id);

                        stockGroups.TryGetValue(key, out var stockList);
                        sortieGroups.TryGetValue(key, out var sortieList);

                        stockList ??= new List<Container>();
                        sortieList ??= new List<Container>();

                        stockList = stockList
                            .Where(c =>
                                c.CellStock != null &&
                                c.CellStock.RackCells != null &&
                                c.CellStock.RackCells.Any(rc => rc.Rack.WorkshopID == selectedWorkshopId))
                            .ToList();

                        double stockWeighted = 0;
                        int stockCount = 0;

                        foreach (var container in stockList)
                        {
                            if (container.ContainerType.NbrMaxContainer > 0)
                                continue;

                            double weight = container.FillStatus switch
                            {
                                StatusContainer.Empty => 0,
                                StatusContainer.HalfFull => 0.25,
                                StatusContainer.Full => 1,
                                _ => 0
                            };

                            stockWeighted += weight;
                            stockCount++;
                        }

                        double sortieWeighted = 0;
                        int sortieCount = 0;


                        foreach (var container in sortieList)
                        {
                            double weight = container.FillStatus switch
                            {
                                StatusContainer.Empty => 0,
                                StatusContainer.HalfFull => 0.25,
                                StatusContainer.Full => 1,
                                _ => 0
                            };

                            sortieWeighted += weight;
                            sortieCount++;
                            Debug.WriteLine($"[SORTIE] Produit: {product.Name} | ContainerID: {container.Id} | FillStatus: {container.FillStatus} | Weight: {weight}");
                        }

                        var colorNode = new ProductColorNode
                        {
                            ProdColor = color,
                            QuantityPerContainer = product.QuantityPerContainer,

                            TotalContainers = stockCount,
                            TotalQuantity = (int)(stockWeighted * product.QuantityPerContainer),

                            SortieContainers = sortieCount,
                            SortieQuantity = (int)(sortieWeighted * product.QuantityPerContainer)
                        };

                        productNode.Colors.Add(colorNode);
                    }

                    if (productNode.Colors.Any())
                        familyNode.Products.Add(productNode);
                }

                if (familyNode.Products.Any())
                    productTree.Add(familyNode);
            }
        }

        public int SelectedWorkshopId
        {
            get { return selectedWorkshopId; }
            set
            {
                selectedWorkshopId = value;
                _cellList = _cellService.GetByWorkshop(selectedWorkshopId);
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
                productList = productList.Concat(new List<Product> { prodDefault }).Where(p=>p.Active).ToList();
                SelectedProductId = 0;
                ShowInfo();
            }
        }

        private Product? SelectedProduct { get; set; }
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
            productfamiliesList = _productfamilyService.GetAll().ToList().Concat(new List<ProductFamily> { prodFamilyDefault });
            productList = _productService.GetAllPerProductFamily(SelectedProductFamilyId).Where(p=>p.Active);
            productList = productList.Concat(new List<Product> { prodDefault }).ToList();
            //colorList = new List<ProdColor> { colorDefault }.ToList();
            
            _workshopList = _workshopService.GetAll().ToList();
            cellStatus = new StatusCell().Status;
            SelectedProductFamilyId = 0;
            SelectedProductId = 0;
            selectedColorId = 0;
            selectedWorkshopId = 1;
            _cellList = _cellService.GetByWorkshop(selectedWorkshopId);
            initialTable();
            await Task.Run(ShowInfo);
        }

        

        private MudDialog dialog;
        private Cell selectedCell;

        private void ShowCellDetails(Cell cell)
        {
            selectedCell = cell;
            dialog.Show();
        }

        private string GetFamilyHeader(ProductFamilyNode family)
        {
            return $"{family.ProductFamily.Name} " +
                   $"(Stock — Conteneurs: {family.TotalContainers:N0}, Qté: {family.TotalQuantity:N0} | " +
                   $"Sortie — Conteneurs: {family.SortieContainers:N0}, Qté: {family.SortieQuantity:N0})";
        }

        private string GetProductHeader(ProductNode product)
        {
            return $"{product.Product.Name} " +
                   $"(Stock — Conteneurs: {product.TotalContainers:N0}, Qté: {product.TotalQuantity:N0} | " +
                   $"Sortie — Conteneurs: {product.SortieContainers:N0}, Qté: {product.SortieQuantity:N0})";
        }

        private int cellNb;
        private int contenaireNb;
        private int productNb;

        void ShowInfo()
        {
            if (selectedWorkshopId == 0)
                return;

            cellNb = 0;
            contenaireNb = 0;
            productNb = 0;

            refreshCellList = null;

            if (selectedProductFamilyId == 0)
            {
                cellNb = _cellList.Count();
                contenaireNb = productTree.Sum(f => f.TotalContainers) + productTree.Sum(f => f.SortieContainers);
                productNb = productTree.Sum(f => f.TotalQuantity + f.SortieQuantity);
            }
            else
            {
                var familyNode = productTree.FirstOrDefault(f => f.ProductFamily.Id == selectedProductFamilyId);
                if (familyNode != null)
                {
                    if (selectedProductId != 0)
                    {
                        var productNode = familyNode.Products.FirstOrDefault(p => p.Product.Id == selectedProductId);
                        if (productNode != null)
                        {
                            if (selectedColorId != 0)
                            {
                                var colorNode = productNode.Colors.FirstOrDefault(c => c.ProdColor.Id == selectedColorId);
                                if (colorNode != null)
                                {
                                    contenaireNb = colorNode.TotalContainers + colorNode.SortieContainers;
                                    productNb = colorNode.TotalQuantity + colorNode.SortieContainers;

                                    refreshCellList = _cellList
                                        .Where(cell => cell.Containers.Any(c =>
                                            c.ProductID == selectedProductId &&
                                            c.ProdColorID == selectedColorId))
                                        .ToList();
                                }
                            }
                            else
                            {
                                contenaireNb = productNode.TotalContainers + productNode.SortieContainers;
                                productNb = productNode.TotalQuantity + productNode.SortieQuantity;
                                refreshCellList = _cellList
                                    .Where(cell => cell.Containers.Any(c =>
                                        c.ProductID == selectedProductId))
                                    .ToList();
                            }
                        }
                    }
                    else
                    {
                        contenaireNb = familyNode.TotalContainers + familyNode.SortieContainers;
                        productNb = familyNode.TotalQuantity + familyNode.SortieQuantity;

                        var productIds = familyNode.Products.Select(p => p.Product.Id).ToHashSet();
                        refreshCellList = _cellList
                            .Where(cell => cell.Containers.Any(c =>
                                productIds.Contains(c.ProductID ?? -1)))
                            .ToList();
                    }

                    cellNb = refreshCellList?.Count ?? 0;
                }
            }
        }



    }
}