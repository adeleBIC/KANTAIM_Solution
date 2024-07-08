using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
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
using KANTAIM.DAL.Services;
using KANTAIM.DAL.Model;
using KANTAIM.WEB.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Pages.Administration
{
    public partial class UserPge
    {
        [Inject] public UserService _userService { get; set; }
        [Inject] IDialogService _dialogService { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public List<UserVM> Users { get; set; }
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
            UserVM item = new UserVM(_userService.GetAllLvl()) { LoginADUser = "", IsEditing = true, UserAccessLvlId = _userService.GetFirstLvl().Id };
            Users.Insert(0,item);
            //await InvokeAsync(StateHasChanged);
        }

        async Task SaveAsync()
        {
            foreach (UserVM vm in Users.Where(vm => vm.IsEditing))
            {
                ValidationContext validationContext = new ValidationContext(vm);
                var validationResults = vm.Validate(validationContext).ToList();

                if (validationResults.Count == 0)
                {
                    User u = (User)vm;
                    _userService.UpSert(u);
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
            var list = Users.Where(vm => vm.IsChecked).ToList();

            if (list.Count > 0)
            {
                await _dialogService.Confirm($"Souhaitez-vous supprimer les {list.Count} lignes ?", () =>
                {
                    foreach (UserVM item in Users.Where(vm => vm.IsChecked).ToList())
                    {
                        if (item.Id != 0)
                        {
                            _userService.Delete(item.Id);
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

        public string RowClassFct(UserVM unityVM, int row)
        {
            return unityVM.IsEditing ? "editing" : "";
        }

        void RefreshData()
        {
            Users = _userService.GetAll().Select(u => new UserVM(u,_userService.GetAllLvl())).ToList();
        }
        void SelectionChanged(HashSet<UserVM> changes)
        {
            foreach (var u in Users)
                u.IsChecked = changes.Contains(u);
        }
    }
}