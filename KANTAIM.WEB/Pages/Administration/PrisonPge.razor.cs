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
using static KANTAIM.WEB.Pages.Administration.PrisonPge;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class PrisonPge
    {
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<ContainerWithEvents> Containers { get; set; }
        
        public Dictionary<int, string> ContainerStatus { get; set; }
        public Dictionary<int, string> CellStatus { get; set; }
        private string _searchString;


        public class ContainerWithEvents
        {
            public ContainerVM Container { get; set; }
            public DateTime ProdTime { get; set; }
            public DateTime StockTime { get; set; }

            public ContainerWithEvents(ContainerVM container, DateTime prodTime, DateTime stockTime)
            {
                Container = container;
                ProdTime = prodTime;
                StockTime = stockTime;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            CellStatus = new StatusCell().Status;
            ContainerStatus = new StatusContainer().Status;
            await Task.Run(RefreshData);
        }
        void RefreshData()
        {
            Containers = _contenaireService.GetAll()
            .Where(u => u.InJail == true)
            .Select(u =>
            {
                var containerVM = new ContainerVM(
                    u,
                    _contenaireService.GetAllContainer(),
                    _contenaireService.GetAllContainerType(),
                    _contenaireService.GetAllCell(),
                    _contenaireService.GetAllAction()
                );
                int id;
                // Récupérer ProdTime et StockTime depuis _logService
                if(u.ContainerType != null && (u.BigContainer == null || !u.ContainerType.IsContainable))
                {
                    id = u.Id;
                } else
                {
                    id = u.BigContainer.Id;
                }
                var prodTime = _logService.GetByContenaireIdAction(id, OperationContainer.Initisalisation)?.EventTime ?? DateTime.MinValue;
                var stockTime = _logService.GetByContenaireIdAction(id, OperationContainer.Store)?.EventTime ?? DateTime.MinValue;
                return new ContainerWithEvents(containerVM, prodTime, stockTime);
            })
            .ToList();
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<ContainerWithEvents, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Container.Number.ToString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

       
        public string RowClassFct(ContainerWithEvents unityVM, int row)
        {
            return unityVM.Container.IsEditing ? "editing" : "";
        }

    }
}