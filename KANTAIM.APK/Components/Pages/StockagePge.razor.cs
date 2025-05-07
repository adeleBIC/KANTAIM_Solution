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
        [Inject] public PressService _pressService { get; set; }
        [Inject] public ColorService _colorService { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] public DataProdService _dataProdService { get; set; }
        [Inject] public CellProductService _cellProductService { get; set; }
        [Inject] public CellService _cellService { get; set; }
        [Inject] public ScanService _scanService { get; set; }
        [Inject] public ActionService _actionService { get; set; }

        [Inject] ISnackbar _snackService { get; set; }

        [Parameter]
        public int Id { get; set; }
        [Parameter]
        public int Number { get; set; }

        public Product? Product { get; set; }
        //public Product? productModified { get; set; }
        public ProdColor? ColorOfProduct { get; set; }
        public bool _shipment { get; set; } = false;
        public bool Stock { get; set; } = false;
        private bool _goodStock { get; set; } = false;

        public bool MaintenanceBool { get; set; } = false; // pour indiquer si le contenaire est stock dans la zone maintenance
        //public ProdColor? colorOfProductModified { get; set; }
        //public string? CellValue { get; set; }
        //public string? ContainerValue { get; set; }

        public Container? ContainerScanner { get; set; }
        public Container? PaletteScanner { get; set; }
        public Log? LogRescent { get; set; }
        public DAL.Model.Cell? CellPropose { get; set; }

        public DAL.Model.Cell? CellScanner { get; set; }
        public int Fillstatus = 0;
        public Boolean IsPalette { get; set; } = false;
        public Container? BacScanner { get; set; }
        //public string? BacValue { get; set; }
        //public string ContainerFeedback { get; set; } = string.Empty;
        public int ActionStatus;
        public List<CellLog> CellList { get; set; }
       
        public Log? CellLog { get; set; }
        public Log? ContainerLog { get; set; }
        public Log? PaletteLog { get; set; }
        public Boolean ScanPallete { get; set; } = false;
        //public Boolean ShowBacList { get; set; } = false;

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

        protected override void OnInitialized()
        {
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
                if (ContainerScanner.ContainerType.NbrMaxContainer > 0) IsPalette = true;// S'il est palette, soit on fait stocker dans la cellule, soit on met bac dessus.

                ActionStatus = ContainerScanner.ContainerAction.Status;
                if (ActionStatus == 0 || ActionStatus == 2 || ActionStatus == 3 || ContainerScanner.FillStatus != StatusContainer.Undefinded)
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
                if (palette.InJail)
                {
                    bac.InJail = true;
                }
                _contenaireService.UpSert(bac);


                Log bacLog = new Log()
                {
                    EventTime = DateTime.Now,
                    Operation = palette.ActionID, // Initialisation pour le bac
                    ProductID = LogRescent.ProductID,
                    Press = LogRescent.Press,
                    PressID = LogRescent.PressID,
                    Shape = LogRescent.Shape,
                    ShapeID = LogRescent.ShapeID,
                    Container = bac,
                    ContainerID = bac.Id,
                    ProdColor = LogRescent.ProdColor,
                    ProdColorID = LogRescent.ProdColorID,
                    CellID = LogRescent.CellID,
                    FillStatus = LogRescent.FillStatus
                };
                _logService.UpSert(bacLog);
                _snackService.Add("Réussi !", Severity.Success);
            }
        }

        void FillstatusSelected(int fillstatu)
        {
            Fillstatus = fillstatu;
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
                
                if (LogRescent != null)
                {
                    Product = _productService.GetById((int)LogRescent.ProductID);
                    ColorOfProduct = _colorService.GetById(LogRescent.ProdColorID);
                }
                if(ContainerScanner.ContainerType.TypeNumber == 2)
                {
                    ScanPallete = true;
                }
                if(IsPalette && _contenaireService.CountBac(ContainerScanner.Id) < 24 )
                {
                    _snackService.Add("Le nombre de bac est inférieur que 24 !", Severity.Error);
                    //stock = false;
                    //return;
                }
                findCells();
                
            }
           
        }

        void findCells()
        {
            CellList = new List<CellLog>();
            List<Container> list = new List<Container>(_contenaireService.GetAll().Where(c => c.CellStock != null && c.ContainerType.IsContainable == false));
            foreach (Container container in list) // recherche tous les contenaire qui est stockés dans la cellule, find all the contenaire who in currutly in the cell
            {
                CellLog = _logService.GetByContenaireByOperationStatus(container.Id, OperationContainer.Store, OperationContainer.Transfer);
                //DAL.Model.Cell? cellAct = _cellService.GetById(cellLog?.CellID ?? 0);
                DAL.Model.Cell? cellAct = _cellService.GetByXY(CellLog.Cell.X, CellLog.Cell.Y);
                if (cellAct != null && cellAct.IsJail != true && cellAct.IsMaintenance != true && cellAct.ForEmpty != true && cellAct.IsPhantom != true && cellAct.Status != StatusCell.Full && _cellProductService.FindLink(cellAct.Id, Product.Id) && cellAct.Id != ContainerScanner?.CellId) { 
                    if (CellLog != null && CellLog.ProductID == Product.Id )
                    {
                        if (ColorOfProduct == null || CellLog.ProdColorID == ColorOfProduct.Id)
                        {
                            if (!CellList.Any(c => c.Cell.Id == container.CellStock.Id) )
                            {
                                CellList.Add(new CellLog { Cell = container.CellStock, EventTime = CellLog.EventTime });
                            }
                        }
                    }
                }  
            }
            if (CellList != null && CellList.Count > 0)
            {
                var oldestCellLog = CellList.OrderBy(c => c.EventTime).FirstOrDefault();
                if (oldestCellLog != null)
                {
                    CellPropose = oldestCellLog.Cell;
                }
            } else {
                List<DAL.Model.Cell> cells = new List<DAL.Model.Cell>();
                cells = _cellService.GetAll().Where(u => u.IsJail != true && u.IsMaintenance != true && u.ForEmpty != true && u.IsPhantom != true && u.Status == StatusCell.Empty).ToList();
                foreach (DAL.Model.Cell cell in cells)
                {
                    if (_cellProductService.FindLink(cell.Id, Product.Id) && cell.Id != ContainerScanner.CellId)
                    {

                        CellPropose = cell;
                        return;
                    }
                }
            }
        }

        void Maintenance()
        {
            MaintenanceBool = true;
        }

        void cellScan(string code)
        {
            string[] parts = _scanService.ParseCode(code);

            int.TryParse(parts[0], out int type);

            if (type != 4)
            {  
                _snackService.Add("Svp scannez le QR code de la cellule.", Severity.Error);  
            }
            else
            {
                int.TryParse(parts[1], out int X);
                int.TryParse(parts[2], out int Y);
                // int.TryParse(parts[3], out int typeCell);
                CellScanner = _cellService.GetAll().Where(u => u.X == X && u.Y == Y).FirstOrDefault();
                

                if (MaintenanceBool)
                {
                    if (CellScanner.IsMaintenance == true)
                    {

                        Exit(2);
                        _goodStock = true;
                    }
                    else
                    {
                        _snackService.Add("Svp scannez le QR code de la maintenance.", Severity.Error);
                    }

                }


                if (CellPropose != null && CellScanner.Id == CellPropose.Id)
                {
                    Exit(2);
                }
                else
                {
                    if (CellScanner.IsMaintenance)
                    {
                        _snackService.Add("Attention vous avez scanné le QR code de la maintenance.", Severity.Error);
                    }
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
                            if (CellScanner.IsJail)
                            {
                                _snackService.Add("Attention c'est pas une zone Fantôme. C'est la zone Prizon", Severity.Error);
                            } else if (CellScanner.IsMaintenance)
                            {
                                _snackService.Add("Attention c'est pas une zone Fantôme. C'est la zone Maintenance", Severity.Error);
                            } else if(!CellScanner.IsPhantom)
                            {
                                _snackService.Add("Attention c'est pas une zone Fantôme.", Severity.Error);
                                CellScanner = null;
                                return;
                            }

                        }
                        /*Verify the others products in this cell is the same type with the product in the contenaire we scan*/
                        if(CellScanner != null && Fillstatus > StatusContainer.Empty && !CellScanner.IsPhantom && !CellScanner.IsJail && !CellScanner.IsMaintenance)
                        {
                            foreach (Container container in CellScanner.Containers)
                            {
                                ContainerLog = _logService.GetByContenaireByOperationStatus(container.Id, OperationContainer.Store, OperationContainer.Transfer);
                                if (ContainerLog?.ProductID != Product.Id || ContainerLog.ProdColorID != ColorOfProduct.Id)
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

        }

        void upDateCellState()
        {
            if(CellScanner == null)
            {
                CellScanner = CellPropose;
            }
            if (_contenaireService.CountCells(CellScanner.Id) == 0)
            //if (_contenaireService.CountCellsXY(cellScanner) == 0)
            {
                CellScanner.Status = StatusCell.Empty;
            }
            else if (_contenaireService.CountCells(CellScanner.Id) < CellScanner.NbMax)
            //else if (_contenaireService.CountCellsXY(cellScanner) < cellScanner.NbMax)
            {
                CellScanner.Status = StatusCell.InFill;
            }
            else
            {
                CellScanner.Status = StatusCell.Full;
            }
            CellScanner.Products.Add(Product);
            _cellService.Upsert(CellScanner);
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
                    PaletteScanner = _contenaireService.GetContainerByNumber(PaletteNumber);
                    PaletteLog = _logService.GetByContenaireByOperationStatus(PaletteScanner.Id, OperationContainer.Store, OperationContainer.Transfer);
                    ContainerLog = _logService.GetByContenaireId(ContainerScanner.Id);
                    if (PaletteLog != null && PaletteLog.Product != null && PaletteLog.ProductID != ContainerLog.ProductID)
                    {
                        _snackService.Add("Scannez une Palette qui a le même produit que bac s'il vous plaît!", Severity.Error);
                        return;
                    }
                    Exit(2);
                }
                else
                {
                    _snackService.Add("Scannez un Palette s'il vous plaît!", Severity.Error);
                }
            }

        }

        void ModifierPaletteStatue(Log logPalette)
        {
            //PaletteScanner = _contenaireService.GetContainerByNumber(paletteNumber);
            //if (_contenaireService.CountBac(PaletteScanner.Id) == 0) // s'il n'y a plus de bac sur la palette
            //{
            //    PaletteScanner.ContainerAction = _actionService.GetByStatus(0);// Stocké Vide
            //    PaletteScanner.ActionID = PaletteScanner.ContainerAction.Id;
            //    PaletteScanner.FillStatus = StatusContainer.Empty;//Palette statut changé à vide
            //    _contenaireService.UpSert(PaletteScanner);
            //}else
            if (_contenaireService.CountBac(PaletteScanner.Id) == 1) // vide palette ajouter un bac
            {
                PaletteScanner.ContainerAction = _actionService.GetByStatus(5);// Stocké avec produit
                PaletteScanner.ActionID = ContainerScanner.ActionID;
                PaletteScanner.FillStatus = StatusContainer.HalfFull;//Palette statut changé à semi pleine
                PaletteScanner.CellStock = CellPropose;
                PaletteScanner.CellId = CellPropose.Id;
                if(CellPropose.IsJail)
                {
                    PaletteScanner.InJail = true;
                }
                _contenaireService.UpSert(PaletteScanner);
                logPalette.Container = PaletteScanner;
                logPalette.ContainerID = PaletteScanner.Id;
                _logService.UpSert(logPalette);
            }
        }

        void BacInPallete(int action)
        {
            ContainerScanner.ContainerAction = _actionService.GetByStatus(action);/*0: Stocké vide, store without product 2 : store with product 3 : Shipment*/
            ContainerScanner.ActionID = ContainerScanner.ContainerAction.Id;
            ContainerScanner.BigContainer = PaletteScanner;
            ContainerScanner.ContainerID = PaletteScanner.Id;
            ContainerScanner.FillStatus = Fillstatus;
            Log u;
            u = new Log()
            {
                EventTime = DateTime.Now,
                Operation = _shipment ? OperationContainer.Shipment : OperationContainer.Store,
                Product = LogRescent.Product,
                ProductID = LogRescent.ProductID,
                Press = LogRescent.Press,
                PressID = LogRescent.PressID,
                Shape = LogRescent.Shape,
                ShapeID = LogRescent.ShapeID,
                Container = ContainerScanner,
                ContainerID = ContainerScanner.Id,
                ProdColor = LogRescent.ProdColor,
                ProdColorID = LogRescent.ProdColorID,
                FillStatus = Fillstatus
            };
            if (PaletteLog != null)
            {
                u.Cell = PaletteLog.Cell;
                u.CellID = PaletteLog.CellID;
            }
            else
            {
                u.Cell = CellPropose;
                u.CellID = CellPropose.Id;
            }
            ContainerScanner.CellStock = u.Cell;
            ContainerScanner.CellId = u.CellID;
            if (CellScanner.IsJail)
            {
                ContainerScanner.InJail = true;
            }
            _contenaireService.UpSert(ContainerScanner);
            upDateCellState();
            _logService.UpSert(u);// indere log de bac
            ModifierPaletteStatue(u);
            _goodStock = true;
        }

        public List<Container> bacList { get; set; }

        async Task OpenDialogAsync()
        {
            if (IsPalette)
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
                if (ContainerScanner != null && ContainerScanner.ContainerType.TypeNumber == 2) // if it isn't an empty bac, we can replace this bac to correspond pallete 
                {
                    BacInPallete(action);
                    //_snackService.Add("Non vide bac stock dans une cellule est interdit !", Severity.Error);
                    //CellScanner = null;
                    return;
                }
                u = new Log()
                {
                    EventTime = DateTime.Now,
                    Operation = _shipment ? OperationContainer.Shipment : OperationContainer.Store,
                    Product = LogRescent.Product,
                    ProductID = LogRescent.ProductID,
                    Press = LogRescent.Press,
                    PressID = LogRescent.PressID,
                    Shape = LogRescent.Shape,
                    ShapeID = LogRescent.ShapeID,
                    Container = ContainerScanner,
                    ContainerID = ContainerScanner.Id,
                    ProdColor = LogRescent.ProdColor,
                    ProdColorID = LogRescent.ProdColorID,
                    FillStatus = Fillstatus
                };
                if(CellScanner != null)
                {
                    u.Cell = CellScanner;
                    u.CellID = CellScanner.Id;
                }
                //ContainerScanner.ActionID = action; /*2 : store with product 3 : Shipment*/
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
                
                //ContainerScanner.ActionID = 0; 
            }
           

            ContainerScanner.ContainerAction = _actionService.GetByStatus(action);/*0: Stocké vide, store without product 2 : store with product 3 : Shipment*/
            ContainerScanner.ActionID = ContainerScanner.ContainerAction.Id;
            if (CellScanner != null)
            {
                u.CellID = CellScanner.Id;
                CellScanner.Containers.Add(ContainerScanner); /*Add contenaire into the liste of cell's contenaires*/
            }

            if (ActionStatus == 0 || ActionStatus == 2 || ActionStatus == 3)
            {
                u.Operation = OperationContainer.Transfer; //  Transfer : Déplacment contenaire
            }
            _logService.UpSert(u);
            ContainerScanner.FillStatus = Fillstatus;
            ContainerScanner.CellId = u.CellID;
            if (CellScanner.IsJail)
            {
                ContainerScanner.InJail = true;
            }
            _contenaireService.UpSert(ContainerScanner);
            
            if (IsPalette)
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
                    if((ContainerScanner != null && ContainerScanner.InJail) || (PaletteScanner != null && PaletteScanner.InJail))
                    {
                        item.InJail = true;
                    }
                    if ((ContainerScanner != null && ContainerScanner.InMaintenance) || (PaletteScanner != null && PaletteScanner.InMaintenance))
                    {
                        item.InMaintenance = true;
                    }
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
                    if (IsPalette)
                    {
                        BacScan(msg.Code);
                    }
                    break;
                case 2:
                    if (ScanPallete)
                    {
                        PalleteScan(msg.Code);
                    }
                    else
                    {
                        cellScan(msg.Code);
                    }
                    break;
                case 3:
                    ContainerScan(msg.Code);
                    break;
                case 4:
                    if (ScanPallete)
                    {
                        PalleteScan(msg.Code);
                    }
                    else
                    {
                        cellScan(msg.Code);
                    }
                    break;
            }
            
            await InvokeAsync(StateHasChanged);
        }
    }
}

