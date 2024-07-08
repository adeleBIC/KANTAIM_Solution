using Azure;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.Ressources;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Pages.Consultation
{
    public partial class LogPge
    {
        [Inject] public LogService _logService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }
        private string _searchString;
        public List<LogVM> Logs { get; set; }
        public Dictionary<int, string> ContainerStatus { get; set; }
        public Dictionary<int, string> Actions { get; set; }
        void RefreshData()
        {
            Logs = _logService.GetAll()
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
        }
        protected override async Task OnInitializedAsync()
        {
            ContainerStatus = new StatusContainer().Status;
            Actions = new OperationContainer().Operations;
            await Task.Run(RefreshData);
        }
        private Func<LogVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.ContainerID.ToString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };
        void AddAsync()
        {
            LogVM item = new LogVM(
                _logService.GetAllProduct(),
                                        _logService.GetAllPress(),
                                        _logService.GetAllShape(),
                                        _logService.GetAllMachine(),
                                        _logService.GetAllCell(),
                                        _logService.GetAllContainer(),
                                        _logService.GetAllColor());
            item.EventTime = DateTime.Now;
            Logs.Add(item);
        }
        async Task SaveAsync()
        {
            foreach (LogVM vm in Logs.Where(vm => vm.IsEditing))
            {
                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {
                    Log u = (Log)vm;
                    _logService.UpSert(u);
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
            var list = Logs.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                {
                    foreach (LogVM item in Logs.Where(vm => vm.IsChecked).ToList())
                    {
                        if (item.Id != 0)
                        {
                            _logService.Delete(item.Id);
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