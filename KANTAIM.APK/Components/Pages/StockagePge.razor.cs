using Android.Media;
using Android.Service.Autofill;
using KANTAIM.APK.Components.Dialog;
using KANTAIM.APK.MessageBus.Messages;
using KANTAIM.APK.Resources;
using KANTAIM.APK.Services;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using static KANTAIM.APK.Components.Pages.FindProductPge;
using static System.Collections.Specialized.BitVector32;



namespace KANTAIM.APK.Components.Pages
{
    public partial class StockagePge : BasePage
    {
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] public ColorService _colorService { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] public CellProductService _cellProductService { get; set; }
        [Inject] public CellService _cellService { get; set; }
        [Inject] public ScanService _scanService { get; set; }
        [Inject] public ActionService _actionService { get; set; }
        [Inject] public ProfilSessionService _profilSessionService { get; set; }

        [Inject] ISnackbar _snackService { get; set; }

        [Parameter]
        public int Id { get; set; }
        [Parameter]
        public int Number { get; set; }

        public Product? Product { get; set; }
        public ProdColor? ColorOfProduct { get; set; }
        public bool _shipment { get; set; } = false;
        public bool Stock { get; set; } = false;
        private bool _goodStock { get; set; } = false;

        public bool MaintenanceBool { get; set; } = false; // pour indiquer si le contenaire est stock dans la zone maintenance
        public Container? ContainerScanner { get; set; }
        public Container? PalletScanner { get; set; }
        public DAL.Model.Cell? CellPropose { get; set; }
        public DAL.Model.Cell? CellScanner { get; set; }
        public int Fillstatus = 0;
        public Boolean IsPallet { get; set; } = false;
        public Container? BacScanner { get; set; }
        public Boolean ScanPallet { get; set; } = false;
        public Boolean InjectForBac { get; set; } = false;
        public string? TextValue { get; set; }

        private int state;

        public int State
        {
            get
            {
                state = 0;
                if (_goodStock) /*state = 1;*/{ _snackService.Add("Bien Stocké !", Severity.Success); NavigationManager.NavigateTo($"/"); }
                else if (MaintenanceBool) state = 2;
                else if (CellScanner != null) state = 3;
                else if (Stock) state = 4;
                else if (_shipment) /*state = 5;*/  { _snackService.Add("Bien sortie !", Severity.Success); NavigationManager.NavigateTo($"/"); }
                else state = 0;

                return state;
            }
        }

        private Profil profilSelected;

        protected override void OnInitialized()
        {
            profilSelected = _profilSessionService.CurrentProfil;
            switch (Id)
            {
                //Scanner le contenaire
                case 1:
                    ContainerManage(Number);
                    break;
                //Scanner le cell
                case 4:
                    CellScanner = _cellService.GetById(Number);
                    break;
            }
        }
        void ContainerScan(string code)
        {
            string[] parts = _scanService.ParseCode(code);
            if (parts != null)
            {
                int.TryParse(parts[0], out int type);
                int.TryParse(parts[1], out int containerNumber);
                int.TryParse(parts[2], out int ContenaireType);
                if (type == 1)
                {
                    ContainerManage(containerNumber);
                }
                else
                {
                    _snackService.Add("Svp scannez un bac !", Severity.Error);
                }
            }
        }


        void ContainerManage(int containerNumber)
        {
            ContainerScanner = _contenaireService.GetContainerByNumber(containerNumber);
            if (ContainerScanner.ContainerType.IsContainable && ContainerScanner.FillStatus != StatusContainer.Empty)// si il est bac sur pallete qui est aprés l'initialisation
            {
                InjectForBac = true;
            }
            else
            {
                if (ContainerScanner.ContainerType.NbrMaxContainer > 0)
                {
                    bacList = _contenaireService.GetAll()
                                .Where(c => c.ContainerID == ContainerScanner.Id)
                                .ToList();
                    IsPallet = true;// S'il est palette, soit on fait stocker dans la cellule, soit on met bac dessus.
                }
                int actionStatus = ContainerScanner.ContainerAction.Status;
                if (actionStatus == 0 || actionStatus == 2 || actionStatus == 3 || ContainerScanner.FillStatus != StatusContainer.Undefinded)
                {
                    FillstatusSelected(ContainerScanner.FillStatus);
                }
                else
                {
                    Product = _productService.GetById((int)ContainerScanner.ProductID);
                    ColorOfProduct = _colorService.GetById(ContainerScanner.ProdColorID);
                }
            }


        }

