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
using KANTAIM.WEB.ViewModels;
using KANTAIM.DAL.Services;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class WorkshopPge
    {
        [Inject] public WorkshopService _workshopService { get; set; }
        [Inject] public PressService _pressService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<WorkshopVM> Workshops { get; set; }
        public List<PressVM> Presses { get; set; }
        public WorkshopVM SelectedWorkshop { get; set; }
        private string _searchString;
        MudListItem selectedItem;
        object selectedValue;

        public bool IsEditing
        {
            get
            {
                if (SelectedWorkshop == null)
                {
                    return false;
                }
                else if (Workshops.Count > 0)
                {
                    return Workshops.Any(r => r.IsEditing);
                }
                else return false;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            //RefreshData();
            await Task.Run(RefreshData);
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<WorkshopVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        private async void SelectWorkshop(WorkshopVM ws)
        {
            //foreach (var item in Receipes) item.IsEditing = false;
            if (IsEditing) await Cancel();
            SelectedWorkshop = ws;
            selectedValue = ws.Id;
            RefreshPressesData();
        }

        async Task SaveAsync()
        {
            RefreshData();
            await InvokeAsync(StateHasChanged);
        }

        public async Task Cancel()
        {
            RefreshData();
            if (SelectedWorkshop != null) SelectedWorkshop = Workshops.SingleOrDefault(r => r.Id == SelectedWorkshop.Id);

            await InvokeAsync(StateHasChanged);
        }

        void AddAsync()
        {
            WorkshopVM vm = new WorkshopVM() { Name = "Nouvel Atelier", IsEditing = true };
            Workshops.Insert(0,vm);
            SelectedWorkshop = vm;
            selectedValue = vm.Id;
            //await InvokeAsync(StateHasChanged);
        }

        async void EditAsync()
        {
            if (SelectedWorkshop != null) SelectedWorkshop.IsEditing = true;
        }

        async Task SavePressAsync()
        {
            RefreshPressesData();
            await InvokeAsync(StateHasChanged);
        }

        async Task CancelPressAsync()
        {
            RefreshPressesData();
            await InvokeAsync(StateHasChanged);
        }

        async Task DeleteAsync()
        {
            await _dialogService.Confirm($"Souhaitez-vous l'atelier sélectionné ?", () =>
            {
                Workshops.Remove(SelectedWorkshop);
                if (SelectedWorkshop.Id != 0) _workshopService.Delete(SelectedWorkshop.Id);
                SelectedWorkshop = null;
                _snackService.Add("Données supprimées !", Severity.Success);
            });

            await InvokeAsync(StateHasChanged);
        }

        public string RowClassFct(WorkshopVM unityVM, int row)
        {
            return unityVM.IsEditing ? "editing" : "";
        }

        void RefreshData()
        {
            //_WorkshopService.ResetCache();
            Workshops = _workshopService.GetAll().Select(u => new WorkshopVM(u)).ToList();
        }

        void RefreshPressesData()
        {
            //_WorkshopService.ResetCache();
            if (SelectedWorkshop != null) Presses = _pressService.GetAllPerWorkshop(SelectedWorkshop.Id).Select(u => new PressVM(u, _pressService.GetAllShapes())).OrderBy(p=>p.Number).ToList();
        }

    }
}