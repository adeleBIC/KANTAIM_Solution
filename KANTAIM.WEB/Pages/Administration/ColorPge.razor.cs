using Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using KANTAIM.DAL.Model;
using Radzen;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class ColorPge
    {
        [Inject] public ColorService _colorService { get; set; }
        [Inject] public ColorProductService _ColorProductService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        public List<ColorVM> Colors { get; set; }
        private string _searchString;

        protected override async Task OnInitializedAsync()
        {
            //RefreshData();
            await Task.Run(RefreshData);
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<ColorVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        void AddAsync()
        {
            ColorVM item = new ColorVM() { IsEditing = true };
            Colors.Insert(0, item);
            //await InvokeAsync(StateHasChanged);
        }

        async Task SaveAsync()
        {
            foreach (ColorVM vm in Colors.Where(vm => vm.IsEditing))
            {
                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {

                    try
                    {
                        ProdColor u = (ProdColor)vm;
                        _colorService.Upsert(u);
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
            var list = Colors.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                {
                    foreach (ColorVM item in Colors.Where(vm => vm.IsChecked).ToList())
                    {
                        if (item.Id != 0)
                        {
                            if(_ColorProductService.hasLien(item.Id))
                            {
                                _snackService.Add("Cette couleur est associée à d'autres produits !", Severity.Error);
                            } else
                            {
                                _colorService.Delete(item.Id);
                                RefreshData();
                                _snackService.Add("Données supprimées !", Severity.Success);
                            }
                            
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

        public string RowClassFct(ColorVM unityVM, int row)
        {
            return unityVM.IsEditing ? "editing" : "";
        }

        void RefreshData()
        {
            Colors = _colorService.GetAll().Select(u => new ColorVM(u)).OrderBy(u => u.Priority ?? int.MaxValue).ToList();
        }
        void SelectionChanged(HashSet<ColorVM> changes)
        {
            foreach (var u in Colors)
                u.IsChecked = changes.Contains(u);
        }
    }
}