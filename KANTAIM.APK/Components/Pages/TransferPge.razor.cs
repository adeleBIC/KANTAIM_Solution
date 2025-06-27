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
using KANTAIM.APK.MessageBus.Messages;
using Java.Util;
using Java.Nio.Channels;

namespace KANTAIM.APK.Components.Pages
{
    public partial class TransferPge : BasePage
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public ScanService _scanService { get; set; }
        [Inject] public ActionService _actionService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        [Inject] public LogService _logService { get; set; }
        public string? ContainerValue { get; set; }
        public Container? ContainerScanner { get; set; }
        public Container? OtherScanner { get; set; }
        public bool GoodTransfer { get; set; } = false;
        public string? TextValue { get; set; }
        [Parameter] public int Number { get; set; }

        private int state;
        public int State
        {
            get
            {
                state = 0;
                if (GoodTransfer) state = 3;
                else if (ContainerScanner != null && OtherScanner != null) state = 1;
                else if (ContainerScanner != null) state = 2;
                else state = 0;

                return state;
            }
        }
        protected override async Task OnInitializedAsync()
        {
            if(ContainerScanner == null)
            {
                ContainerScanner = _contenaireService.GetContainerByNumber(Number);
                //logRescent = _logService.GetByContenaireId(ContainerScanner.Id);
                state = 2;
            }
        }

        void OtherScan(string code)

        {
            string[] parts = _scanService.ParseCode(code);
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
                    OtherScanner = _contenaireService.GetContainerByNumber(Number);
                    if (OtherScanner.FillStatus != 1)
                    {
                        _snackService.Add("Svp vérifier le Contenaire est vide.", Severity.Error);
                        OtherScanner = null;
                    }
                }
            }
        }

        async void TransferInformation()
        {
            DateTime now = DateTime.Now;
            OtherScanner.CellID = ContainerScanner.CellID;
            OtherScanner.ProductID = ContainerScanner.ProductID;
            OtherScanner.ProdColorID = ContainerScanner.ProdColorID;
            OtherScanner.PressID = ContainerScanner.PressID;
            int? shapeId = ContainerScanner.Press?.ShapeID;
            OtherScanner.MachineID = ContainerScanner.MachineID;
            OtherScanner.ActionID = ContainerScanner.ActionID;
            OtherScanner.FillStatus = StatusContainer.Undefinded;
            OtherScanner.Status = ContainerScanner.Status;
            OtherScanner.InJail = ContainerScanner.InJail;
            OtherScanner.InMaintenance = ContainerScanner.InMaintenance;
            OtherScanner.Comment = ContainerScanner.Comment;
            OtherScanner.LastEvent = now;
            _contenaireService.UpSert(OtherScanner);

            ContainerScanner.ProductID = null;
            ContainerScanner.ProdColorID = null;
            ContainerScanner.PressID = null;
            ContainerScanner.MachineID = null;
            ContainerScanner.CellID = null;
            ContainerScanner.FillStatus = StatusContainer.Empty;
            ContainerScanner.LastEvent = now;
            _contenaireService.UpSert(ContainerScanner);

            Log u_autre = new Log()
            {
                EventTime = now,
                Operation = OperationContainer.Transfer, // Transfer contenaire
                ProductID = OtherScanner.ProductID,
                PressID = OtherScanner.PressID,
                ShapeID = shapeId,
                ContainerID = OtherScanner.Id,
                ProdColorID = OtherScanner.ProdColorID,
                CellID = OtherScanner.CellID,
                MachineID = OtherScanner.MachineID,
                FillStatus = StatusContainer.Undefinded
            };
            _logService.UpSert(u_autre);

            Log u_contenaire = new Log()
            {
                EventTime = now,
                Operation = OperationContainer.Transfer, // Transfer contenaire
                Container = ContainerScanner,
                ContainerID = ContainerScanner.Id,
                FillStatus = StatusContainer.Empty
            };
            _logService.UpSert(u_contenaire);

            GoodTransfer = true;
            state = State;
            NavigationManager.NavigateTo($"/");
            _snackService.Add("Bien Transféré !", Severity.Success);
        }

        public override async void OnMessageReceived(InputMessage msg)
        {
            TextValue = msg.Code;
            switch (state)
            {
                case 0:
                    ContainerScanner = _contenaireService.GetContainerByNumber(Number);
                    break;
                case 2:
                    OtherScan(msg.Code);
                    break;
            }
            state = State;
            await InvokeAsync(StateHasChanged);
        }
    }
}