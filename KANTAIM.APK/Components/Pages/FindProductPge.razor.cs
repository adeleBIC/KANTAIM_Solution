using KANTAIM.APK.Resources;
using KANTAIM.APK.Services;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Buffers;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using static System.Collections.Specialized.BitVector32;


namespace KANTAIM.APK.Components.Pages
{
    public partial class FindProductPge
    {

        [Inject] private IJSRuntime JSRuntime { get; set; }
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

        public List<CellLog> cells { get; set; }
        public Log logRescent { get; set; }
        public Container? ContainerScanner { get; set; }
        public DAL.Model.Cell? cellPropose { get; set; }
        public Product? containerProduct { get; set; }
        public ProdColor? containerProdColor { get; set; }
        public string ContainerValue { get; set; }
        public bool ShowAllCells { get; set; }

        private static FindProductPge _instance;
        private string currentUrl;
        private string pageUrl;
        public string? TextValue { get; set; }

        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            // Mettre à jour l'URL actuelle lorsque l'URL change
            currentUrl = e.Location;
            // Vous pouvez ajouter ici toute logique que vous souhaitez exécuter lorsque l'URL change
        }

        public void Dispose()
        {
            // Se désabonner de l'événement pour éviter les fuites de mémoire
            NavigationManager.LocationChanged -= OnLocationChanged;
        }
        [JSInvokable]
        public static void CaptureInputFindProd(string input)
        {
            _instance?.HandleInput(input);
        }

        private string LastKey { get; set; }

        private void HandleInput(string input)
        {

            if (input == "Enter" && currentUrl == pageUrl)
            {
                ContainerScan(TextValue);
                StateHasChanged();
            }
            else
            {
                TextValue += input;
                StateHasChanged();

            }
        }
        void ToggleCellList()
        {
            ShowAllCells = !ShowAllCells;
        }

        public class CellLog
        {
            public DAL.Model.Cell Cell { get; set; }
            public DateTime EventTime { get; set; }
        }


        protected override void OnInitialized()
        {
            currentUrl = NavigationManager.Uri;
            pageUrl = NavigationManager.Uri;
            _instance = this;
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
            NavigationManager.LocationChanged += OnLocationChanged;
        }

        void findCells()
        {
            cells = new List<CellLog>();
            foreach (Container container in _contenaireService.GetAll().Where(c => c.CellStock != null))
            {
                logRescent = _logService.GetByContenaireByOperationStatus(container.Id, OperationContainer.Store);
                if (logRescent != null && logRescent.ProductID == ProductScanner.Id)
                {
                    if (ColorChoose == null || logRescent.ProdColorID == ColorChoose.Id)
                    {
                        if (!cells.Any(c => c.Cell.Id == container.CellStock.Id))
                        {
                            cells.Add(new CellLog { Cell = container.CellStock, EventTime = logRescent.EventTime });
                        }
                    }
                }
            }
            if (cells != null && cells.Count > 0)
            {
                var oldestCellLog = cells.OrderBy(c => c.EventTime).FirstOrDefault();
                if (oldestCellLog != null)
                {
                    cellPropose = oldestCellLog.Cell;
                }
            }
        }


        void ColorSelected(int colorid)
        {
            ColorChoose = _colorService.GetById(colorid);
            findCells();
        }

        void ContainerScan(string code)
        {
            string[] parts = _scanService.scanCode(code);
            if (parts != null)
            {
                int.TryParse(parts[0], out int type);
                if (type == 1)
                {
                    if (int.TryParse(parts[1], out int containerNumber))
                    {

                        ContainerScanner = _contenaireService.GetContainerByNumber(containerNumber);
                        logRescent = _logService.GetByContenaireId(ContainerScanner.Id);
                        containerProduct = _productService.GetById(logRescent.ProductID);
                        containerProdColor = _colorService.GetById(logRescent.ProdColorID);
                        if (containerProduct != ProductScanner || containerProdColor != ColorChoose)
                        {
                            _snackService.Add("Svp vérifiez le produit dans le contenaire et le produit que vous voulez rechercher !", Severity.Error);
                        }
                        else
                        {
                            if (ContainerScanner != null)
                            {
                                switch (ContainerScanner.ContainerAction.Status)
                                {
                                    case 0:
                                        /*Quand on scan un contenaire vide, on l'initialise sur press.*/
                                        NavigationManager.NavigateTo($"/InitialisationPge/0/{containerNumber}");
                                        break;
                                    case 1:
                                        /*Après initialisation, on choisie son fillstatus, et après on le mise en rack.*/
                                        NavigationManager.NavigateTo($"/StockagePge/1/{containerNumber}");
                                        break;
                                    case 2:
                                        /*Traite un contenaire qui stock avec produit, on peut sortir stock ou le déplacer.*/
                                        NavigationManager.NavigateTo($"/ShipmentPge/1/{containerNumber}");
                                        break;
                                    case 3:
                                        /*Après sortie le contenaire avec produit, on vas le mise en Machine*/
                                        NavigationManager.NavigateTo($"/InjectPge/1/{containerNumber}");
                                        break;
                                    case 4:
                                        /*Apres vidange le contenaire est vide, on Mise en rack.*/
                                        NavigationManager.NavigateTo($"/StockagePge/1/{containerNumber}");
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    _snackService.Add("Scannez une contenaire s'il vous plaît  !", Severity.Error);
                }
            }
        }
        void GoBack()
        {
            NavigationManager.NavigateTo("/");
        }


    }
}