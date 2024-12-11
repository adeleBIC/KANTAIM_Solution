using Azure;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Ressources;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using static KANTAIM.WEB.Pages.Kanban.FindProductPge;

namespace KANTAIM.WEB.Pages.Consultation.ScanInfo
{
    public partial class FindPge
    {
        [Inject] public ProductFamilyService _productfamilyService { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] public ColorProductService _colorProductServiceService { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public ColorService _colorService { get; set; }
        public List<ProdColor> Colors { get; set; }
        public List<CellLog> cells { get; set; }

        private string _searchString;
        private IEnumerable<ProductFamily> productfamiliesList;
        private IEnumerable<Product> productList;
        public List<DataProd> DataProds { get; set; }
        public Product? SelectedProduct { get; set; }
        public Log logRescent { get; set; }
        

        public int SelectedProductId { get; set; }
        public int SelectedProductFamilyId { get; set; }
        public List<StoreInfo> storeInfos { get; set; }
        public class StoreInfo
        {
            public ProdColor ProdColor { get; set; }
            public List<Container> containers { get; set; }
            public int ProductCount { get; set; }
        }
        protected override async Task OnInitializedAsync()
        {
            SelectedProductFamilyId = 1;
            SelectedProductId = 0;
            productfamiliesList = _productfamilyService.GetAll();
            productList = _productService.GetAllPerProductFamily(SelectedProductFamilyId);
            await Task.Run(RefreshData);
        }
        private Func<StoreInfo, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.ProdColor.ToString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        void RefreshData()
        {
            productList = _productService.GetAllPerProductFamily(SelectedProductFamilyId);
            if(SelectedProductId != 0)
            {
                SelectedProduct = _productService.GetById(SelectedProductId);
                foreach (ColorProduct colorProduct in _colorProductServiceService.GetAllPerProduct(SelectedProductId))
                {
                    Colors.Add(_colorService.GetById(colorProduct.ColorID));
                }
                if (Colors.Count() == 0)
                {
                    ShowInfo();
                }
            }
        }

        void ShowInfo()
        {
            storeInfos = new List<StoreInfo>();
            foreach (Container container in _contenaireService.GetAll().Where(c => c.CellStock != null))
            {
                logRescent = _logService.GetByContenaireByOperationStatus(container.Id, OperationContainer.Store);
                if (logRescent != null && logRescent.ProductID == SelectedProduct.Id)
                {
                    // Recherche s'il y a déjà un StoreInfo pour cette couleur de produit
                    var storeInfo = storeInfos.FirstOrDefault(s => s.ProdColor == logRescent.ProdColor);

                    if (storeInfo == null)
                    {
                        // Si aucun StoreInfo n'existe pour cette couleur de produit, crée un nouveau StoreInfo
                        storeInfo = new StoreInfo
                        {
                            ProdColor = logRescent.ProdColor,
                            containers = new List<Container> { container },
                            ProductCount = 1 // Initialise avec le premier produit trouvé
                        };
                        storeInfos.Add(storeInfo);
                    }
                    else
                    {
                        // Si un StoreInfo existe déjà pour cette couleur, ajoute le conteneur et mets à jour le ProductCount
                        storeInfo.containers.Add(container);
                        storeInfo.ProductCount++;
                    }

                }
            }
        }


    }
}