        void BacScan(string code)
        {
            string[] parts = _scanService.ParseCode(code);
            if (parts != null)
            {
                int.TryParse(parts[0], out int type);
                int.TryParse(parts[1], out int BacNumber);
                //int.TryParse(parts[2], out int ContenaireType);
                if (type == 1)
                {
                    BacScanner = _contenaireService.GetContainerByNumber(BacNumber);
                    if (BacScanner.ContainerType.IsContainable)
                    {
                        TransferBacToPalette(BacScanner, ContainerScanner);
                    }
                    else
                    {
                        _snackService.Add("Scannez un bac s'il vous plaît!", Severity.Error);
                    }
                }
                else
                {
                    _snackService.Add("Scannez un bac s'il vous plaît!", Severity.Error);
                }
            }
        }

        void TransferBacToPalette(Container bac, Container palette)
        {
            if (bac.ContainerID == palette.Id || bac.FillStatus > StatusContainer.Empty)
            {
                _snackService.Add("Bac déjà présent dans une palette ou non vide !", Severity.Error);
            }
            else
            {
                bac.CellID = palette.CellID;
                bac.ActionID = palette.ActionID;
                bac.FillStatus = StatusContainer.Full;
                bac.Status = palette.Status;
                bac.InJail = palette.InJail;
                bac.InMaintenance = palette.InMaintenance;
                bac.Comment = palette.Comment;
                bac.ContainerID = palette.Id;
                bac.LastEvent = DateTime.Now;
                bac.Product = palette.Product;
                bac.ProductID = palette.ProductID;
                bac.ProdColor = palette.ProdColor;
                bac.ProdColorID = palette.ProdColorID;
                bac.Press = palette.Press;
                bac.PressID = palette.PressID;
                if (palette.InJail)
                {
                    bac.InJail = true;
                }
                _contenaireService.UpSert(bac);


                Log bacLog = new Log()
                {
                    EventTime = bac.LastEvent.Value,
                    Operation = palette.ActionID, // Initialisation pour le bac
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
                    CellID = palette.CellID,
                    FillStatus = StatusContainer.Full
                };
                _logService.UpSert(bacLog);
                _snackService.Add("Réussi !", Severity.Success);
                bacList = _contenaireService.GetAll()
                                            .Where(c => c.ContainerID == ContainerScanner.Id)
                                            .ToList();
            }
        }

        void FillstatusSelected(int fillstatus)
        {
            Fillstatus = fillstatus;
            List<DAL.Model.Cell> cells = new List<DAL.Model.Cell>();
            Stock = true;
            if (Fillstatus == StatusContainer.Empty) // vide
            {
                cells = _cellService.GetAll().Where(u => u.ForEmpty == true).ToList();
                if (ContainerScanner.ContainerType.IsContainable == true)
                {
                    cells = cells.FindAll(u => u.IsPhantom == true); // if it is a bac empty, we stock it in the cell phantom and empty
                }
                else
                {
                    cells = cells.FindAll(u => u.IsPhantom != true);// if it isn't a bac empty, we stock it in the cell empty
                }
                foreach (DAL.Model.Cell cell in cells)
                {
                    if (cell.Status != StatusCell.Full && cell.Id != ContainerScanner.CellID)
                    {
                        CellPropose = cell;
                    }
                }
            }
            else // plein ou semi-plein
            {
                Product = ContainerScanner.Product;
                ColorOfProduct = ContainerScanner.ProdColor;

                if (ContainerScanner.ContainerType.IsContainable)
                {
                    ScanPallet = true;
                }

                if (IsPallet && _contenaireService.CountBac(ContainerScanner.Id) < 24)
                {
                    _snackService.Add("Le nombre de bac est inférieur que 24 !", Severity.Error);
                }
                findCells();

            }

        }

