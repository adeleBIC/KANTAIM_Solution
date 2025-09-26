using KANTAIM.APK.MessageBus.Messages;
using KANTAIM.APK.Resources;
using KANTAIM.APK.Services;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

namespace KANTAIM.APK.Components.Pages
{
    public partial class InjectPge : BasePage
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public MachineService _machineService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] public ScanService _scanService { get; set; }
        [Inject] public ActionService _actionService { get; set; }
        [Inject] public CellService _cellService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        [Parameter] public int Id { get; set; }
        [Parameter] public int Number { get; set; }
        public bool inject { get; set; } = false;
        public Container? ContainerScanner { get; set; }
        public Machine? MachineScanner { get; set; }
        //public Log logRescent { get; set; }
        public Product? Product { get; set; }
        public ProdColor? ProdColor { get; set; }

        public string? TextValue { get; set; }

        private int state;

        public int State
        {
            get
            {
                if (MachineScanner != null && ContainerScanner != null) state = 4;
                else if (MachineScanner == null && ContainerScanner != null) state = 2;
                else if (MachineScanner != null && ContainerScanner == null) state = 3;

                return state;
            }
        }
        protected override void OnInitialized()
        {
            switch (Id)
            {
                //Scanner le contenaire
                case 1:
                    ContainerScanner = _contenaireService.GetContainerByNumber(Number);
                    if (ContainerScanner != null)
                    {
                        Product = ContainerScanner.Product;
                        ProdColor = ContainerScanner.ProdColor;
                    }
                    break;
                //Scanner la machine
                case 2:
                    MachineScanner = _machineService.GetByNumber(Number);
                    break;
            }
        }

        void contenaireScan(string code)
        {
            string[] parts = _scanService.ParseCode(code);

            int.TryParse(parts[0], out int type);
            if (type != 1)
            {
                _snackService.Add("Svp scanner le QR code de le contenaire.", Severity.Error);
            }
            else
            {
                int.TryParse(parts[1], out int Container);
                ContainerScanner = _contenaireService.GetContainerByNumber(Container);
                if (ContainerScanner.ContainerAction.Status != 3)
                {
                    _snackService.Add("Svp scanner le QR code du contenaire qui a déjà été sorti du rack avec produits.", Severity.Error);
                    ContainerScanner = null;
                }
                else
                {
                    Product = ContainerScanner.Product;
                    ProdColor = ContainerScanner.ProdColor;
                }

            }
        }
        void machineScan(string code)
        {

            string[] parts = _scanService.ParseCode(code);

            int.TryParse(parts[0], out int type);
            if (type != 2)
            {
                _snackService.Add("Svp scanner le QR code de la Machine.", Severity.Error);
            }
            else
            {
                int.TryParse(parts[1], out int MachineNumber);
                MachineScanner = _machineService.GetByNumber(MachineNumber);
            }

        }

        void returnAction()
        {
            switch (Id)
            {
                //Scanner le contenaire first, so the last step is scan machine
                case 1:
                    MachineScanner = null;
                    break;
                //Scanner la machine first, so the last step is scan contenaire
                case 2:
                    ContainerScanner = null;
                    break;
            }
        }

        void upDateCellState(DAL.Model.Cell cell)
        {
            if (cell != null)
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
        }

        void VerifyEmptyPallet(Container PaletteScanner)
        {
            //PaletteScanner = _contenaireService.GetContainerByNumber(paletteNumber);
            if (PaletteScanner != null && _contenaireService.CountBac(PaletteScanner.Id) <= 1) // s'il n'y a plus de bac sur la palette
            {
                PaletteScanner.ContainerAction = _actionService.GetByStatus(0);// Stocké Vide
                PaletteScanner.ActionID = PaletteScanner.ContainerAction.Id;
                PaletteScanner.FillStatus = StatusContainer.Empty;//Palette statut changé à vide
                PaletteScanner.CellStock = null;
                PaletteScanner.CellID = null;
                _contenaireService.UpSert(PaletteScanner);
            }
            else if (PaletteScanner.FillStatus == StatusContainer.Full)
            {
                PaletteScanner.FillStatus = StatusContainer.HalfFull;//Palette statut changé à semi-pleine
                _contenaireService.UpSert(PaletteScanner);
            }
        }


            void Inject()
            {
                var cellstock = ContainerScanner.CellStock;
                Log u = new Log()
                {
                    EventTime = DateTime.Now,
                    Operation = OperationContainer.Inject, // Mise en machine
                    Product = ContainerScanner.Product,
                    ProductID = ContainerScanner.ProductID,
                    Press = ContainerScanner.Press,
                    PressID = ContainerScanner.PressID,
                    Shape = ContainerScanner.Press?.Shape,
                    ShapeID = ContainerScanner.Press?.ShapeID,
                    Container = ContainerScanner,
                    ContainerID = ContainerScanner.Id,
                    ProdColorID = ContainerScanner.ProdColorID,
                    FillStatus = ContainerScanner.FillStatus,
                    Machine = MachineScanner,
                    MachineID = MachineScanner.Id
                };
                _logService.UpSert(u);

                if (ContainerScanner.ContainerType.IsContainable)
                {
                    ContainerScanner.ContainerAction = _actionService.GetByStatus(OperationContainer.Undefinded);// Stocké Vide
                    ContainerScanner.ActionID = ContainerScanner.ContainerAction.Id;
                    ContainerScanner.FillStatus = StatusContainer.Empty;
                    ContainerScanner.Product = null;
                    ContainerScanner.ProductID = null;
                    ContainerScanner.ProdColor = null;
                    ContainerScanner.ProdColorID = null;
                    ContainerScanner.Press = null;
                    ContainerScanner.PressID = null;
                    ContainerScanner.MachineID = MachineScanner.Id;
                    VerifyEmptyPallet(ContainerScanner.BigContainer);
                }
                else
                {
                    ContainerScanner.ContainerAction = _actionService.GetByStatus(4);// En vidange
                    ContainerScanner.ActionID = ContainerScanner.ContainerAction.Id;
                    ContainerScanner.FillStatus = StatusContainer.Undefinded;
                    ContainerScanner.MachineID = MachineScanner.Id;
                }

                ContainerScanner.CellID = null;
                ContainerScanner.ContainerID = null;
                ContainerScanner.LastEvent = u.EventTime;
                _contenaireService.UpSert(ContainerScanner);

                upDateCellState(cellstock);
                inject = true;
                NavigationManager.NavigateTo($"/");
                _snackService.Add("Réussi !", Severity.Success);
            }
        

        public override async void OnMessageReceived(InputMessage msg)
        {
            TextValue = msg.Code;
            switch (state)
            {
                case 2:
                    machineScan(msg.Code);
                    break;
                case 3:
                    contenaireScan(msg.Code);
                    break;
            }

            await InvokeAsync(StateHasChanged);
        }
    }
}