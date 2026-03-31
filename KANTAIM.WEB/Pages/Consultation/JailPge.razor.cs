using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
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
using static KANTAIM.WEB.Pages.Consultation.JailPge;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace KANTAIM.WEB.Pages.Consultation
{
    public partial class JailPge
    {
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        [Inject] public ActionService _actionService { get; set; }
        [Inject] public CellService _cellService { get; set; }
        private bool _isLoading = false;
        public IEnumerable<ContainerWithEvents> Containers { get; set; }
        
        public Dictionary<int, string> ContainerStatus { get; set; }
        public Dictionary<int, string> CellStatus { get; set; }
        private string _searchString;
        private bool _isRefreshing = false;

        bool isPopoverOpen = false;
        List<Container> contenairesNames = new List<Container>();
        void TogglePopover()
        {
            isPopoverOpen = !isPopoverOpen;
        }
        Container? selectedContenaire;

        void SelectContenaire(Container contenaire)
        {
            selectedContenaire = contenaire;
        }
        void MiseEnPrison()
        {
            if (selectedContenaire != null)
            {
                var logRescent = _logService.GetByContenaireNumber(selectedContenaire.Number);
                if (logRescent != null)
                {
                    selectedContenaire.InJail = true;
                    _contenaireService.UpSert(selectedContenaire);
                    selectedContenaire.CellStock = _cellService.GetAll().Where(c => c.IsJail == true).FirstOrDefault();
                    upDateCellState(selectedContenaire.CellStock);

                    Log logUpdate = new Log()
                    {
                        EventTime = DateTime.Now,
                        Operation = selectedContenaire.ActionID, // Initialisation pour le bac
                        ProductID = logRescent.ProductID,
                        Press = logRescent.Press,
                        PressID = logRescent.PressID,
                        Shape = logRescent.Shape,
                        ShapeID = logRescent.ShapeID,
                        Container = selectedContenaire,
                        ContainerID = selectedContenaire.Id,
                        ProdColor = logRescent.ProdColor,
                        ProdColorID = logRescent.ProdColorID,
                        CellID = logRescent.CellID,
                        FillStatus = selectedContenaire.FillStatus
                    };
                    _logService.UpSert(logRescent);
                    _snackService.Add("Mise en Prison !", Severity.Success);
                    RefreshData();
                }
                
            }
        }
        public record ContainerWithEvents(Container container, string prodTime, string stockTime);



        protected override async Task OnInitializedAsync()
        {
            CellStatus = new StatusCell().Status;
            ContainerStatus = new StatusContainer().Status;

            // Yield so Blazor renders the skeleton before we block.
            await Task.Yield();

            await Task.Run(() =>
            {
                contenairesNames = _contenaireService.GetAll()
                    .Where(c => !c.InJail)
                    .ToList();

                RefreshDataInternal();
            });

            _isLoading = false;
            StateHasChanged();
        }

        async Task RefreshData()
        {
            _isRefreshing = true;
            StateHasChanged();

            await Task.Run(RefreshDataInternal);

            _isRefreshing = false;
            StateHasChanged();
        }

        void RefreshDataInternal()
        {
            var jailContainers = _contenaireService.GetAll()
                .Where(u => u.InJail == true)
                .ToList();

            var logIds = jailContainers
                .Select(u =>
                    u.ContainerType != null && (u.BigContainer == null || !u.ContainerType.IsContainable)
                        ? u.Id
                        : u.BigContainer.Id)
                .Distinct()
                .ToList();

            var timesMap = _logService.GetTimesForContainers(logIds);

            Containers = jailContainers.Select(u =>
            {
                var id = u.ContainerType != null && (u.BigContainer == null || !u.ContainerType.IsContainable)
                    ? u.Id : u.BigContainer.Id;

                timesMap.TryGetValue(id, out var times);

                return new ContainerWithEvents(u, Format(times.ProdTime), Format(times.StockTime));
            }).ToList();

            contenairesNames = _contenaireService.GetAll()
                .Where(c => !c.InJail)
                .ToList();
        }

        static string Format(DateTime? dt) =>
            dt.HasValue && dt.Value != DateTime.MinValue
                ? $"{dt.Value.ToShortTimeString()}   {dt.Value.ToShortDateString()}"
                : "—";

        // quick filter - filter gobally across multiple columns with the same input
        private Func<ContainerWithEvents, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.container.Number.ToString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };


        void upDateCellState(DAL.Model.Cell cell)
        {
            if (_contenaireService.CountCellsInJail(cell.Id) == 0)
            {
                cell.Status = StatusCell.Empty;
            }
            else
            {
                cell.Status = StatusCell.InFill;
            }

            _cellService.Upsert(cell);

        }
        public void OnViderClicked(Container c)
        {
            c.FillStatus = StatusContainer.Empty;
            c.InJail = false;
            c.ContainerAction = _actionService.GetByStatus(OperationContainer.Undefinded);// En vidange;
            c.ActionID = OperationContainer.Initisalisation;
            _contenaireService.UpSert(c);
            upDateCellState(c.CellStock);
            RefreshData();
        }

        public void OnSortirClicked(Container c)
        {
            c.InJail = false;
            _contenaireService.UpSert(c);
            upDateCellState(c.CellStock);
            RefreshData();
        }

    }
}