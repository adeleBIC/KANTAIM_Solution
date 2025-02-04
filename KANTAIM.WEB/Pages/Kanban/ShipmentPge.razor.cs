using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Ressources;
using KANTAIM.WEB.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KANTAIM.WEB.Pages.Kanban
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


        public Product? product { get; set; }
        public Cell? cellStock { get; set; }
        //public Product? productModified { get; set; }
        public ProdColor? colorOfProduct { get; set; }
        public bool shipment { get; set; } = false;
        public Container? ContainerScanner { get; set; }
        public Container? PaletteScanner { get; set; }
        public Log? logRescent { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        public bool correct { get; set; } = true;

        protected override void OnInitialized()
        {
            ContainerScanner = _contenaireService.GetContainerByNumber(Number);
            //if(ContainerScanner.ContainerType.Name == "Palette" && _contenaireService.CountBac(ContainerScanner.Id) != 0)// S'il est palette et n'est pas vide, on interdit cette opération
            //{
            //    correct = false;
            //    NavigationManager.NavigateTo($"/ScannerPge");
            //    _snackService.Add("Scannez les bacs à la place de la palette", Severity.Error);
                
            //}
            if (ContainerScanner.ContainerTypeID == 2) //s'il est un bac 
            {
                logRescent = _logService.GetByContenaireId(ContainerScanner.ContainerID.Value);
            }
            else
            {
                logRescent = _logService.GetByContenaireId(ContainerScanner.Id);
            }
            if (logRescent != null)
            {
                product = _productService.GetById((int)logRescent.ProductID);
                colorOfProduct = _colorService.GetById(logRescent.ProdColorID);
                cellStock = _cellService.GetById(logRescent.CellID);
            }
        }

        void VerifyPaletteEmpty(int paletteNumber)
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


        void upDateCellState(Cell cell)
        {
            if (_contenaireService.CountCells(cell.Id) == 0)
            {
                cell.Status = StatusCell.Empty;
            }
            else
            {
                cell.Status = StatusCell.InFill;
            }

            _cellService.Upsert(cell);

        }

        void Shipment()
        {
            
            Log u = new Log()
            {
                EventTime = DateTime.Now,
                Operation = OperationContainer.Shipment, // shipment
                Product = logRescent.Product,
                ProductID = logRescent.ProductID,
                Press = logRescent.Press,
                PressID = logRescent.PressID,
                Shape = logRescent.Shape,
                ShapeID = logRescent.ShapeID,
                Container = ContainerScanner,
                ContainerID = ContainerScanner.Id,
                
                FillStatus = logRescent.FillStatus,
                Machine = logRescent.Machine,
                MachineID = logRescent.MachineID,
                Cell = logRescent.Cell,
                CellID = logRescent.CellID
            };
            if(colorOfProduct != null )
            {
                u.ProdColor = colorOfProduct;
                u.ProdColorID = colorOfProduct.Id;
            }
            _logService.UpSert(u);

            ContainerScanner.ContainerAction = _actionService.GetByStatus(3);// Sortie stock
            ContainerScanner.ActionID = ContainerScanner.ContainerAction.Id;
            ContainerScanner.CellId = null;
            if (ContainerScanner.ContainerType.IsContainable == true) //s'il est un bac 
            {
                int paletteNumber = ContainerScanner.BigContainer.Number;
                ContainerScanner.BigContainer = null;
                ContainerScanner.ContainerID = null;
                _contenaireService.UpSert(ContainerScanner);
                VerifyPaletteEmpty(paletteNumber);
            } else
            {
                _contenaireService.UpSert(ContainerScanner);
            }

            upDateCellState(ContainerScanner.CellStock);

            //shipment = true;
            NavigationManager.NavigateTo($"/ScannerPge");
            _snackService.Add("Bien sortie !", Severity.Success);
        }

    }
}