using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Ressources;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class CellPge
    {
        [Inject] public CellService _cellService { get; set; }
        [Inject] public ContenaireService _contenaireService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<CellVM> Cells { get; set; }
        public List<Rack> Racks { get; set; }
        public Dictionary<int, string> CellStatus { get; set; }
        private string _searchString;

        protected override async Task OnInitializedAsync()
        {
            RefreshData();
            CellStatus = new StatusCell().Status;
            await Task.Run(RefreshData);
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<CellVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        void AddAsync()
        {
            CellVM item = new CellVM() { IsEditing = true}; 
            Cells.Insert(0, item);
            //await InvokeAsync(StateHasChanged);
        }

        async Task SaveAsync()
        {
            foreach (CellVM vm in Cells.Where(vm => vm.IsEditing))
            {
                vm.QRCode = "4#" + vm.X + "#" + vm.Y + "$";
                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {
                    try
                    {
                        Cell u = (Cell)vm;
                        int cId = _cellService.Upsert(u);
                        

                        foreach (var m in Racks)
                        {
                            if (vm.SelectedRackNames?.Contains(m.Name) ?? false)
                            {
                                if (vm.Racks == null) vm.Racks = new List<Rack>();
                                if (!vm.Racks.Any(r => r.Id == m.Id))
                                {

                                    RackCell rc = new RackCell() { RackId = m.Id, CellId = cId };
                                    _cellService.InsertRackCell(rc);
                                    if (vm.RackCells == null) vm.RackCells = new List<RackCell>();
                                    vm.RackCells.Add(rc);
                                    vm.Racks.Add(m);
                                }
                            }
                            else
                            {
                                Rack rackToRemove = vm.Racks?.FirstOrDefault(r => r.Id == m.Id);
                                if (rackToRemove != null) _cellService.DeleteRackCell(rackToRemove.Id, cId);
                                if (rackToRemove != null)
                                {
                                    RackCell rc = vm.RackCells?.FirstOrDefault(r => r.RackId == rackToRemove.Id);
                                    _cellService.DeleteRackCell(rackToRemove.Id, cId);
                                    vm.RackCells.Remove(rc);
                                    vm.Racks.Add(rackToRemove);
                                }
                            }
                        }

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
            var list = Cells.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                {
                foreach (CellVM item in Cells.Where(vm => vm.IsChecked).ToList())
                    {
                        if (item.Id != 0)
                        {
                            foreach (RackCell rc in item.RackCells) _cellService.DeleteRackCell(rc.RackId, item.Id);

                            _cellService.Delete(item.Id);
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

        public string RowClassFct(CellVM unityVM, int row)
        {
            return unityVM.IsEditing ? "editing" : "";
        }

        void RefreshData()
        {
            Cells = _cellService.GetAll().Select(u => new CellVM(u)
            {
                ContainerCount = _cellService.GetContainerCount(u.Id)
            }).OrderBy(c => c.X).ToList();
            Racks = _cellService.GetAllRack().ToList();
        }
        void SelectionChanged(HashSet<CellVM> changes)
        {
            foreach (var u in Cells)
                u.IsChecked = changes.Contains(u);
        }

        public string GetMultiSelectionRackText(List<string> selectedRackNames)
        {
            if (selectedRackNames == null || selectedRackNames.Count == 0)
                return "Aucun rack sélectionné";

            return string.Join(", ", selectedRackNames);
        }

        private Task OnSelectedRacksChanged(CellVM cell, IEnumerable<string> selectedRackNames)
        {
            cell.SelectedRackNames = selectedRackNames.ToList();
            cell.IsEditing = true;
            return Task.CompletedTask;
        }
    }
}




