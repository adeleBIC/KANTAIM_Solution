using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace KANTAIM.WEB.Pages.Kanban
{
    public partial class InjectPge
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public MachineService _machineService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] public ScanService _scanService { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] public ColorService _colorService { get; set; }
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

        protected override void OnInitialized()
        {
            ContainerScanner = _contenaireService.GetContainerByNumber(Number).FirstOrDefault();
            logRescent = _logService.GetByContenaireId(ContainerScanner.Id);
            product = _productService.GetById(logRescent.ProductID.Value);
            prodColor = _colorService.GetById(logRescent.ProdColorID);
        }
        void machineScan(KeyboardEventArgs e)
        {

            if (e.Key == "Enter")
            {
                string[] parts = _scanService.scanCode(MachineValue);

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

        }

        void Inject()
            {
            Log u = new Log()
            {
                EventTime = DateTime.Now,
                Operation = 4, // Mise en machine
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
                MachineID = MachineScanner.Id,
                Cell = logRescent.Cell,
                CellID = logRescent.CellID
            };
            _logService.UpSert(u);

            ContainerScanner.ActionID = 4; // En vidange
            ContainerScanner.FillStatus = logRescent.FillStatus;
            _contenaireService.UpSert(ContainerScanner);

            inject = true;
         }    
    }
}