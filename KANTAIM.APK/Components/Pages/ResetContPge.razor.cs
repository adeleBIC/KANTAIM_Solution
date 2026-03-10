using KANTAIM.APK.MessageBus.Messages;
using KANTAIM.APK.Services;
using KANTAIM.DAL.Services;
using KANTAIM.DAL.Model;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel;
using KANTAIM.APK.Resources;
using System.Xml.Serialization;

namespace KANTAIM.APK.Components.Pages
{
    public partial class ResetContPge : BasePage
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] public ColorService _colorService { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public ColorProductService _colorProductServiceService { get; set; }
        [Inject] public ActionService _actionService { get; set; }
        [Inject] public ProfilSessionService _profilSessionService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        [Inject] public ScanService _scanService { get; set; }
        public int State { get; set; }
        public DAL.Model.Container? ContainerScanner { get; set; }
        public Dictionary<int, string> ContainerStatus { get; set; }

        public List<DAL.Model.Container> ListContainerInPallet { get; set; }

        public string? TextValue { get; set; }

        protected override void OnInitialized()
        {
            ContainerStatus = new StatusContainer().Status;
            State = 0;
        }

        public override async void OnMessageReceived(InputMessage msg)
        {
            TextValue = msg.Code;
            string[] parts = _scanService.ParseCode(TextValue);
            if (parts != null)
            {
                int.TryParse(parts[0], out int type);
                int.TryParse(parts[1], out int contNumber);
                if (type == 1)
                {
                    ContainerScanner = _contenaireService.GetContainerByNumber(contNumber);
                    ListContainerInPallet = _contenaireService.GetAllBacs(contNumber).ToList();
                    if (!ContainerScanner.InJail && !ContainerScanner.InMaintenance)
                    {
                        State = 1;
                    }
                    else
                    {
                        _snackService.Add("Le conteneur est en maintenance ou en prison !", MudBlazor.Severity.Error);
                        ContainerScanner = null;
                    }
                }
                else _snackService.Add("Mauvais QRCode scanné !", MudBlazor.Severity.Error);
            }
            else _snackService.Add("Mauvais QRCode scanné !", MudBlazor.Severity.Error);
            await InvokeAsync(StateHasChanged);
        }

        public async void ResetContainer()
        {
            if (ContainerScanner != null)
            {
                ContainerScanner.Status = StatusContainer.Empty;

                ContainerScanner.ContainerID = null;
                ContainerScanner.CellID = null;
                ContainerScanner.ActionID = _actionService.GetByStatus(99)?.Id ?? 99;
                ContainerScanner.ProductID= null;
                ContainerScanner.ProdColorID = null;
                ContainerScanner.PressID = null;
                ContainerScanner.MachineID = null;

                if (ContainerScanner.ContainerType.NbrMaxContainer > 0) //Palette
                {
                    foreach (DAL.Model.Container item in ListContainerInPallet)
                    {
                        item.BigContainer = null;
                        item.CellID = null;
                        item.ContainerID = null;
                        item.ContainerAction = _actionService.GetByStatus(OperationContainer.Shipment);
                        item.ActionID = ContainerScanner.ContainerAction.Id;
                        _contenaireService.UpSert(item);
                    }
                }

                Log u = new Log()
                {
                    EventTime = DateTime.Now,
                    Operation = OperationContainer.Reset,
                    Container = ContainerScanner,
                    ContainerID = ContainerScanner.Id,
                    FillStatus = StatusContainer.Empty
                };

                ContainerScanner.LastEvent = u.EventTime;

                _contenaireService.UpSert(ContainerScanner);
                _logService.UpSert(u);

                _snackService.Add("Conteneur réinitialisé avec succès !", MudBlazor.Severity.Success);
                State = 0;
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}