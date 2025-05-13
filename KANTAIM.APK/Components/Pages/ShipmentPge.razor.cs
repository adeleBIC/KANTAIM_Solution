using KANTAIM.APK.Resources;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace KANTAIM.APK.Components.Pages
{
    public partial class ShipmentPge
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] public ColorService _colorService { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] public CellService _cellService { get; set; }
        [Inject] public ActionService _actionService { get; set; }
        [Parameter] public int Id { get; set; }
        [Parameter] public int Number { get; set; }
        public Boolean IsPalette { get; set; } = false;


        public Product? Product { get; set; }
        public DAL.Model.Cell? CellStock { get; set; }
        public ProdColor? ColorOfProduct { get; set; }
        public Container? ContainerScanner { get; set; }
        public Container? PaletteScanner { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        public bool correct { get; set; } = true;

        protected override void OnInitialized()
        {
            ContainerScanner = _contenaireService.GetContainerByNumber(Number);
            if (ContainerScanner.InMaintenance)
            {
                correct = false;
                NavigationManager.NavigateTo($"/");
                _snackService.Add("Le contenaire est en maintenance!", Severity.Error);
                return;
            }
            if (ContainerScanner.InJail)
            {
                correct = false;
                NavigationManager.NavigateTo($"/");
                _snackService.Add("Le contenaire est en prison!", Severity.Error);
                return;
            }
            if (!ContainerScanner.ContainerType.IsContainable && ContainerScanner.ContainerType.NbrMaxContainer > 0 && _contenaireService.CountBac(ContainerScanner.Id) != 0)// S'il est palette et n'est pas vide, on interdit cette opération
            {
                IsPalette = true;
            }

            Product = ContainerScanner.Product;
            ColorOfProduct = ContainerScanner.ProdColor;
            CellStock = ContainerScanner.CellStock;
        }

        void VerifyEmptyPallet(int paletteNumber)
        {
            PaletteScanner = _contenaireService.GetContainerByNumber(paletteNumber);
            if(_contenaireService.CountBac(PaletteScanner.Id) == 0) // s'il n'y a plus de bac sur la palette
            {
                PaletteScanner.ContainerAction = _actionService.GetByStatus(0);// Stocké Vide
                PaletteScanner.ActionID = PaletteScanner.ContainerAction.Id;
                PaletteScanner.FillStatus = StatusContainer.Empty;//Palette statut changé à vide
                _contenaireService.UpSert(PaletteScanner);
            }
        }

        void UpDateCellState(DAL.Model.Cell cell)
        {
            if (_contenaireService.CountCells(cell.Id) == 0) cell.Status = StatusCell.Empty;
            else cell.Status = StatusCell.InFill;

            _cellService.Upsert(cell);

        }

        void Shipment()
        {
            Log u = new Log()
            {
                EventTime = DateTime.Now,
                Operation = OperationContainer.Shipment, // shipment
                Product = ContainerScanner.Product,
                ProductID = ContainerScanner.ProductID,
                Press = ContainerScanner.Press,
                PressID = ContainerScanner.PressID,
                Shape = ContainerScanner.Press.Shape,
                ShapeID = ContainerScanner.Press.ShapeID,
                Container = ContainerScanner,
                ContainerID = ContainerScanner.Id,
                
                FillStatus = ContainerScanner.FillStatus,
                Machine = ContainerScanner.Machine,
                MachineID = ContainerScanner.MachineID,
                Cell = ContainerScanner.CellStock,
                CellID = ContainerScanner.CellID
            };
            if(ColorOfProduct != null )
            {
                u.ProdColor = ColorOfProduct;
                u.ProdColorID = ColorOfProduct.Id;
            }
            _logService.UpSert(u);

            ContainerScanner.LastEvent = u.EventTime;
            ContainerScanner.ContainerAction = _actionService.GetByStatus(3);// Sortie stock
            ContainerScanner.ActionID = ContainerScanner.ContainerAction.Id;
            ContainerScanner.CellID = null;

            if (ContainerScanner.ContainerType.IsContainable == true) //s'il est un bac 
            {
                int paletteNumber = ContainerScanner.BigContainer.Number;
                ContainerScanner.BigContainer = null;
                ContainerScanner.ContainerID = null;
                _contenaireService.UpSert(ContainerScanner);
                VerifyEmptyPallet(paletteNumber);
            } else
            {
                _contenaireService.UpSert(ContainerScanner);
            }

            UpDateCellState(CellStock);

            //shipment = true;
            NavigationManager.NavigateTo($"/");
            _snackService.Add("Bien sortie !", Severity.Success);
        }

    }
}