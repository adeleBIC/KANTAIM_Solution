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
        [Inject] public ProductService _productService { get; set; }
        [Inject] public ColorService _colorService { get; set; }
        [Inject] public ActionService _actionService { get; set; }
        [Inject] public CellService _cellService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        [Parameter] public int Id { get; set; }
        [Parameter] public int Number { get; set; }
        public bool inject { get; set; } = false;

        public string? ContainerValue { get; set; }
        public string? MachineValue { get; set; }
        public Container? ContainerScanner { get; set; }
        public Machine? MachineScanner { get; set; }
        public Log logRescent { get; set; }
        public Product? product { get; set; }
        public ProdColor? prodColor { get; set; }

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
                        logRescent = _logService.GetByContenaireId(ContainerScanner.Id);
                        if (logRescent != null)
                        {
                            product = _productService.GetById(logRescent.ProductID ?? 0);
                            prodColor = _colorService.GetById(logRescent.ProdColorID);
                        }
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
                    _snackService.Add("Svp scanner le QR code de le contenaire qui a sortie de rack avec produits.", Severity.Error);
                    ContainerScanner = null;
                }
                else
                {
                    logRescent = _logService.GetByContenaireId(ContainerScanner.Id);
                    product = _productService.GetById(logRescent.ProductID.Value);
                    prodColor = _colorService.GetById(logRescent.ProdColorID);
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
                /*
                if (MachineScanner.IsInkjet && MachineScanner.ProductID != logRescent.ProductID)
                {
                    _snackService.Add("Le produit dans le contenaire n'est pas correspand avec la machine", Severity.Error);
                    MachineScanner = null;
                }
                */
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

        void Inject()
        {
            var cellstock = ContainerScanner.CellStock;
            Log u = new Log()
            {
                EventTime = DateTime.Now,
                Operation = OperationContainer.Inject, // Mise en machine
                Product = logRescent.Product,
                ProductID = logRescent.ProductID,
                Press = logRescent.Press,
                PressID = logRescent.PressID,
                Shape = logRescent.Shape,
                ShapeID = logRescent.ShapeID,
                Container = ContainerScanner,
                ContainerID = ContainerScanner.Id,
                ProdColorID = logRescent.ProdColorID,
                FillStatus = logRescent.FillStatus,
                Machine = MachineScanner,
                MachineID = MachineScanner.Id
            };
            _logService.UpSert(u);

            ContainerScanner.ContainerAction = _actionService.GetByStatus(4);// En vidange
            ContainerScanner.ActionID = ContainerScanner.ContainerAction.Id;
            ContainerScanner.FillStatus = StatusContainer.Undefinded;
            ContainerScanner.CellId = null;
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