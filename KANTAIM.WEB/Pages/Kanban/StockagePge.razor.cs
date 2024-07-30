using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Ressources;
using KANTAIM.WEB.Services;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using static KANTAIM.WEB.Pages.Kanban.FindProductPge;
using static System.Collections.Specialized.BitVector32;



namespace KANTAIM.WEB.Pages.Kanban
{
    public partial class StockagePge
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

        [Inject] ISnackbar _snackService { get; set; }

        [Parameter]
        public int Id { get; set; }
        [Parameter]
        public int Number { get; set; }

        public Product? product { get; set; }
        //public Product? productModified { get; set; }
        public ProdColor? colorOfProduct { get; set; }
        public bool shipment { get; set; } = false;
        public bool stock { get; set; } = false;
        public bool bienStock { get; set; } = false;
        public bool maintenance { get; set; } = false; // pour indiquer si le contenaire est stock dans la zone maintenance
        //public ProdColor? colorOfProductModified { get; set; }
        public string? CellValue { get; set; }
        public string? ContainerValue { get; set; }

        public Container? ContainerScanner { get; set; }
        public Log? logRescent { get; set; }
        public Cell? cellPropose { get; set; }

        public Cell? cellScanner { get; set; }
        public int fillstatus = 0;

        public Boolean isPalette { get; set; } = false;
        public Container? BacScanner { get; set; }
        public string? BacValue { get; set; }
        public string ContainerFeedback { get; set; } = string.Empty;
        public int actionId;
        public List<CellLog> cellList { get; set; }
        public Log? cellLog { get; set; }
        public Log? containerLog { get; set; }
        
        public string? TextValue { get; set; }

        private static StockagePge _instance;
        private string currentUrl;
        private string pageUrl;
        private int state;

        [JSInvokable]
        public static void CaptureInputStock(string input)
        {
            _instance?.HandleInput(input);
        }
        private async void HandleInput(string input)
        {
            if(currentUrl == pageUrl)
            {
                if (input == "Enter" )
                {
                    switch (state)
                    {
                        case 0: 
                            if(isPalette)
                            {
                                BacScan(TextValue);
                            }
                            break;
                        case 2:
                            cellScan(TextValue);
                            break;
                        case 3:
                            ContainerScan(TextValue);
                            break;
                        case 4:
                            cellScan(TextValue);
                            break;
                    }

                    await InvokeAsync(StateHasChanged);
                    TextValue = null;

                }
                else
                {
                    TextValue += input;
                    await InvokeAsync(StateHasChanged);

                }
            }
            
        }

