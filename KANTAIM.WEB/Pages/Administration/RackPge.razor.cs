using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class RackPge
    {
        [Inject] public RackService _rackService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<RackVM> Racks { get; set; }
        private string _searchString;

        protected override async Task OnInitializedAsync()
        {
            //RefreshData();

            await Task.Run(RefreshData);
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<RackVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Name.ToString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        void AddAsync()
        {
            RackVM item = new RackVM(_rackService.GetAllWorkshops()) {Name = "", IsEditing = true, };
            Racks.Insert(0, item);
        }

        async Task SaveAsync()
        {
            foreach (RackVM vm in Racks.Where(vm => vm.IsEditing))
            {
                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {

                    try
                    {
                        Rack u = (Rack)vm;
                        _rackService.UpSert(u);
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
            var list = Racks.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                {
                    foreach (RackVM item in Racks.Where(vm => vm.IsChecked).ToList())
                    {
                        if (item.Id != 0)
                        {
                            _rackService.Delete(item.Id);
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

        public string RowClassFct(RackVM vm, int row)
        {
            return vm.IsEditing ? "editing" : "";
        }

        void RefreshData()
        {
            Racks = _rackService.GetAll().Select(u => new RackVM(u, _rackService.GetAllWorkshops())).ToList();
        }
        void SelectionChanged(HashSet<RackVM> changes)
        {
            foreach (var u in Racks)
                u.IsChecked = changes.Contains(u);
        }
    }
}