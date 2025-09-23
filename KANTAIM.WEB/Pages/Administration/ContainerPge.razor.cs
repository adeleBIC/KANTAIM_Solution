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

        public List<ContainerVM> Containers { get; set; } = new List<ContainerVM>();
        public Dictionary<int, string> ContainerStatus { get; set; }
        public Dictionary<int, string> CellStatus { get; set; }
        private string _searchString;
        private bool _isLoading = true;

        private int _currentPage = 0;
        private int _pageSize = 50;
        private int _totalItems = 0;

        private List<Container> _allContainersForLookup;
        private List<ContainerType> _containerTypes;
        private List<Cell> _cells;
        private List<ContainerAction> _actions;
        private List<Product> _products;
        private List<ProdColor> _colors;
        private List<Press> _presses;
        private List<Machine> _machines;

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            StateHasChanged();

            CellStatus = new StatusCell().Status;
            ContainerStatus = new StatusContainer().Status;

            await LoadLookupDataAsync();

            await LoadPageDataAsync();

            _isLoading = false;
            StateHasChanged();
        }

        private async Task LoadLookupDataAsync()
        {
            await Task.Run(() =>
            {
                _allContainersForLookup = _contenaireService.GetAll(false).ToList();
                _containerTypes = _contenaireService.GetAllContainerType().ToList();
                _cells = _contenaireService.GetAllCell().ToList();
                _actions = _contenaireService.GetAllAction().ToList();
                _products = _contenaireService.GetAllProd().ToList();
                _colors = _contenaireService.GetAllColor().ToList();
                _presses = _contenaireService.GetAllPress().ToList();
                _machines = _contenaireService.GetAllMachine().ToList();
            });
        }

        private async Task LoadPageDataAsync()
        {
            try
            {
                var (data, totalCount) = await _contenaireService.GetPagedContainersAsync(
                    _currentPage,
                    _pageSize,
                    _searchString,
                    false);

                _totalItems = totalCount;

                Containers = data.Select(u => new ContainerVM(u,
                    _allContainersForLookup,
                    _containerTypes,
                    _cells,
                    _actions,
                    _products,
                    _colors,
                    _presses,
                    _machines)).ToList();
            }
            catch (Exception ex)
            {
                _snackService.Add($"Erreur lors du chargement des données: {ex.Message}", Severity.Error);
                Containers = new List<ContainerVM>();
                _totalItems = 0;
            }
        }
        private async Task OnSearchChanged(string searchTerm)
        {
            _searchString = searchTerm;
            _currentPage = 0;
            await LoadPageDataAsync();
            StateHasChanged();
        }

        private async Task OnPageChanged(int page)
        {
            _currentPage = page;
            await LoadPageDataAsync();
            StateHasChanged();
        }

        void AddAsync()
        {
            ContainerVM item = new ContainerVM(_allContainersForLookup,
                                                _containerTypes,
                                                _cells,
                                                _actions,
                                                _products,
                                                _colors,
                                                _presses,
                                                _machines)
            { IsEditing = true };
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
                        _snackService.Add($"{ex.Message}{ex.InnerException?.Message}", Severity.Error);
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

            await LoadPageDataAsync();
            await InvokeAsync(StateHasChanged);
        }

        async Task DeleteAsync()
        {
            var list = Containers.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", async () =>
                {
                    foreach (ContainerVM item in list)
                    {
                        if (item.Id != 0)
                        {
                            await Task.Run(() =>
                            {
                                foreach (Log log in _logService.GetAllWithouInclude().Where(l => l.ContainerID == item.Id))
                                    _logService.Delete(log.Id);
                            });

                            _contenaireService.Delete(item.Id);
                        }
                    }

                    await LoadPageDataAsync();
                    _snackService.Add("Données supprimées !", Severity.Success);
                    StateHasChanged();
                });
            }
        }

        async Task CancelAsync()
        {
            await LoadPageDataAsync();
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

        private async Task RefreshCurrentPageAsync()
        {
            await LoadPageDataAsync();
            StateHasChanged();
        }

    }
}