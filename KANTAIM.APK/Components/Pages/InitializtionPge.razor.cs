using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.APK.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Charts;
using MudBlazor;
using MudBlazor.Services;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Routing;
using KANTAIM.APK.Services;
using KANTAIM.APK.MessageBus.Messages;
using KANTAIM.APK.MessageBus;

namespace KANTAIM.APK.Components.Pages
{
    public partial class InitializtionPge : BasePage
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] public PressService _pressService { get; set; }
        [Inject] public ColorService _colorService { get; set; }
        [Inject] public ScanService _scanService { get; set; }
        [Inject] public MachineService _machineService { get; set; }
        [Inject] public ActionService _actionService { get; set; }
        [Inject] public ColorProductService _colorProductServiceService { get; set; }
        [Inject] public CellService _cellService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        [Parameter] public int Id { get; set; }
        [Parameter] public int Number { get; set; }

        public Boolean BacInitialisation { get; set; } = false;
        public Boolean Maintenance { get; set; } = false;
        
        public Container? ContainerScanner { get; set; }
        public Press? PressScanner { get; set; }
        public ProdColor? ColorChoose { get; set; }
        public List<ProdColor> Colors { get; set; }
        public Container? PaletteScanner { get; set; }
        public Machine? MachineScanner { get; set; }
        //public Log logRescent { get; set; }
        public string? TextValue { get; set; }

        private int state;
        public int State
        {
            get
            {
                state = 0;
                if(Colors == null && (PressScanner != null || MachineScanner != null)) // quand on n'a pas la liste de couleurs et on a déjà su le press scanner, on récupére la liste de colors 
                {
                    getColors();
                }
                if (ColorChoose != null || (ContainerScanner != null && Colors != null && Colors.Count() == 0)) state = 4;
                else if (ContainerScanner != null && (PressScanner != null || MachineScanner != null)) state = 3;
                else if (ContainerScanner != null && BacInitialisation == false) state = 2;
                else if (ContainerScanner != null && BacInitialisation == true) state = 1;
                else if (PressScanner != null || MachineScanner != null) state = 0;

                return state; 
            }
        }

        protected override async Task OnInitializedAsync()
        {
            switch (Id)
            {
                case 0:
                    ContainerScanner = _contenaireService.GetContainerByNumber(Number);
                    if (ContainerScanner.InMaintenance)
                    {
                        Maintenance = true;
                        ContainerScanner = null;
                        NavigationManager.NavigateTo($"/");
                        _snackService.Add("Le contenaire est en maintenance!", Severity.Error);
                        return;
                    }
                    if (ContainerScanner.ContainerType.IsContainable) // Quand on scan le bac
                    {
                        BacInitialisation = true;
                    }
                    break;
                case 3:
                    PressScanner = _pressService.GetByNumber(Number);
                    break;
                default:
                    break;
            }
        }

        void PaletteScan(string code)
        {
            string[] parts = _scanService.ParseCode(code);
            if (parts != null)
            {
                int.TryParse(parts[0], out int type);
                int.TryParse(parts[1], out int PaletteNumber);

                if (type == 1)
                {
                    PaletteScanner = _contenaireService.GetContainerByNumber(PaletteNumber);
                    if (PaletteScanner.InMaintenance)
                    {
                        Maintenance = true;
                        PaletteScanner = null;
                        NavigationManager.NavigateTo($"/");
                        _snackService.Add("Le contenaire est en maintenance!", Severity.Error);
                        return;
                    }
                    if (PaletteScanner.ContainerAction.Status == 0) // On ne peux pas mettre une bac vide dans une palette, on ne peux pas mettre un bac dans une palette qu'il n'as pas été initialisé.
                    {
                        _snackService.Add("Svp initialisez palette d'abord!", Severity.Error);
                        PaletteScanner = null;
                    }
                    else
                    {
                        TransferBacToPalette(ContainerScanner, PaletteScanner);
                    }

                }
            }
        }
        void TransferBacToPalette(Container bac, Container palette)
        {
            if (bac.ContainerID == palette.Id || bac.FillStatus > StatusContainer.Empty)
            {
                _snackService.Add("Bac déjà présent dans une palette ou non vide !", Severity.Error);
            } else
            {
                bac.CellID = palette.CellID;
                bac.ActionID = palette.ActionID;
                bac.FillStatus = StatusContainer.Full;
                bac.Status = palette.Status;
                bac.InJail = palette.InJail;
                bac.InMaintenance = palette.InMaintenance;
                bac.Comment = palette.Comment;
                bac.ContainerID = palette.Id;
                bac.PressID = palette.PressID;
                bac.LastEvent = DateTime.Now;
                bac.Product = palette.Product;
                bac.ProductID = palette.ProductID;
                bac.ProdColor = palette.ProdColor;
                bac.ProdColorID = palette.ProdColorID;
                _contenaireService.UpSert(bac);


                Log bacLog = new Log()
                {
                    EventTime = bac.LastEvent.Value,
                    Operation = OperationContainer.Initisalisation, // Initialisation pour le bac
                    ProductID = palette.ProductID,
                    Press = palette.Press,
                    PressID = palette.PressID,
                    Shape = palette.Press?.Shape,
                    ShapeID = palette.Press?.ShapeID,
                    Container = bac,
                    ContainerID = bac.Id,
                    BigContainer = palette.Number,
                    ProdColor = palette.ProdColor,
                    ProdColorID = palette.ProdColorID,
                    //CellID = logRescent.CellID,
                    FillStatus = StatusContainer.Full
                };
                _logService.UpSert(bacLog);
                NavigationManager.NavigateTo($"/");
                _snackService.Add("Réussi !", Severity.Success);
            }
            
        }
        void PressScan(string code)
        {
            string[] parts = _scanService.ParseCode(code);
            if (parts != null)
            {
                int.TryParse(parts[0], out int type);
                int.TryParse(parts[1], out int Number);

                if (type == 3 && Number > 0) // scanner une press
                {
                    PressScanner = _pressService.GetByNumber(Number);
                    return;
                }
                else if (type == 2 && Number > 0) // scanner un machine inject pour initialiser
                {
                    MachineScanner = _machineService.GetByNumber(Number);
                    if(MachineScanner.IsInkjet == false)
                    {
                        _snackService.Add("Ce n'est pas une machine INK JET !", Severity.Error);
                        MachineScanner = null;
                       
                    }
                    return;
                }

                _snackService.Add("Svp scanner une presse ou machine INK JET!", Severity.Error);
            }
        }

        void getColors()
        {
            Colors = new List<ProdColor>();
            if(PressScanner != null)
            {
                foreach (ColorProduct colorProduct in _colorProductServiceService.GetAllPerProduct(PressScanner.Shape.ProductID))
                {
                    Colors.Add(_colorService.GetById(colorProduct.ColorID));
                }
            }
            if(MachineScanner != null)
            {
                foreach (ColorProduct colorProduct in _colorProductServiceService.GetAllPerProduct(MachineScanner.ProductID))
                {
                    Colors.Add(_colorService.GetById(colorProduct.ColorID));
                }
            }
            Colors = Colors.OrderBy(x => x.Name).ToList();
            
        }
        void ContainerScan(string code)
        {
            string[] parts = _scanService.ParseCode(code);

            int.TryParse(parts[0], out int type);
            int.TryParse(parts[1], out int ContainerNumber);

            if (type == 1 && ContainerNumber > 0)
            {
                ContainerScanner = _contenaireService.GetContainerByNumber(ContainerNumber);
                if (ContainerScanner.InMaintenance)
                {
                    Maintenance = true;
                    ContainerScanner = null;
                    NavigationManager.NavigateTo($"/");
                    _snackService.Add("Le contenaire est en maintenance!", Severity.Error);
                    return;
                }
                // Vérifier si le Contenaire que l'on veut initialiser est bien vide
                if (ContainerScanner.ContainerAction.Status != 0)
                {
                    ContainerScanner = null;
                    _snackService.Add("Contenaire doit être vide pour l'initialiser sous la presse!", Severity.Error);
                }

            } else
            {
                _snackService.Add("Scannez une contenaire ou palette s'il vous plaît!", Severity.Error);
            }

        }

        void ColorSelected(int colorid)
        {
            ColorChoose = _colorService.GetById(colorid);
        }

        void GoBack(int step)
        {
            switch(step)
            {
                case 1:
                    if (Id == 0)
                    {
                        //PressValue = null;
                        PressScanner = null;
                        MachineScanner = null;
                    }  
                    else if (Id == 3)
                    {
                        //ContainerValue = null;
                        ContainerScanner = null;
                    }
                    break;
                case 2:
                    ColorChoose = null;
                    break;
                default:
                    break;

            }
            StateHasChanged();
        }

        void upDateCellState(DAL.Model.Cell cell)
        {
            if(cell != null)
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

        private void ClearTextField()
        {
            TextValue = string.Empty;
            //scanFieldCont.FocusAsync();
        }


        void SaveLog()
        {
            Log u = new Log()
            {
                EventTime = DateTime.Now,
                Operation = 1,
                Container = ContainerScanner,
                ContainerID = ContainerScanner.Id,
                
            };
            if(PressScanner != null)
            {
                u.Product = PressScanner.Shape.Product;
                u.ProductID = PressScanner.Shape.Product.Id;
                u.Press = PressScanner;
                u.PressID = PressScanner.Id;
                u.Shape = PressScanner.Shape;
                u.ShapeID = PressScanner.Shape.Id;

                ContainerScanner.Product = PressScanner.Shape.Product;
                ContainerScanner.ProductID = PressScanner.Shape.Product.Id;
                ContainerScanner.Press = PressScanner;
                ContainerScanner.PressID = PressScanner.Id;
            }
            if(MachineScanner != null)
            {
                u.Product = MachineScanner.Product;
                u.ProductID = MachineScanner.ProductID;
                u.Machine = MachineScanner;
                u.MachineID = MachineScanner.Id;

                ContainerScanner.Product = MachineScanner.Product;
                ContainerScanner.ProductID = MachineScanner.ProductID;
                ContainerScanner.Machine = MachineScanner;
                ContainerScanner.MachineID = MachineScanner.Id;
            }
            if (ColorChoose != null)
            {
                u.ProdColor = ColorChoose;
                u.ProdColorID = ColorChoose.Id;
                ContainerScanner.ProdColor = ColorChoose;
                ContainerScanner.ProdColorID = ColorChoose.Id;
            }
            _logService.UpSert(u);

            ContainerScanner.ContainerAction = _actionService.GetByStatus(1);
            ContainerScanner.ActionID = ContainerScanner.ContainerAction.Id;
            //ContainerScanner.CellStock = null;
            ContainerScanner.CellID = null;
            if(ContainerScanner.ContainerTypeID == 2) ContainerScanner.Status = 3;// S'il est un bac, on initialise son fillstatue en plein directement
            ContainerScanner.FillStatus = StatusContainer.Undefinded;
            ContainerScanner.LastEvent = u.EventTime;

            _contenaireService.UpSert(ContainerScanner);

            upDateCellState(ContainerScanner.CellStock);
            
            NavigationManager.NavigateTo("/");
            _snackService.Add("Bien initialisé !", Severity.Success);
        }

        public override async void OnMessageReceived(InputMessage msg)
        {
            TextValue = msg.Code;
            switch (state)
            {
                case 0:
                    ContainerScan(msg.Code);
                    break;
                case 1:
                    PaletteScan(msg.Code);
                    break;
                case 2:
                    PressScan(msg.Code);
                    break;
            }

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