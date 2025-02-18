using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Routing;
using System;
using KANTAIM.APK.Services;
using KANTAIM.APK.Resources;

namespace KANTAIM.APK.Components.Pages
{
    public partial class TransferPge
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public ScanService _scanService { get; set; }
        [Inject] public ActionService _actionService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        [Inject] public LogService _logService { get; set; }
        public string? ContainerValue { get; set; }
        public string? autreValue { get; set; }
        public Container? ContainerScanner { get; set; }
        public Container? AutreScanner { get; set; }
        public Log logRescent { get; set; }
        public bool bienTransfer { get; set; } = false;
        public string? TextValue { get; set; }
        private int state;
        private static TransferPge _instance;
        private string currentUrl;
        private string pageUrl;
        [Parameter] public int Number { get; set; }

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

        public int State
        {
            get
            {
                state = 0;
                if (bienTransfer) state = 3;
                else if (ContainerScanner != null && AutreScanner != null) state = 1;
                else if (ContainerScanner != null) state = 2;
                else state = 0;

                return state;
            }
        }
        protected override async Task OnInitializedAsync()
        {
            //RefreshData();
            currentUrl = NavigationManager.Uri;
            pageUrl = NavigationManager.Uri;
            _instance = this;
            if(ContainerScanner == null)
            {
                ContainerScanner = _contenaireService.GetContainerByNumber(Number);
                logRescent = _logService.GetByContenaireId(ContainerScanner.Id);
                state = 2;
            }
            NavigationManager.LocationChanged += OnLocationChanged;
        }
        private string LastKey { get; set; }

        [JSInvokable]
        public static void CaptureInputTransfer(string input)
        {
            _instance?.HandleInput(input);
        }

        private async void HandleInput(string input)
        {
            if (currentUrl == pageUrl)
            {
                if (input == "Enter")
                {
                    switch (state)
                    {
                        case 0:
                            ContainerScanner = _contenaireService.GetContainerByNumber(Number);
                            logRescent = _logService.GetByContenaireId(ContainerScanner.Id);
                            break;
                        case 2:
                            autreScan(TextValue);
                            break;
                    }
                    TextValue = null;
                    state = State;
                    await InvokeAsync(StateHasChanged);
                    

                }
                else
                {
                    TextValue += input;
                    state = State;
                    await InvokeAsync(StateHasChanged);

                }
            }
            
        }

        //void containerScan(string code)
        //{
        //    string[] parts = _scanService.scanCode(code);
        //    if (parts != null)
        //    {
        //        int.TryParse(parts[0], out int type);
        //        if (type != 1)
        //        {
        //            _snackService.Add("Svp scanner le QR code du Contenaire.", Severity.Error);
        //        }
        //        else
        //        {
        //            int.TryParse(parts[1], out int Number);
        //            ContainerScanner = _contenaireService.GetContainerByNumber(Number);
        //            logRescent = _logService.GetByContenaireId(ContainerScanner.Id);
        //        }
        //    }
        //}

        void autreScan(string code)

        {
            string[] parts = _scanService.scanCode(code);
            if (parts != null)
            {
                int.TryParse(parts[0], out int type);
                if (type != 1)
                {
                    _snackService.Add("Svp scanner le QR code du Contenaire.", Severity.Error);
                }
                else
                {
                    int.TryParse(parts[1], out int Number);
                    AutreScanner = _contenaireService.GetContainerByNumber(Number);
                    if (AutreScanner.FillStatus != 1)
                    {
                        _snackService.Add("Svp vérifier le Contenaire est vide.", Severity.Error);
                        AutreScanner = null;
                    }
                }
            }
        }

        async void transferInformation()
        { 
            AutreScanner.CellId = ContainerScanner.CellId;
            AutreScanner.ActionID = ContainerScanner.ActionID;
            AutreScanner.FillStatus = StatusContainer.Undefinded;
            AutreScanner.Status = ContainerScanner.Status;
            AutreScanner.InJail = ContainerScanner.InJail;
            AutreScanner.InMaintenance = ContainerScanner.InMaintenance;
            AutreScanner.Comment = ContainerScanner.Comment;
            _contenaireService.UpSert(AutreScanner);
            //ContainerScanner.ActionID = OperationContainer.Inject;
            //ContainerScanner.ContainerAction = _actionService.GetById(OperationContainer.Inject);
            ContainerScanner.FillStatus = StatusContainer.Empty;
            _contenaireService.UpSert(ContainerScanner);

            Log u_autre = new Log()
            {
                EventTime = DateTime.Now,
                Operation = OperationContainer.Transfer, // Transfer contenaire
                ProductID = logRescent.ProductID,
                Press = logRescent.Press,
                PressID = logRescent.PressID,
                Shape = logRescent.Shape,
                ShapeID = logRescent.ShapeID,
                Container = AutreScanner,
                ContainerID = AutreScanner.Id,
                ProdColor = logRescent.ProdColor,
                ProdColorID = logRescent.ProdColorID,
                CellID = logRescent.CellID,
                MachineID = logRescent.MachineID,
                Machine = logRescent.Machine,
                FillStatus = StatusContainer.Undefinded
            };
            _logService.UpSert(u_autre);

            Log u_contenaire = new Log()
            {
                EventTime = DateTime.Now,
                Operation = OperationContainer.Transfer, // Transfer contenaire
                Container = ContainerScanner,
                ContainerID = ContainerScanner.Id,
                FillStatus = StatusContainer.Empty
            };
            _logService.UpSert(u_contenaire);

            bienTransfer = true;
            state = State;
            NavigationManager.NavigateTo($"/");
            _snackService.Add("Bien Transféré !", Severity.Success);
        }
    }
}