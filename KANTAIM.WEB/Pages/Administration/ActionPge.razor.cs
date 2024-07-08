using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class ActionPge
    {
        [Inject] public ActionService _actionService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        public ContainerActionVM SelectedAction { get; set; }
        public List<ContainerActionVM> ContainerActions { get; set; }
        public string TextValue { get; set; }
        private string _searchString;

        private Func<ContainerActionVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Id.ToString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };


        public string RowClassFct(ContainerActionVM unityVM, int row)
        {
            return unityVM.IsEditing ? "editing" : "";
        }
        protected override async Task OnInitializedAsync()
        {
            //RefreshData();
            await Task.Run(RefreshData);
        }
        void AddAsync()
        {
            ContainerActionVM item = new ContainerActionVM() { IsEditing = true };
            ContainerActions.Insert(0, item);
        }


        async Task SaveAsync()
        {
            foreach (ContainerActionVM vm in ContainerActions.Where(vm => vm.IsEditing))
            {
                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {
                    try
                    {
                        ContainerAction u = (ContainerAction)vm;
                        _actionService.UpSert(u);
                        vm.IsEditing = false;

                        _snackService.Add("Données sauvgardées !", Severity.Success);
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        _snackService.Add("Erreur de concurrence: Les données ont été modifiées par un autre utilisateur.", Severity.Error);
                    }
                }
                else
                {
                    // Handle validation errors
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
            var list = ContainerActions.Where(vm => vm.IsChecked).ToList();
            foreach (var item in list)
            {
                if (list.Count > 0)
                {
                    await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                    {
                        foreach (ContainerActionVM item in ContainerActions.Where(vm => vm.IsChecked).ToList())
                        {
                            if (item.Id != 0)
                            {
                                _actionService.Delete(item.Id);
                                RefreshData();
                                _snackService.Add("Données supprimées !", Severity.Success);
                            }
                        }
                    });
                }
                await InvokeAsync(StateHasChanged);
            }
        }

        async Task CancelAsync()
        {
            RefreshData();
            await InvokeAsync(StateHasChanged);
        }
        void RefreshData()
        {
            ContainerActions = _actionService.GetAll().Select(u => new ContainerActionVM(u)).ToList();
        }

        void SelectionChanged(HashSet<ContainerActionVM> changes)
        {
            foreach (var u in ContainerActions)
                u.IsChecked = changes.Contains(u);
        }
    }
}