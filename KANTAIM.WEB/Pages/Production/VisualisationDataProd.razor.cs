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

namespace KANTAIM.WEB.Pages.Production
{
    public partial class VisualisationDataProd
    {
        [Inject] public DataProdService _dataProdService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<DataProdVM> DataProds { get; set; }
        private string _searchString;

        MudDatePicker _picker;
        DateTime? date;
        private bool autoClose;

        private void OnDateChanged()
        {
            date = date ?? DateTime.MinValue;
            RefreshData();
        }



        protected override async Task OnInitializedAsync()
        {
            date = DateTime.Today.AddDays(-1);
            //RefreshData();
            await Task.Run(RefreshData);
        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<DataProdVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.PressID.ToString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        void AddAsync()
        {
            DataProdVM item = new DataProdVM(_dataProdService.GetAllProducts(), _dataProdService.GetAllPresses()) { ProductID = 0, PressID = 0, IsEditing = true, };
            DataProds.Add(item);
            //await InvokeAsync(StateHasChanged);
        }

        async Task SaveAsync()
        {
            foreach (DataProdVM vm in DataProds.Where(vm => vm.IsEditing))
            {
                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {
                    try
                    {
                        DataProd u = (DataProd)vm;
                        _dataProdService.UpSert(u);
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
            var list = DataProds.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                {
                    foreach (DataProdVM item in DataProds.Where(vm => vm.IsChecked).ToList())
                    {
                        if (item.Id != 0)
                        {
                            try
                            {
                                _dataProdService.Delete(item.Id);
                                RefreshData();
                                _snackService.Add("Données supprimées !", Severity.Success);
                            }
                            catch (Exception ex)
                            {

                                _snackService.Add($"{ex.Message}{ex.InnerException.Message}", Severity.Error);
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

        public string RowClassFct(DataProdVM unityVM, int row)
        {
            return unityVM.IsEditing ? "editing" : "";
        }

        void RefreshData()
        {

            if (date != null)
            {
                DataProds = _dataProdService.GetAll()
                            .Where(u => u.DateProd.Date == date?.Date) // Filter by the selected date
                            .Select(u => new DataProdVM(u, _dataProdService.GetAllProducts(), _dataProdService.GetAllPresses()))
                            .ToList();
            }
            else
            {
                DataProds = _dataProdService.GetAll()
                            .Select(u => new DataProdVM(u, _dataProdService.GetAllProducts(), _dataProdService.GetAllPresses()))
                            .ToList();
            }

        }
        void SelectionChanged(HashSet<DataProdVM> changes)
        {
            foreach (var u in DataProds)
                u.IsChecked = changes.Contains(u);
        }
    }
}