        public int State
        {
            get
            {
                state = 0;
                if (bienStock) /*state = 1;*/{ _snackService.Add("Bien Stocké !", Severity.Success); NavigationManager.NavigateTo($"/ScannerPge"); }
                else if (maintenance) state = 2;
                else if (cellScanner != null) state = 3;
                else if (stock) state = 4;
                else if (shipment) /*state = 5;*/  { _snackService.Add("Bien sortie !", Severity.Success); NavigationManager.NavigateTo($"/ScannerPge"); }
                else state = 0;

                return state;
            }
        }
        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            // Mettre à jour l'URL actuelle lorsque l'URL change
            currentUrl = e.Location;
            // Vous pouvez ajouter ici toute logique que vous souhaitez exécuter lorsque l'URL change
        }

        public void Dispose()
        {
            // Se désabonner de l'événement pour éviter les fuites de mémoire
            NavigationManager.LocationChanged -= OnLocationChanged;
        }
        protected override void OnInitialized()
        {
            currentUrl = NavigationManager.Uri;
            pageUrl = NavigationManager.Uri;
            _instance = this;
            switch (Id)
            {
                //Scanner le contenaire
                case 1:
                    ContainerManage(Number);
                    break;
                //Scanner le cell
                case 4:
                    cellScanner = _cellService.GetById(Number);
                    break;
            }
            NavigationManager.LocationChanged += OnLocationChanged;
        }
        void ContainerScan(string code)
        {
            string[] parts = _scanService.scanCode(code);
            if (parts != null)
            {
                int.TryParse(parts[0], out int type);
                int.TryParse(parts[1], out int containerNumber);
                //int.TryParse(parts[2], out int ContenaireType);
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
            ContainerScanner = _contenaireService.GetContainerByNumber(containerNumber).FirstOrDefault();
            if (ContainerScanner.ContainerType.TypeNumber != 2 || ContainerScanner.FillStatus != 1)
            {
                if (ContainerScanner.ContainerType.TypeNumber == 3) // S'il est palette, soit on fait stocker dans la cellule, soit on met bac dessus.
                {
                    isPalette = true;
                }
                logRescent = _logService.GetByContenaireId(ContainerScanner.Id);
                actionId = ContainerScanner.ActionID;
                if (actionId == 0 || actionId == 2 || actionId == 3)
                {
                    FillstatusSelected(ContainerScanner.FillStatus);
                }
                else
                {
                    if (logRescent != null)
                    {
                        product = _productService.GetById((int)logRescent.ProductID);
                        colorOfProduct = _colorService.GetById(logRescent.ProdColorID);
                    }
                }
            }
            else
            {
                ContainerScanner = null;
                _snackService.Add("Svp scannez une palette ou un contenaire !", Severity.Error);
                NavigationManager.NavigateTo($"/ScannerPge");
            }
        }

        void BacScan(string code)
        {
            string[] parts = _scanService.scanCode(code);
            if (parts != null)
            {
                int.TryParse(parts[0], out int type);
                int.TryParse(parts[1], out int BacNumber);
                //int.TryParse(parts[2], out int ContenaireType);
                if (type == 1)
                {
                    BacScanner = _contenaireService.GetContainerByNumber(BacNumber).FirstOrDefault();
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
                _contenaireService.UpSert(bac);


                Log bacLog = new Log()
                {
                    EventTime = DateTime.Now,
                    Operation = palette.ActionID, // Initialisation pour le bac
                    ProductID = logRescent.ProductID,
                    Press = logRescent.Press,
                    PressID = logRescent.PressID,
                    Shape = logRescent.Shape,
                    ShapeID = logRescent.ShapeID,
                    Container = bac,
                    ContainerID = bac.Id,
                    ProdColor = logRescent.ProdColor,
                    ProdColorID = logRescent.ProdColorID,
                    CellID = logRescent.CellID,
                    FillStatus = logRescent.FillStatus
                };
                _logService.UpSert(bacLog);
                _snackService.Add("Réussi !", Severity.Success);
            }
        }

        void FillstatusSelected(int fillstatu)
        {
            fillstatus = fillstatu;
            List<Cell> cells = new List<Cell>();
            stock = true;
            if(fillstatus == 1) // vide
            {
                cells = _cellService.GetAll().Where(u => u.ForEmpty == true).ToList();
                if(ContainerScanner.ContainerType.IsContainable == true)
                {
                    cells = cells.FindAll(u => u.IsPhantom == true); // if it is a bac empty, we stock it in the cell phantom and empty
                } else
                {
                    cells = cells.FindAll(u => u.IsPhantom != true);// if it isn't a bac empty, we stock it in the cell empty
                }
                foreach (Cell cell in cells)
                {
                    if (cell.Status != 2 && cell.Id != ContainerScanner.CellId)
                    {
                        cellPropose = cell;
                        
                    }       
                }
            } else // plein ou semi-plein
            {
                
                if (logRescent != null)
                {
                    product = _productService.GetById((int)logRescent.ProductID);
                    colorOfProduct = _colorService.GetById(logRescent.ProdColorID);
                }
                
                if(isPalette && _contenaireService.CountBac(ContainerScanner.Id) < 24 )
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
            cellList = new List<CellLog>();
            foreach (Container container in _contenaireService.Cache.Where(c => c.CellStock != null && c.ContainerType.IsContainable == false)) // recherche tous les contenaire qui est stockés dans la cellule, find all the contenaire who in currutly in the cell
            {
                cellLog = _logService.GetByContenaireByActionId(container.Id, 2);
                Cell? cellAct = _cellService.GetById(cellLog?.CellID ?? 0);
                if(cellAct != null && cellAct.IsJail != true && cellAct.IsMaintenance != true && cellAct.ForEmpty != true && cellAct.IsPhantom != true && cellAct.Status != 2 && _cellProductService.FindLink(cellAct.Id, product.Id) && cellAct.Id != ContainerScanner?.CellId) { 
                    if (cellLog != null && cellLog.ProductID == product.Id )
                    {
                        if (colorOfProduct == null || cellLog.ProdColorID == colorOfProduct.Id)
                        {
                            if (!cellList.Any(c => c.Cell.Id == container.CellStock.Id) )
                            {
                                cellList.Add(new CellLog { Cell = container.CellStock, EventTime = cellLog.EventTime });
                            }
                        }
                    }
                }  
            }
            if (cellList != null && cellList.Count > 0)
            {
                var oldestCellLog = cellList.OrderBy(c => c.EventTime).FirstOrDefault();
                if (oldestCellLog != null)
                {
                    cellPropose = oldestCellLog.Cell;
                }
            } else {
                List<Cell> cells = new List<Cell>();
                cells = _cellService.GetAll().Where(u => u.IsJail != true && u.IsMaintenance != true && u.ForEmpty != true && u.IsPhantom != true && u.Status == 0).ToList();
                foreach (Cell cell in cells)
                {
                    if (_cellProductService.FindLink(cell.Id, product.Id) && cell.Id != ContainerScanner.CellId)
                    {

                        cellPropose = cell;
                        return;
                    }
                }
            }
        }

        void Maintenance()
        {
            maintenance = true;
        }

        void cellScan(string code)
        {
            string[] parts = _scanService.scanCode(code);

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
                cellScanner = _cellService.GetAll().Where(u => u.X == X && u.Y == Y).FirstOrDefault();
                

                if (maintenance)
                {
                    if (cellScanner.IsMaintenance == true)
                    {
                        Exit(2);
                        bienStock = true;
                    }
                    else
                    {
                        _snackService.Add("Svp scannez le QR code de la maintenance.", Severity.Error);
                    }

                }


                if (cellScanner.Id == cellPropose.Id)
                {
                    Exit(2);
                }
                else
                {
                    if (cellScanner.IsMaintenance)
                    {
                        _snackService.Add("Attention vous avez scanné le QR code de la maintenance.", Severity.Success);
                    }
                    else
                    {
                        if (cellScanner.Status == 2) 
                        {
                            _snackService.Add("Attention la cellule est pleine.", Severity.Error);
                            cellScanner = null;
                            return;
                        }
                        if (cellPropose == null)
                        {
                            if (!cellScanner.IsPhantom)
                            {
                                _snackService.Add("Attention c'est pas une zone Fantôme.", Severity.Error);
                                cellScanner = null;
                                return;
                            }

                        }
                        /*Verify the others products in this cell is the same type with the product in the contenaire we scan*/
                        if(cellScanner != null && fillstatus > 1)
                        {
                            foreach (Container container in cellScanner.Containers)
                            {
                                containerLog = _logService.GetByContenaireByActionId(container.Id, 2);
                                if (containerLog?.ProductID != product.Id || containerLog.ProdColorID != colorOfProduct.Id)
                                {
                                    _snackService.Add("Attention le produit déjà stocké n'est pas identique.", Severity.Error);
                                    cellScanner = null;
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
            if (_contenaireService.CountCells(cellScanner.Id) == 0)
            {
                cellScanner.Status = 0;
            }
            else if (_contenaireService.CountCells(cellScanner.Id) < cellScanner.NbMax)
            {
                cellScanner.Status = 1;
            }
            else
            {
                cellScanner.Status = 2;
            }
            _cellService.Upsert(cellScanner);
        }

        void Exit(int action)
        {
            if(action == 3)
                shipment = true;
            Log u;
            if (fillstatus != 1)
            {//if container is not empty
                if (ContainerScanner != null && ContainerScanner.ContainerType.TypeNumber == 2)
                {
                    _snackService.Add("Non vide bac stock dans une cellule est interdit !", Severity.Error);
                    cellScanner = null;
                    return;
                }
                u = new Log()
                {
                    EventTime = DateTime.Now,
                    Operation = shipment ? 3 : 2,
                    Product = logRescent.Product,
                    ProductID = logRescent.ProductID,
                    Press = logRescent.Press,
                    PressID = logRescent.PressID,
                    Shape = logRescent.Shape,
                    ShapeID = logRescent.ShapeID,
                    Container = ContainerScanner,
                    ContainerID = ContainerScanner.Id,
                    ProdColor = logRescent.ProdColor,
                    ProdColorID = logRescent.ProdColorID,
                    FillStatus = fillstatus
                };
                if(cellScanner != null)
                {
                    u.Cell = cellScanner;
                    u.CellID = cellScanner.Id;
                }
                //ContainerScanner.ActionID = action; /*2 : store with product 3 : Shipment*/
            } else
            {
                action = 0;/*0: Stocké vide, store without product*/
                u = new Log()
                {
                    EventTime = DateTime.Now,
                    Operation = 2, // store without product , operation : store
                    Container = ContainerScanner,
                    ContainerID = ContainerScanner.Id,
                    Cell = cellScanner,
                    CellID = cellScanner.Id,
                    FillStatus = fillstatus
                };
                
                //ContainerScanner.ActionID = 0; 
            }
            ContainerScanner.ActionID = action; /*0: Stocké vide, store without product 2 : store with product 3 : Shipment*/
            if (cellScanner != null)
            {
                u.CellID = cellScanner.Id;
                cellScanner.Containers.Add(ContainerScanner); /*Add contenaire into the liste of cell's contenaires*/
            }

            if (actionId == 0 || actionId == 2 || actionId == 3)
            {
                u.Operation = 5; //  Transfer : Déplacment contenaire
            }
            _logService.UpSert(u);
            ContainerScanner.FillStatus = fillstatus;
            ContainerScanner.CellId = u.CellID;
            _contenaireService.UpSert(ContainerScanner);
            
            if (isPalette)
            {
                foreach (var item in _contenaireService.GetAllBacs(ContainerScanner.Id))
                {
                    item.ActionID = action;
                    if(cellScanner != null)
                    {
                        item.CellStock = cellScanner;
                        item.CellId = cellScanner.Id;
                    }
                    
                    item.FillStatus = fillstatus;
                    _contenaireService.UpSert(item);
                }
            }
            upDateCellState();
            bienStock = true;
           }
    }
}

