using KANTAIM.APK.MessageBus.Messages;
using KANTAIM.APK.MessageBus;
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
    public partial class FindProductPge : BasePage
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] public ColorService _colorService { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public ColorProductService _colorProductServiceService { get; set; }
        [Inject] public ProfilSessionService _profilSessionService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        [Inject] public ScanService _scanService { get; set; }


        [Parameter] public int Id { get; set; }
        [Parameter] public int Number { get; set; }

        public Product? ProductScanner { get; set; }
        public ProdColor? ColorChoose { get; set; }
        public List<ProdColor> Colors { get; set; }

        public List<Container> ContainerFindList { get; set; }
        public List<Container> ContainerPhantomFindList { get; set; }
        public Container? ContainerScanner { get; set; }
        public DAL.Model.Cell? CellPropose { get; set; }
        public bool ShowAllCells { get; set; }
        public bool ShowAllCellsPhantom { get; set; }
        private bool contFind;

        public bool ContFind
        {
            get { return (ContainerFindList != null && ContainerFindList.Count > 0); }
        }


        public string? TextValue { get; set; }

        void ToggleCellList()
        {
            ShowAllCells = !ShowAllCells;
        }

        void ToggleCellListPhantom()
        {
            ShowAllCellsPhantom = !ShowAllCellsPhantom;
        }

        public class CellLog
        {
            public DAL.Model.Cell Cell { get; set; }
            public DateTime EventTime { get; set; }
        }

        private Profil profilSelected;

        protected override void OnInitialized()
        {
            profilSelected = _profilSessionService.CurrentProfil;
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
        }

        void findCells()
        {
            var rackIds = profilSelected.RackProfils.Select(rp => rp.RackId).ToList();

            var orderedList = _contenaireService.GetAllByOperationStatus(ActionStatus.Store)
                                                .Where(c => c.CellStock != null &&
                                                            c.ProductID == ProductScanner.Id &&
                                                            (ColorChoose == null || c.ProdColorID == ColorChoose.Id) &&
                                                            c.CellID != ContainerScanner?.CellID &&
                                                            !c.CellStock.IsJail &&
                                                            !c.CellStock.IsMaintenance)
                                                .OrderBy(c => c.LastEvent)
                                                .ToList();

            var noPhantom = orderedList.Where(c => !c.CellStock.IsPhantom).ToList();

            ContainerFindList = noPhantom.Where(c => c.CellStock.RackCells != null &&
                                                        c.CellStock.RackCells.Any(rc => rackIds.Contains(rc.RackId)))
                                            .ToList();

            if (!ContainerFindList.Any()) ContainerFindList = noPhantom;

            var phantom = orderedList.Where(c => c.CellStock.IsPhantom).ToList();

            ContainerPhantomFindList = phantom.Where(c => c.CellStock.RackCells != null &&
                                                        c.CellStock.RackCells.Any(rc => rackIds.Contains(rc.RackId)))
                                                 .ToList();

            if (!ContainerPhantomFindList.Any()) ContainerPhantomFindList = phantom;

            CellPropose = ContainerFindList.FirstOrDefault()?.CellStock;

            //ContainerFindList = new List<Container>(_contenaireService.GetAllByOperationStatus(ActionStatus.Store)
            //                                                        .Where(c => c.CellStock != null &&
            //                                                                    c.ProductID == ProductScanner.Id &&
            //                                                                    (ColorChoose == null || c.ProdColorID == ColorChoose.Id) &&
            //                                                                    c.CellID != ContainerScanner?.CellID &&
            //                                                                    !c.CellStock.IsJail &&
            //                                                                    !c.CellStock.IsMaintenance &&
            //                                                                    !c.CellStock.IsPhantom &&
            //                                                                    c.CellStock.RackCells != null &&
            //                                                                    c.CellStock.RackCells.Any(rc => profilSelected.RackProfils.Any(rp => rp.RackId == rc.RackId)))
            //                                                        .OrderBy(c => c.LastEvent));

            //ContainerPhantomFindList = new List<Container>(_contenaireService.GetAllByOperationStatus(ActionStatus.Store)
            //                                                        .Where(c => c.CellStock != null &&
            //                                                                    c.ProductID == ProductScanner.Id &&
            //                                                                    (ColorChoose == null || c.ProdColorID == ColorChoose.Id) &&
            //                                                                    c.CellID != ContainerScanner?.CellID &&
            //                                                                    !c.CellStock.IsJail &&
            //                                                                    !c.CellStock.IsMaintenance &&
            //                                                                    c.CellStock.IsPhantom &&
            //                                                                    c.CellStock.RackCells != null &&
            //                                                                    c.CellStock.RackCells.Any(rc => profilSelected.RackProfils.Any(rp => rp.RackId == rc.RackId)))
            //                                                        .OrderBy(c => c.LastEvent));
            //if (ContainerFindList != null && ContainerFindList.Count > 0) {CellPropose = ContainerFindList.FirstOrDefault()?.CellStock ?? null; }

        }

        async void ColorSelected(int colorid)
        {
            ColorChoose = _colorService.GetById(colorid);
            findCells();
            await InvokeAsync(StateHasChanged);
        }

        void ContainerScan(string code)
        {
            string[] parts = _scanService.ParseCode(code);
            if (parts != null)
            {
                int.TryParse(parts[0], out int type);
                if (type == 1)
                {
                    if (int.TryParse(parts[1], out int containerNumber))
                    {

                        ContainerScanner = _contenaireService.GetContainerByNumber(containerNumber);
                        if (ContainerScanner.ProductID != ProductScanner.Id || ContainerScanner.ProdColorID != ColorChoose.Id)
                        {
                            _snackService.Add("Svp vérifiez le produit dans le contenaire et le produit que vous voulez rechercher !", Severity.Error);
                            TextValue = null;
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
                                        /*Après initialisation, on choisie son fillstatus
                                         * , et après on le mise en rack.*/
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
                    TextValue = null;
                }
            }
        }
        void GoBack() => NavigationManager.NavigateTo("/");

        public override async void OnMessageReceived(InputMessage msg)
        {
            TextValue = msg.Code;
            ContainerScan(msg.Code);

            await InvokeAsync(StateHasChanged);
        }

        private string GetContrastingTextColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor) || !hexColor.StartsWith("#") || (hexColor.Length != 7 && hexColor.Length != 9)) return "#000000"; // fallback 

            // Extraire les composantes R, G, B
            var r = Convert.ToInt32(hexColor.Substring(1, 2), 16);
            var g = Convert.ToInt32(hexColor.Substring(3, 2), 16);
            var b = Convert.ToInt32(hexColor.Substring(5, 2), 16);
            var luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;

            return luminance > 0.5 ? "#000000" : "#FFFFFF";
        }
    }
}