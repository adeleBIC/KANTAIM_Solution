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

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class ContainerPge
    {
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] public LogService _logService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<ContainerVM> Containers { get; set; }
        public Dictionary<int,string> ContainerStatus {  get; set; }
        public Dictionary<int, string> CellStatus { get; set; }
        private string _searchString;

        
        protected override async Task OnInitializedAsync()
        {
            CellStatus = new StatusCell().Status;
            ContainerStatus = new StatusContainer().Status;
            await Task.Run(RefreshData);
        }
        void RefreshData()
        {
            Containers = _contenaireService.GetAll(false).Select(u => new ContainerVM(u,
                                                                    _contenaireService.GetAll(true),
                                                                    _contenaireService.GetAllContainerType(),
                                                                    _contenaireService.GetAllCell(),
                                                                    _contenaireService.GetAllAction(),
                                                                    _contenaireService.GetAllProd(),
                                                                    _contenaireService.GetAllColor(),
                                                                    _contenaireService.GetAllPress(),
                                                                    _contenaireService.GetAllMachine())).ToList();
            
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<ContainerVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Number.ToString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        void AddAsync()
        {
            ContainerVM item = new ContainerVM(_contenaireService.GetAll(false),
                                                _contenaireService.GetAllContainerType(),
                                                _contenaireService.GetAllCell(),
                                                _contenaireService.GetAllAction(),
                                                _contenaireService.GetAllProd(),
                                                _contenaireService.GetAllColor(),
                                                _contenaireService.GetAllPress(),
                                                _contenaireService.GetAllMachine()) { IsEditing = true };
            Containers.Insert(0, item);
        }

        async Task SaveAsync()
        {
            foreach (ContainerVM vm in Containers.Where(vm => vm.IsEditing))
            {
                vm.QRCode = "1#" + vm.Number + "#" + vm.ContainerTypeID + "$";
                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {
                    try
                    {
                        Container u = (Container)vm;
                        _contenaireService.UpSert(u);
                        vm.IsEditing = false;

                        _snackService.Add("Données sauvgardées !", Severity.Success);
                    }
                    catch (Exception ex)
                    {

                        _snackService.Add($"{ex.Message}{ex.InnerException.Message}", Severity.Error);
                    }

                   
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
            var list = Containers.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                {
                    foreach (ContainerVM item in Containers.Where(vm => vm.IsChecked).ToList())
                    {
                        if (item.Id != 0)
                        {
                            foreach (Log log in _logService.GetAllWithouInclude().Where(l => l.ContainerID == item.Id)) _logService.Delete(log.Id);

                            _contenaireService.Delete(item.Id);
                            RefreshData();
                            _snackService.Add("Données supprimées !", Severity.Success);
                        }
                    }
                });
            }
            await InvokeAsync(StateHasChanged);
        }

        async Task CancelAsync()
        {
            RefreshData();
            await InvokeAsync(StateHasChanged);
        }

        public string RowClassFct(ContainerVM unityVM, int row)
        {
            return unityVM.IsEditing ? "editing" : "";
        }

        
        void SelectionChanged(HashSet<ContainerVM> changes)
        {
            foreach (var u in Containers)
                u.IsChecked = changes.Contains(u);
        }
    }
}