        void findCells()
        {
            List<Container> list = new List<Container>(_contenaireService.GetAllByOperationStatus(ActionStatus.Store)
                                                                                .Where(c => c.CellStock != null &&
                                                                                c.ContainerType.IsContainable == false &&
                                                                                c.ProductID == Product.Id &&
                                                                                c.ProdColorID == ColorOfProduct.Id &&
                                                                                !c.CellStock.IsJail &&
                                                                                !c.CellStock.IsMaintenance &&
                                                                                !c.CellStock.ForEmpty &&
                                                                                !c.CellStock.IsPhantom &&
                                                                                c.CellStock.Status != StatusCell.Full &&
                                                                                c.CellID != ContainerScanner?.CellID &&
                                                                                c.CellStock.RackCells != null &&
                                                                                c.CellStock.RackCells.Any(rc => profilSelected.RackProfils.Any(rp => rp.RackId == rc.RackId)))
                                                                                .OrderBy(c => c.LastEvent));


            if (list != null && list.Count > 0) CellPropose = list.FirstOrDefault()?.CellStock ?? null;
            else CellPropose = _cellService.GetAll().Where(u => u.IsJail != true &&
                                                        u.IsMaintenance != true &&
                                                        u.ForEmpty != true &&
                                                        u.IsPhantom != true &&
                                                        u.Status == StatusCell.Empty &&
                                                        u.Id != ContainerScanner.CellID &&
                                                        _cellProductService.FindLink(u.Id, Product.Id) &&
                                                        u.RackCells.Any(rc => profilSelected.RackProfils.Any(rp => rp.RackId == rc.RackId))).FirstOrDefault();
        }

        void Maintenance()
        {
            MaintenanceBool = true;
        }

