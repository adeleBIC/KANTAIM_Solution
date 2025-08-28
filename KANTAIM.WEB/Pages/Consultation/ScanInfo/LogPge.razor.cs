using Azure;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Ressources;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Pages.Consultation.ScanInfo
{
    public partial class LogPge
    {
        [Inject] public LogService _logService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        private string _searchString;
        public List<LogVM> Logs { get; set; }
        public Dictionary<int, string> ContainerStatus { get; set; }
        public Dictionary<int, string> Operations { get; set; }
        private bool isLoading = false;

        private DateTime? startDate = DateTime.Today.AddDays(-1);
        private DateTime? endDate = DateTime.Today;
        private bool isDateFilterApplied = true;
        // Méthodes pour la gestion des dates
        private async Task ApplyDateFilter()
        {
            isDateFilterApplied = true;
            await RefreshData();
        }

        private async Task ResetDateFilter()
        {
            startDate = DateTime.Today.AddDays(-30);
            endDate = DateTime.Today;
            isDateFilterApplied = true;
            await RefreshData();
        }

        private async Task ClearDateFilter()
        {
            isDateFilterApplied = false;
            await RefreshData();
        }

        //void RefreshData()
        //{
        //    Logs = _logService.GetAll()
        //            .Where(u => u.EventTime >= startDate && u.EventTime <= endDate)
        //            .Select(u => new LogVM(u,
        //                                    _logService.GetAllProduct(),
        //                                    _logService.GetAllPress(),
        //                                    _logService.GetAllShape(),
        //                                    _logService.GetAllMachine(),
        //                                    _logService.GetAllCell(),
        //                                    _logService.GetAllContainer(),
        //                                    _logService.GetAllColor()))
        //            .OrderByDescending(l => l.EventTime)
        //            .ToList();
        //}

        private async Task RefreshData()
        {
            isLoading = true;
            StateHasChanged();

            await Task.Run(() =>
            {
                Logs = _logService.GetAll()
                        .Where(u => !isDateFilterApplied
                                    || (u.EventTime >= startDate && u.EventTime <= endDate))
                        .Select(u => new LogVM(u,
                                                _logService.GetAllProduct(),
                                                _logService.GetAllPress(),
                                                _logService.GetAllShape(),
                                                _logService.GetAllMachine(),
                                                _logService.GetAllCell(),
                                                _logService.GetAllContainer(),
                                                _logService.GetAllColor()))
                        .OrderByDescending(l => l.EventTime)
                        .ToList();
            });

            isLoading = false;
            StateHasChanged();
        }
        protected override async Task OnInitializedAsync()
        {
            ContainerStatus = new StatusContainer().Status;
            Operations = new OperationContainer().Operations;
            await RefreshData();
        }
        private Func<LogVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Container.Number.ToString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        public string RowClassFct(LogVM unityVM, int row)
        {
            return unityVM.IsEditing ? "editing" : "";
        }
        void SelectionChanged(HashSet<LogVM> changes)
        {
            foreach (var u in Logs)
                u.IsChecked = changes.Contains(u);
        }
    }
}