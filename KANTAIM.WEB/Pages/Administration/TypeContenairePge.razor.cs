using Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using KANTAIM.DAL.Model;
using System.ComponentModel.DataAnnotations;


namespace KANTAIM.WEB.Pages.Administration
{
    public partial class TypeContenairePge
    {
        [Inject] public ContenaireTypeService _contenaireTypeService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<ContenaireTypeVM> ContenaireTypes { get; set; }
        private string _searchString;

        protected override async Task OnInitializedAsync()
        {
            //RefreshData();
            await Task.Run(RefreshData);
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<ContenaireTypeVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        void AddAsync()
        {
            ContenaireTypeVM item = new ContenaireTypeVM() { IsEditing = true };
            ContenaireTypes.Insert(0, item);
            //await InvokeAsync(StateHasChanged);
        }

        async Task SaveAsync()
        {
            foreach (ContenaireTypeVM vm in ContenaireTypes.Where(vm => vm.IsEditing))
            {
                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {
                    ContainerType u = (ContainerType)vm;
                    _contenaireTypeService.UpSert(u);
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
            var list = ContenaireTypes.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                {
                    foreach (ContenaireTypeVM item in ContenaireTypes.Where(vm => vm.IsChecked).ToList())
                    {
                        if (item.Id != 0)
                        {
                            _contenaireTypeService.Delete(item.Id);
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

        public string RowClassFct(ContenaireTypeVM unityVM, int row)
        {
            return unityVM.IsEditing ? "editing" : "";
        }

        void RefreshData()
        {
            ContenaireTypes = _contenaireTypeService.GetAll().Select(u => new ContenaireTypeVM(u)).ToList();
        }
        void SelectionChanged(HashSet<ContenaireTypeVM> changes)
        {
            foreach (var u in ContenaireTypes)
                u.IsChecked = changes.Contains(u);
        }
    }
}