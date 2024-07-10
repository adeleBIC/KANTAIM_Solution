using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
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
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using KANTAIM.DAL.Model;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Components.WorkshopComp
{
    public partial class PresseDetail
    {
        [Inject] public PressService _pressService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        [Parameter] public EventCallback OnSaved { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }

        [Parameter] public WorkshopVM Workshop { get; set; }
        [Parameter] public List<PressVM> Presses { get; set; }

        private string _searchString;

        void Add()
        {
            PressVM item = new PressVM(_pressService.GetAllShapes().Where(p => p.Active.HasValue)) { IsEditing = true };
            Presses.Insert(0, item);
            //await InvokeAsync(StateHasChanged);
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<PressVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Number.ToString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        protected override async Task OnInitializedAsync()
        {
            //RefreshData();
            await Task.Run(RefreshData);
        }

        async Task SaveAsync()
        {
            foreach (PressVM vm in Presses.Where(vm => vm.IsEditing))
            {
                vm.WorkshopID = Workshop.Id;
                vm.QRcode = "3#" + vm.Number + "$";

                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {    
                    Press u = (Press)vm;
                    _pressService.UpSert(u);
                    vm.IsEditing = false;

                    _snackService.Add("Données sauvgardées !", Severity.Success);
                }
                else
                {
                    string txt = "<ul>";
                    foreach (ValidationResult item in validationResults)
                        txt += $"<li>{item.ErrorMessage}</li>";

                    txt += "</ul>";
                    _snackService.Add(txt, Severity.Error);
                }
            }
            await InvokeAsync(StateHasChanged);
        }

        async Task DeleteAsync()
        {
            var list = Presses.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                {
                    foreach (PressVM item in list)
                    {
                        if (item.Id != 0)
                        {
                            _pressService.Delete(item.Id);
                            RefreshData();
                            _snackService.Add("Données supprimées !", Severity.Success);
                        }
                    }
                });
            }

            await OnSaved.InvokeAsync(StateHasChanged);
        }

        async Task CancelAsync()
        {
            RefreshData();
            await OnCancel.InvokeAsync(StateHasChanged);
        }

        public string RowClassFct(PressVM vm, int row)
        {
            return vm.IsEditing ? "editing" : "";
        }

        void RefreshData()
        {
            if (Workshop != null) Presses = _pressService.GetAllPerWorkshop(Workshop.Id).Select(u => new PressVM(u, _pressService.GetAllShapes())).OrderBy(p => p.Number).ToList();
        }

        void SelectionChanged(HashSet<PressVM> changes)
        {
            foreach (var u in Presses)
                u.IsChecked = changes.Contains(u);
        }
    }
}