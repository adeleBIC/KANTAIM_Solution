using KANTAIM.DAL.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using global::System.Threading.Tasks;
using global::Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using KANTAIM.WEB;
using KANTAIM.WEB.Shared;
using MudBlazor;
using KANTAIM.DAL;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using System.ComponentModel.DataAnnotations;
using KANTAIM.WEB.Ressources;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class MaintenancePge

    {
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        [Inject] public ActionService _actionService { get; set; }
        [Inject] public CellService _cellService { get; set; }

        public IEnumerable<ContainerWithEvents> Containers { get; set; }

        public Dictionary<int, string> ContainerStatus { get; set; }
        public Dictionary<int, string> CellStatus { get; set; }
        private string _searchString;


        public record ContainerWithEvents(Container container, string stockTime);



        protected override async Task OnInitializedAsync()
        {
            CellStatus = new StatusCell().Status;
            ContainerStatus = new StatusContainer().Status;
            Containers = new List<ContainerWithEvents> { };
            await Task.Run(RefreshData);
        }
        void RefreshData()
        {
            Containers = _contenaireService.GetAll()
            .Where(u => u.InMaintenance == true)
            .Select(u =>
            {
                int id;
                // Récupérer ProdTime et StockTime depuis _logService
                if (u.ContainerType != null && (u.BigContainer == null || !u.ContainerType.IsContainable))
                {
                    id = u.Id;
                }
                else
                {
                    id = u.BigContainer.Id;
                }
                var stockTime = _logService.GetByContenaireIdAction(id, OperationContainer.Store)?.EventTime ?? DateTime.MinValue;
                return new ContainerWithEvents(u, stockTime.ToShortTimeString() + "   " + stockTime.ToShortDateString());
            })
            .ToList();
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<ContainerWithEvents, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.container.Number.ToString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };



        public void OnViderClicked(Container c)
        {
            c.FillStatus = StatusContainer.Empty;
            c.InMaintenance = false;
            c.ContainerAction = _actionService.GetByStatus(OperationContainer.Undefinded);// En vidange;
            c.ActionID = OperationContainer.Initisalisation;
            _contenaireService.UpSert(c);
            RefreshData();
        }

    }
}