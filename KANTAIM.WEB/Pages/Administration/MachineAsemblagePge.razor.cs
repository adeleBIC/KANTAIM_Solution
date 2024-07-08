using Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using KANTAIM.DAL.Model;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class MachineAsemblagePge
    {
        [Inject] public MachineService _machineService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<MachineVM> Machines { get; set; }
        private string _searchString;

        protected override async Task OnInitializedAsync()
        {
            //RefreshData();
            await Task.Run(RefreshData);
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<UserVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.LoginADUser.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        void AddAsync()
        {
            MachineVM item = new MachineVM(_machineService.GetAllProd()) { IsEditing = true, ProductID = _machineService.GetFirstProdId().Id };
            Machines.Insert(0, item);
            //await InvokeAsync(StateHasChanged);
        }

        async Task SaveAsync()
        {
            foreach (MachineVM vm in Machines.Where(vm => vm.IsEditing))
            {
                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {
                    Machine u = (Machine)vm;
                    _machineService.UpSert(u);
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
            var list = Machines.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                {
                    foreach (MachineVM item in Machines.Where(vm => vm.IsChecked).ToList())
                    {
                        if (item.Id != 0)
                        {
                            _machineService.Delete(item.Id);
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

        public string RowClassFct(MachineVM unityVM, int row)
        {
            return unityVM.IsEditing ? "editing" : "";
        }

        void RefreshData()
        {
            Machines = _machineService.GetAll().Select(u => new MachineVM(u, _machineService.GetAllProd())).ToList();
        }
        void SelectionChanged(HashSet<MachineVM> changes)
        {
            foreach (var u in Machines)
                u.IsChecked = changes.Contains(u);
        }
    }
}