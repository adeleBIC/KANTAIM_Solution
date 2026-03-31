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
        private string _searchString = "";
        private bool _isLoading = true;

        private int _currentPage = 0;
        private int _pageSize = 14;
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
            _searchString = searchTerm?.Trim() ?? "";
            _currentPage = 0;
            await LoadPageDataAsync();
            StateHasChanged();
        }

        private async Task OnPageChanged(int page)
        {
            if (page < 0 || (page * _pageSize) >= _totalItems && _totalItems > 0) return;

            _currentPage = page;
            await LoadPageDataAsync();
            StateHasChanged();
        }

        private async Task ChangePageSize(int newSize)
        {
            _pageSize = newSize;
            _currentPage = 0;
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
            StateHasChanged();
        }

        async Task SaveAsync()
        {
            var editingItems = Containers.Where(vm => vm.IsEditing).ToList();
            if (!editingItems.Any())
            {
                _snackService.Add("Aucune modification à sauvegarder", Severity.Info);
                return;
            }

            foreach (ContainerVM vm in editingItems)
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
                    }
                    catch (Exception ex)
                    {
                        _snackService.Add($"Erreur sauvegarde: {ex.Message}{ex.InnerException?.Message}", Severity.Error);
                        return; // Arrêter le processus en cas d'erreur
                    }
                }
                else
                {
                    string txt = "<ul>";
                    foreach (ValidationResult item in validationResults)
                        txt += $"<li>{item.ErrorMessage}</li>";
                    txt += "</ul>";
                    _snackService.Add(txt, Severity.Error);
                    return; // Arrêter le processus en cas d'erreur de validation
                }
            }

            _snackService.Add($"{editingItems.Count} élément(s) sauvegardé(s) avec succès!", Severity.Success);
            await LoadPageDataAsync();
            StateHasChanged();
        }

        async Task DeleteAsync()
        {
            var list = Containers.Where(vm => vm.IsChecked).ToList();

            if (list.Count == 0)
            {
                _snackService.Add("Aucun élément sélectionné", Severity.Warning);
                return;
            }

            bool? result = await _dialogService.ShowMessageBox(
                "Confirmation de suppression",
                $"Souhaitez-vous vraiment supprimer les {list.Count} élément(s) sélectionné(s) ?",
                yesText: "Oui", cancelText: "Annuler");

            if (result == true)
            {
                try
                {
                    foreach (ContainerVM item in list)
                    {
                        if (item.Id != 0)
                        {
                            // Supprimer les logs associés
                            await Task.Run(() =>
                            {
                                var logs = _logService.GetAllWithouInclude().Where(l => l.ContainerID == item.Id).ToList();
                                foreach (Log log in logs)
                                    _logService.Delete(log.Id);
                            });

                            _contenaireService.Delete(item.Id);
                        }
                        else
                        {
                            // Supprimer les nouveaux éléments non sauvegardés de la liste
                            Containers.Remove(item);
                        }
                    }

                    _snackService.Add($"{list.Count} élément(s) supprimé(s) avec succès!", Severity.Success);
                    await LoadPageDataAsync();
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    _snackService.Add($"Erreur lors de la suppression: {ex.Message}", Severity.Error);
                }
            }
        }

        async Task CancelAsync()
        {
            // Supprimer les nouveaux éléments non sauvegardés
            var newItems = Containers.Where(c => c.Id == 0 && c.IsEditing).ToList();
            foreach (var item in newItems)
            {
                Containers.Remove(item);
            }

            // Recharger les données pour annuler les modifications
            await LoadPageDataAsync();
            StateHasChanged();
        }

        public string RowClassFct(ContainerVM containerVM, int row)
        {
            return containerVM.IsEditing ? "editing" : "";
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
        private Task OnInJailChanged(ContainerVM item, bool value)
        {
            
            var cellExists = item.CellID != null && item.Cells.Any(c => c.Id == item.CellID);
            if (value && !cellExists)
            {
                _snackService.Add("Impossible : le conteneur n'est pas stocké (CellStock absent)", Severity.Error);
              
                return Task.CompletedTask;
            }

            item.InJail = value;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task OnInMaintenanceChanged(ContainerVM item, bool value)
        {
            var cellExists = item.CellID != null && item.Cells.Any(c => c.Id == item.CellID);
            if (value && !cellExists)
            {
                _snackService.Add("Impossible : le conteneur n'est pas stocké (CellStock absent)", Severity.Error);
                return Task.CompletedTask;
            }

            item.InMaintenance = value;
            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}