        void cellScan(string code)
        {
            string[] parts = _scanService.ParseCode(code);
            int.TryParse(parts[0], out int type);

            if (type != 4) _snackService.Add("Svp scannez le QR code de la cellule.", Severity.Error);
            else
            {
                int.TryParse(parts[1], out int X);
                int.TryParse(parts[2], out int Y);
                // int.TryParse(parts[3], out int typeCell);
                CellScanner = _cellService.GetAll().Where(u => u.X == X && u.Y == Y).FirstOrDefault();

                if (CellScanner != null)
                {
                    if (MaintenanceBool)
                    {
                        if (CellScanner.IsMaintenance == true)
                        {
                            Exit(2);
                            _goodStock = true;
                        }
                        else _snackService.Add("Svp scannez le QR code de la maintenance.", Severity.Error);
                    }

                    if (CellPropose != null && CellScanner.Id == CellPropose.Id) Exit(2);
                    else
                    {
                        if (CellScanner.IsMaintenance) _snackService.Add("Attention vous avez scanné le QR code de la maintenance.", Severity.Error);
                        else
                        {
                            if (CellScanner.Status == StatusCell.Full)
                            {
                                _snackService.Add("Attention la cellule est pleine.", Severity.Error);
                                CellScanner = null;
                                return;
                            }
                            if (CellPropose == null)
                            {
                                if (CellScanner.IsJail) _snackService.Add("Attention c'est pas une zone Fantôme. C'est la zone Prizon", Severity.Error);
                                else if (CellScanner.IsMaintenance) _snackService.Add("Attention c'est pas une zone Fantôme. C'est la zone Maintenance", Severity.Error);
                                else if (!CellScanner.IsPhantom)
                                {
                                    _snackService.Add("Attention c'est pas une zone Fantôme.", Severity.Error);
                                    CellScanner = null;
                                    return;
                                }
                            }
                            /*Verify the others products in this cell is the same type with the product in the contenaire we scan*/
                            if (CellScanner != null && Fillstatus > StatusContainer.Empty && !CellScanner.IsPhantom && !CellScanner.IsJail && !CellScanner.IsMaintenance)
                            {
                                foreach (Container container in CellScanner.Containers)
                                {
                                    if (container.ProductID != Product.Id || container.ProdColorID != ColorOfProduct.Id)
                                    {
                                        _snackService.Add("Attention le produit déjà stocké n'est pas identique.", Severity.Error);
                                        CellScanner = null;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                else _snackService.Add("Attention Mauvais QRCode scanné, merci de scanner un cellule valide.", Severity.Error);

            }

        }

        void upDateCellState()
        {
            if (CellScanner == null) CellScanner = CellPropose;
            if (_contenaireService.CountCells(CellScanner.Id) == 0) CellScanner.Status = StatusCell.Empty;
            else if (_contenaireService.CountCells(CellScanner.Id) < CellScanner.NbMax) CellScanner.Status = StatusCell.InFill;
            else CellScanner.Status = StatusCell.Full;

            if (Product != null) CellScanner.Products.Add(Product);
            _cellService.Upsert(CellScanner);
        }

        void upDateCellState(DAL.Model.Cell cell)
        {
            if (cell != null)
            {
                if (_contenaireService.CountCells(cell.Id) == 0) cell.Status = StatusCell.Empty;
                else cell.Status = StatusCell.InFill;

                _cellService.Upsert(cell);
            }
        }

        void PalleteScan(string code)
        {
            string[] parts = _scanService.ParseCode(code);
            if (parts != null || parts.Length != 3)
            {
                int.TryParse(parts[0], out int type);
                int.TryParse(parts[1], out int PaletteNumber);
                int.TryParse(parts[2], out int ContenaireType);
                if (type == 1 && ContenaireType == 3)
                {
                    PalletScanner = _contenaireService.GetContainerByNumber(PaletteNumber);
                    if (PalletScanner != null && PalletScanner.Product != null && PalletScanner.ProductID != ContainerScanner.ProductID)
                    {
                        _snackService.Add("Scannez une Palette qui a le même produit que bac s'il vous plaît!", Severity.Error);
                        return;
                    }
                    Exit(2);
                }
                else _snackService.Add("Scannez une Palette s'il vous plaît!", Severity.Error);
            }

        }

        void BacInPallete(int action)
        {
            ContainerScanner.ContainerAction = _actionService.GetByStatus(action);/*0: Stocké vide, store without product 2 : store with product 3 : Shipment*/
            ContainerScanner.ActionID = ContainerScanner.ContainerAction.Id;
            ContainerScanner.BigContainer = PalletScanner;
            ContainerScanner.ContainerID = PalletScanner.Id;
            //ContainerScanner.FillStatus = Fillstatus;
            ContainerScanner.Product = PalletScanner.Product;
            ContainerScanner.ProductID = PalletScanner.ProductID;
            ContainerScanner.ProdColor = PalletScanner.ProdColor;
            ContainerScanner.ProdColorID = PalletScanner.ProdColorID;
            ContainerScanner.Press = PalletScanner.Press;
            ContainerScanner.PressID = PalletScanner.PressID;
            ContainerScanner.Machine = null;
            ContainerScanner.MachineID = null;

            Log u;
            u = new Log()
            {
                EventTime = DateTime.Now,
                Operation = _shipment ? OperationContainer.Shipment : OperationContainer.Store,
                Product = PalletScanner.Product,
                ProductID = PalletScanner.ProductID,
                Press = PalletScanner.Press,
                PressID = PalletScanner.PressID,
                Shape = PalletScanner.Press?.Shape,
                ShapeID = PalletScanner.Press?.ShapeID,
                Container = ContainerScanner,
                ContainerID = ContainerScanner.Id,
                ProdColor = PalletScanner.ProdColor,
                ProdColorID = PalletScanner.ProdColorID,
                FillStatus = Fillstatus,
                Cell = PalletScanner.CellStock,
                CellID = PalletScanner.CellID
            };

            ContainerScanner.LastEvent = u.EventTime;
            ContainerScanner.CellStock = PalletScanner.CellStock;
            ContainerScanner.CellID = PalletScanner.CellID;
            if (CellScanner?.IsJail ?? false)
            {
                ContainerScanner.InJail = true;
            }
            _contenaireService.UpSert(ContainerScanner);
            upDateCellState();
            _logService.UpSert(u);
            ModifyPalletStatue(u);
            _goodStock = true;
        }

        void ModifyPalletStatue(Log logPalette)
        {
            if (_contenaireService.CountBac(PalletScanner.Id) >= 1) // vide palette ajouter un bac
            {
                PalletScanner.ContainerAction = _actionService.GetByStatus(ActionStatus.Store);// Stocké avec produit
                PalletScanner.ActionID = ContainerScanner.ActionID;
                PalletScanner.FillStatus = StatusContainer.HalfFull;//Palette statut changé à semi pleine
                PalletScanner.CellStock = CellPropose;
                PalletScanner.CellID = CellPropose.Id;
                if (CellPropose.IsJail) PalletScanner.InJail = true;
                _contenaireService.UpSert(PalletScanner);

                logPalette.Container = PalletScanner;
                logPalette.ContainerID = PalletScanner.Id;
                _logService.UpSert(logPalette);
            }
        }

        public List<Container> bacList { get; set; }

        async Task OpenDialogAsync()
        {
            var parameters = new DialogParameters
            {
                { "BacList", bacList }
            };

            var options = new DialogOptions { CloseOnEscapeKey = true };

            await DialogService.ShowAsync<BacDialog>("Bacs associés à la palette", parameters, options);
        }

        void SortieBacDirect()
        {
            _shipment = true;
            ContainerScanner.BigContainer = null;
            ContainerScanner.ContainerID = null;
            ContainerScanner.ContainerAction = _actionService.GetByStatus(OperationContainer.Shipment);
            ContainerScanner.ActionID = ContainerScanner.ContainerAction.Id;
            Log u = new Log()
            {
                EventTime = DateTime.Now,
                Operation = OperationContainer.Shipment,
                Product = ContainerScanner.Product,
                ProductID = ContainerScanner.ProductID,
                Press = ContainerScanner.Press,
                PressID = ContainerScanner.PressID,
                Shape = ContainerScanner.Press?.Shape,
                ShapeID = ContainerScanner.Press?.ShapeID,
                Machine = ContainerScanner.Machine,
                MachineID = ContainerScanner.MachineID,
                Container = ContainerScanner,
                ContainerID = ContainerScanner.Id,
                ProdColor = ContainerScanner.ProdColor,
                ProdColorID = ContainerScanner.ProdColorID,
                FillStatus = ContainerScanner.FillStatus
            };
            _logService.UpSert(u);
            _contenaireService.UpSert(ContainerScanner);
            _snackService.Add("Bien sortie !", Severity.Success);
            NavigationManager.NavigateTo($"/");
        }

        void Exit(int action)
        {
            var oldCellule = ContainerScanner?.CellStock;
            if (action == 3)
                _shipment = true;
            if (ContainerScanner != null && CellScanner != null)
            {
                ContainerScanner.InMaintenance = CellScanner.IsMaintenance;
                ContainerScanner.InJail = CellScanner.IsJail;
            }
            Log u;
            if (Fillstatus != StatusContainer.Empty)
            {//if container is not empty
                if (ContainerScanner != null && ContainerScanner.ContainerType.IsContainable) // if it isn't an empty bac, we can replace this bac to correspond pallete 
                {
                    BacInPallete(action);
                    return;
                }
                u = new Log()
                {
                    EventTime = DateTime.Now,
                    Operation = _shipment ? OperationContainer.Shipment : OperationContainer.Store,
                    Product = ContainerScanner.Product,
                    ProductID = ContainerScanner.ProductID,
                    Press = ContainerScanner.Press,
                    PressID = ContainerScanner.PressID,
                    Shape = ContainerScanner.Press?.Shape,
                    ShapeID = ContainerScanner.Press?.ShapeID,
                    Machine = ContainerScanner.Machine,
                    MachineID = ContainerScanner.MachineID,
                    Container = ContainerScanner,
                    ContainerID = ContainerScanner.Id,
                    ProdColor = ContainerScanner.ProdColor,
                    ProdColorID = ContainerScanner.ProdColorID,
                    FillStatus = Fillstatus
                };
                if (CellScanner != null)
                {
                    u.Cell = CellScanner;
                    u.CellID = CellScanner.Id;
                }
            }
            else
            {
                action = 0;/*0: Stocké vide, store without product*/
                ContainerScanner.Press = null;
                ContainerScanner.PressID = null;
                ContainerScanner.Product = null;
                ContainerScanner.ProductID = null;
                ContainerScanner.ProdColor = null;
                ContainerScanner.ProdColorID = null;
                ContainerScanner.Machine = null;
                ContainerScanner.MachineID = null;

                u = new Log()
                {
                    EventTime = DateTime.Now,
                    Operation = OperationContainer.Store, // store without product , operation : store
                    Container = ContainerScanner,
                    ContainerID = ContainerScanner.Id,
                    Cell = CellScanner,
                    CellID = CellScanner.Id,
                    FillStatus = Fillstatus
                };
            }

            int actionStatus = ContainerScanner.ContainerAction.Status;
            if (actionStatus == 0 || actionStatus == 2 || actionStatus == 3)
            {
                u.Operation = OperationContainer.Transfer; //  Transfer : Déplacment contenaire
            }
            _logService.UpSert(u);

            ContainerScanner.ContainerAction = _actionService.GetByStatus(action);/*0: Stocké vide, store without product 2 : store with product 3 : Shipment*/
            ContainerScanner.ActionID = ContainerScanner.ContainerAction.Id;
            if (CellScanner != null)
            {
                u.CellID = CellScanner.Id;
                CellScanner.Containers.Add(ContainerScanner); /*Add contenaire into the liste of cell's contenaires*/
            }

            ContainerScanner.LastEvent = u.EventTime;
            ContainerScanner.FillStatus = Fillstatus;
            ContainerScanner.CellID = CellScanner.Id;
            if (CellScanner.IsJail) ContainerScanner.InJail = true;
            _contenaireService.UpSert(ContainerScanner);

            if (IsPallet)
            {
                foreach (var item in _contenaireService.GetAllBacs(ContainerScanner.Id))
                {
                    if (Fillstatus == StatusContainer.Empty)
                    {
                        item.LastEvent = u.EventTime;
                        item.ContainerAction = _actionService.GetByStatus(3);// Sortie stock
                        item.ActionID = item.ContainerAction.Id;
                        item.CellStock = null;
                        item.CellID = null;
                        item.BigContainer = null;
                        item.ContainerID = null;
                    }
                    else
                    {
                        item.ContainerAction = _actionService.GetByStatus(action);/*0: Stocké vide, store without product 2 : store with product 3 : Shipment*/
                        item.ActionID = item.ContainerAction.Id;
                        if (CellScanner != null)
                        {
                            item.CellStock = CellScanner;
                            item.CellID = CellScanner.Id;
                        }
                        if ((ContainerScanner != null && ContainerScanner.InJail) || (PalletScanner != null && PalletScanner.InJail)) item.InJail = true;
                        if ((ContainerScanner != null && ContainerScanner.InMaintenance) || (PalletScanner != null && PalletScanner.InMaintenance)) item.InMaintenance = true;
                    }
                    //item.FillStatus = Fillstatus; 

                    _contenaireService.UpSert(item);
                }
            }
            upDateCellState();
            upDateCellState(oldCellule);
            _goodStock = true;
        }

        public override async void OnMessageReceived(InputMessage msg)
        {
            TextValue = msg.Code;
            switch (state)
            {
                case 0:
                    if (IsPallet) BacScan(msg.Code);
                    break;
                case 2:
                    if (ScanPallet) PalleteScan(msg.Code);
                    else cellScan(msg.Code);
                    break;
                case 3:
                    ContainerScan(msg.Code);
                    break;
                case 4:
                    if (ScanPallet) PalleteScan(msg.Code);
                    else cellScan(msg.Code);
                    break;
            }

            await InvokeAsync(StateHasChanged);
        }

        void Return()
        {
            if (ContainerScanner.ContainerAction.Status != 2)
            {
                Stock = false;
                ScanPallet = false;
                Fillstatus = 0;
            }
            else
            {
                NavigationManager.NavigateTo($"/");
            }
        }

        void PalletEmpty()
        {

        }
    }
}

