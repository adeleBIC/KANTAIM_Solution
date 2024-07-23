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

namespace KANTAIM.WEB.Shared
{
    public partial class MainLayout
    {
        [Inject] public UserService _userService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] ISnackbar _snackService { get; set; }

        public int UserLvl { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            User user = _userService.GetByName(authState.User.Identity.Name ?? "");

            UserLvl = user?.UserAccessLvl.AccesLvL ?? 0;

            await InvokeAsync(StateHasChanged);
        }

        private void NavigateToAuthorizedPage(string page, int lvl)
        {
            if (UserLvl < lvl)
            {
                _snackService.Add("Niveau d'autorisation insuffisant", Severity.Error);
                return;
            }
            NavigationManager.NavigateTo(page);
        }

        private void NavigateToHome()
        {
            NavigationManager.NavigateTo("/");
        }

        private void NavigateToImg(string name)
        {
            List<string> list = new List<string>() { "eu\\talleaume", "eu\\bgermain", "eu\\jbenard","eu\\yxiong", "eu\\mlaget", "eu\\qtanter" }; 
            if (list.Contains(name))
            {
                NavigationManager.NavigateTo("/Autre/Kantaim");
            }
            
        }

        MudTheme BicTheme = new MudTheme()
        {
            Palette = new Palette()
            {
                AppbarBackground = "#f89332",
                AppbarText = "#000000",
                Primary = "#f89332",
                Secondary = "#f89332",
                Tertiary = "#f89332",
            },
            Typography = new Typography()
            {
                H6 = new H6()
                {
                    FontFamily = new[]
                    {
                        "system-ui",
                        "-apple-system",
                        "Segoe UI",
                        "Roboto ",
                        "Helvetica Neue",
                        "Arial",
                        "Noto Sans",
                        "Liberation Sans",
                        "sans-serif",
                        "Apple Color Emoji",
                        "Segoe UI Emoji",
                        "Segoe UI Symbol",
                        "Noto Color Emoji"
                    },
                    FontSize = "1.25rem",
                    FontWeight = 400,
                    LineHeight = 1.7,
                    LetterSpacing = "normal"
                },
                Button = new Button()
                {
                    FontFamily = new[]
                    {
                        "system-ui",
                        "-apple-system",
                        "Segoe UI",
                        "Roboto ",
                        "Helvetica Neue",
                        "Arial",
                        "Noto Sans",
                        "Liberation Sans",
                        "sans-serif",
                        "Apple Color Emoji",
                        "Segoe UI Emoji",
                        "Segoe UI Symbol",
                        "Noto Color Emoji"
                    },
                    FontSize = "1rem",
                    FontWeight = 400,
                    LineHeight = 1.5,
                    LetterSpacing = "normal"
                }
            }
        };
    }
}