using Android.Media;
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
            if (!ContainerScanner.ContainerType.IsContainable || ContainerScanner.FillStatus != 1)
            {
                if (ContainerScanner.ContainerType.NbrMaxContainer > 0) IsPallet = true;// S'il est palette, soit on fait stocker dans la cellule, soit on met bac dessus.

                int actionStatus = ContainerScanner.ContainerAction.Status;
                if (actionStatus == 0 || actionStatus == 2 || actionStatus == 3 || ContainerScanner.FillStatus != StatusContainer.Undefinded)
                {
                    FillstatusSelected(ContainerScanner.FillStatus);
                }
                else
                {
                    Product = _productService.GetById((int)ContainerScanner.ProductId);
                    ColorOfProduct = _colorService.GetById(ContainerScanner.ProdColorId);
                }
            }
            else
            {
                ContainerScanner = null;
                _snackService.Add("Svp scannez une palette ou un contenaire !", Severity.Error);
                NavigationManager.NavigateTo($"/");
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
            if (bac.ContainerID == palette.Id)
            {
                _snackService.Add("Déjà ajouté !", Severity.Error);
            } else
            {
                bac.CellId = palette.CellId;
                bac.ActionID = palette.ActionID;
                bac.FillStatus = palette.FillStatus;
                bac.Status = palette.Status;
                bac.InJail = palette.InJail;
                bac.InMaintenance = palette.InMaintenance;
                bac.Comment = palette.Comment;
                bac.ContainerID = palette.Id;
                bac.LastEvent = DateTime.Now;
                if (palette.InJail)
                {
                    bac.InJail = true;
                }
                _contenaireService.UpSert(bac);


                Log bacLog = new Log()
                {
                    EventTime = bac.LastEvent.Value,
                    Operation = palette.ActionID, // Initialisation pour le bac
                    ProductID = palette.ProductId,
                    Press = palette.Press,
                    PressID = palette.PressId,
                    Shape = palette.Press.Shape,
                    ShapeID = palette.Press.ShapeID,
                    Container = bac,
                    ContainerID = bac.Id,
                    ProdColor = palette.ProdColor,
                    ProdColorID = palette.ProdColorId,
                    CellID = palette.CellId,
                    FillStatus = StatusContainer.Full
                };
                _logService.UpSert(bacLog);
                _snackService.Add("Réussi !", Severity.Success);
            }
        }

        void FillstatusSelected(int fillstatus)
        {
            Fillstatus = fillstatus;
            List<DAL.Model.Cell> cells = new List<DAL.Model.Cell>();
            Stock = true;
            if(Fillstatus == StatusContainer.Empty) // vide
            {
                cells = _cellService.GetAll().Where(u => u.ForEmpty == true).ToList();
                if(ContainerScanner.ContainerType.IsContainable == true)
                {
                    cells = cells.FindAll(u => u.IsPhantom == true); // if it is a bac empty, we stock it in the cell phantom and empty
                } else
                {
                    cells = cells.FindAll(u => u.IsPhantom != true);// if it isn't a bac empty, we stock it in the cell empty
                }
                foreach (DAL.Model.Cell cell in cells)
                {
                    if (cell.Status != StatusCell.Full && cell.Id != ContainerScanner.CellId)
                    {
                        CellPropose = cell;
                    }       
                }
            } else // plein ou semi-plein
            {
                Product = ContainerScanner.Product;
                ColorOfProduct = ContainerScanner.ProdColor;

                if (!ContainerScanner.ContainerType.IsContainable && ContainerScanner.ContainerType.NbrMaxContainer > 0)
                {
                    ScanPallet = true;
                }

                if(IsPallet && _contenaireService.CountBac(ContainerScanner.Id) < 24 )
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
                                                                                c.ProductId == Product.Id &&
                                                                                !c.CellStock.IsJail &&
                                                                                !c.CellStock.IsMaintenance &&
                                                                                !c.CellStock.ForEmpty &&
                                                                                !c.CellStock.IsPhantom &&
                                                                                c.Status != StatusCell.Full &&
                                                                                c.Id != ContainerScanner?.CellId &&
                                                                                c.CellStock.RackCells != null &&
                                                                                c.CellStock.RackCells.Any(rc => profilSelected.RackProfils.Any(rp => rp.RackId == rc.RackId)))
                                                                                .OrderBy(c=>c.LastEvent));


            if (list != null && list.Count > 0) CellPropose = list.FirstOrDefault()?.CellStock ?? null;
            else CellPropose = _cellService.GetAll().Where(u => u.IsJail != true &&
                                                        u.IsMaintenance != true &&
                                                        u.ForEmpty != true &&
                                                        u.IsPhantom != true &&
                                                        u.Status == StatusCell.Empty &&
                                                        u.Id != ContainerScanner.CellId &&
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
                                    if (container.ProductId != Product.Id || container.ProdColorId != ColorOfProduct.Id)
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
            if(CellScanner == null) CellScanner = CellPropose;
            if (_contenaireService.CountCells(CellScanner.Id) == 0) CellScanner.Status = StatusCell.Empty;
            else if (_contenaireService.CountCells(CellScanner.Id) < CellScanner.NbMax) CellScanner.Status = StatusCell.InFill;
            else CellScanner.Status = StatusCell.Full;

            CellScanner.Products.Add(Product);
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
                    if (PalletScanner != null && PalletScanner.Product != null && PalletScanner.ProductId != ContainerScanner.ProductId)
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
            ContainerScanner.FillStatus = Fillstatus;
            Log u;
            u = new Log()
            {
                EventTime = DateTime.Now,
                Operation = _shipment ? OperationContainer.Shipment : OperationContainer.Store,
                Product = PalletScanner.Product,
                ProductID = PalletScanner.ProductId,
                Press = PalletScanner.Press,
                PressID = PalletScanner.PressId,
                Shape = PalletScanner.Press.Shape,
                ShapeID = PalletScanner.Press.ShapeID,
                Container = ContainerScanner,
                ContainerID = ContainerScanner.Id,
                ProdColor = PalletScanner.ProdColor,
                ProdColorID = PalletScanner.ProdColorId,
                FillStatus = Fillstatus,
                Cell = PalletScanner.CellStock,
                CellID = PalletScanner.CellId
            };
            //if (PaletteLog != null)
            //{
            //    u.Cell = PalletScanner.CellStock;
            //    u.CellID = PalletScanner.CellId;
            //}
            //else
            //{
            //    u.Cell = CellPropose;
            //    u.CellID = CellPropose.Id;
            //}

            ContainerScanner.CellStock = PalletScanner.CellStock;
            ContainerScanner.CellId = PalletScanner.CellId;
            if (CellScanner.IsJail)
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
                PalletScanner.CellId = CellPropose.Id;
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
            if (IsPallet)
            {
                bacList = _contenaireService.GetAll()
                            .Where(c => c.ContainerID == ContainerScanner.Id)
                            .ToList();
            }

            var parameters = new DialogParameters
        {
            { "BacList", bacList }
        };

            var options = new DialogOptions { CloseOnEscapeKey = true };

            await DialogService.ShowAsync<BacDialog>("Bacs associés à la palette", parameters, options);
        }

        void Exit(int action)
        {
            var oldCellule = ContainerScanner?.CellStock;
            if (action == 3)
                _shipment = true;
            if(ContainerScanner != null && CellScanner != null)
            {
                ContainerScanner.InMaintenance = CellScanner.IsMaintenance;
                ContainerScanner.InJail = CellScanner.IsJail;
            }
            Log u;
            if (Fillstatus != 1)
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
                    ProductID = ContainerScanner.ProductId,
                    Press = ContainerScanner.Press,
                    PressID = ContainerScanner.PressId,
                    Shape = ContainerScanner.Press.Shape,
                    ShapeID = ContainerScanner.Press.ShapeID,
                    Container = ContainerScanner,
                    ContainerID = ContainerScanner.Id,
                    ProdColor = ContainerScanner.ProdColor,
                    ProdColorID = ContainerScanner.ProdColorId,
                    FillStatus = Fillstatus
                };
                if(CellScanner != null)
                {
                    u.Cell = CellScanner;
                    u.CellID = CellScanner.Id;
                }
            } else
            {
                action = 0;/*0: Stocké vide, store without product*/
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

            ContainerScanner.ContainerAction = _actionService.GetByStatus(action);/*0: Stocké vide, store without product 2 : store with product 3 : Shipment*/
            ContainerScanner.ActionID = ContainerScanner.ContainerAction.Id;
            if (CellScanner != null)
            {
                u.CellID = CellScanner.Id;
                CellScanner.Containers.Add(ContainerScanner); /*Add contenaire into the liste of cell's contenaires*/
            }

            int actionStatus = ContainerScanner.ContainerAction.Status;
            if (actionStatus == 0 || actionStatus == 2 || actionStatus == 3)
            {
                u.Operation = OperationContainer.Transfer; //  Transfer : Déplacment contenaire
            }
            _logService.UpSert(u);

            ContainerScanner.FillStatus = Fillstatus;
            ContainerScanner.CellId = CellScanner.Id;
            if (CellScanner.IsJail) ContainerScanner.InJail = true;
            _contenaireService.UpSert(ContainerScanner);
            
            if (IsPallet)
            {
                foreach (var item in _contenaireService.GetAllBacs(ContainerScanner.Id))
                {
                    item.ContainerAction = _actionService.GetByStatus(action);/*0: Stocké vide, store without product 2 : store with product 3 : Shipment*/
                    item.ActionID = item.ContainerAction.Id;
                    if (CellScanner != null)
                    {
                        item.CellStock = CellScanner;
                        item.CellId = CellScanner.Id;
                    }
                    
                    item.FillStatus = Fillstatus; 
                    if((ContainerScanner != null && ContainerScanner.InJail) || (PalletScanner != null && PalletScanner.InJail)) item.InJail = true;
                    if ((ContainerScanner != null && ContainerScanner.InMaintenance) || (PalletScanner != null && PalletScanner.InMaintenance)) item.InMaintenance = true;
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
    }
}

