using KANTAIM.APK.MessageBus.Messages;
using KANTAIM.APK.Services;
using KANTAIM.DAL.Services;
using KANTAIM.DAL.Model;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel;

namespace KANTAIM.APK.Components.Pages
{
    public partial class ResetContPge : BasePage
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ProductService _productService { get; set; }
        [Inject] public ColorService _colorService { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public ColorProductService _colorProductServiceService { get; set; }
        [Inject] public ProfilSessionService _profilSessionService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        [Inject] public ScanService _scanService { get; set; }
        public int State { get; set; }
        public DAL.Model.Container? ContainerScanner { get; set; }

        public string? TextValue { get; set; }

        protected override void OnInitialized()
        {
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
                if (type != 1)
                {
                    ContainerScanner = _contenaireService.GetContainerByNumber(contNumber);
                }
                else _snackService.Add("Mauvais QRCode scanné !", MudBlazor.Severity.Error);
            }
            else _snackService.Add("Mauvais QRCode scanné !", MudBlazor.Severity.Error);
            await InvokeAsync(StateHasChanged);
        }
    }
}