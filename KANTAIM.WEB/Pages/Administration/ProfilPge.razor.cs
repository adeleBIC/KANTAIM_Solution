using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using static MudBlazor.CategoryTypes;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class ProfilPge
    {
        [Inject] public ProfilService _profilService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<ProfilVM> Profils { get; set; }
        public List<Rack> Racks { get; set; }
        private string _searchString;

        protected override async Task OnInitializedAsync()
        {
            await Task.Run(RefreshData);
            await InvokeAsync(StateHasChanged);

        }

        // quick filter - filter gobally across multiple columns with the same input
        private Func<ProfilVM, bool> _quickFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (x.Name.ToString().Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        void AddAsync()
        {
            ProfilVM item = new ProfilVM() { Name = "", IsEditing = true, };
            Profils.Insert(0, item);
        }

        async Task SaveAsync()
        {
            foreach (ProfilVM vm in Profils.Where(vm => vm.IsEditing))
            {
                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {

                    try
                    {
                        Profil u = (Profil)vm;
                        int pId = _profilService.UpSert(u);

                        foreach (var m in Racks)
                        {
                            if (vm.SelectedRackNames?.Contains(m.Name) ?? false)
                            {
                                if (vm.Racks == null) vm.Racks = new List<Rack>();
                                if (!vm.Racks.Any(r => r.Id == m.Id))
                                {
                                    RackProfil rp = new RackProfil() { RackId = m.Id, ProfilId = pId };
                                    _profilService.InsertRackProfil(rp);
                                    if (vm.RackProfils == null) vm.RackProfils = new List<RackProfil>();
                                    vm.RackProfils.Add(rp);
                                    vm.Racks.Add(m);
                                }
                            }
                            else
                            {
                                Rack rackToRemove = vm.Racks?.FirstOrDefault(r => r.Id == m.Id);
                                if (rackToRemove != null)
                                {
                                    RackProfil rp = vm.RackProfils?.FirstOrDefault(r => r.RackId == rackToRemove.Id);
                                    _profilService.DeleteRackProfil(rackToRemove.Id, pId);
                                    vm.RackProfils.Remove(rp);
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
            //RefreshData();
            await InvokeAsync(StateHasChanged);
        }

        async Task DeleteAsync()
        {
            var list = Profils.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                {
                    foreach (ProfilVM item in Profils.Where(vm => vm.IsChecked).ToList())
                    {
                        if (item.Id != 0)
                        {
                            foreach (RackProfil rp in item.RackProfils) _profilService.DeleteRackProfil(rp.RackId, item.Id);

                            _profilService.Delete(item.Id);
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

        public string RowClassFct(ProfilVM vm, int row)
        {
            return vm.IsEditing ? "editing" : "";
        }

        void RefreshData()
        {
            Profils = _profilService.GetAll().Select(u => new ProfilVM(u)).ToList();
            Racks = _profilService.GetAllRack().ToList();
        }
        void SelectionChanged(HashSet<ProfilVM> changes)
        {
            foreach (var u in Profils)
                u.IsChecked = changes.Contains(u);
        }

        public string GetMultiSelectionRackText(List<string> selectedRackNames)
        {
            if (selectedRackNames == null || selectedRackNames.Count == 0)
                return "Aucun rack sélectionné";

            return string.Join(", ", selectedRackNames);
        }

        private Task OnSelectedRacksChanged(ProfilVM profil, IEnumerable<string> selectedRackNames)
        {
            profil.SelectedRackNames = selectedRackNames.ToList();
            profil.IsEditing = true;
            return Task.CompletedTask;
        }
    }
}