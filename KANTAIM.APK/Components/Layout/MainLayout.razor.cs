using KANTAIM.DAL.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace KANTAIM.APK.Components.Layout
{
    public partial class MainLayout
    {
        private bool drawerOpen = false;

        void ToggleDrawer()
        {
            drawerOpen = !drawerOpen;
        }

        protected override async Task OnInitializedAsync()
        {
           
            await InvokeAsync(StateHasChanged);
        }

        MudTheme BicTheme = new MudTheme()
        {
            PaletteDark = new PaletteDark()
            {
                AppbarBackground = "#f89332",
                AppbarText = "#000000",
                Primary = "#f89332",
                Secondary = "#f89332",
                Tertiary = "#f89332",
            },
            PaletteLight = new PaletteLight()
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
                Button = new MudBlazor.Button()
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