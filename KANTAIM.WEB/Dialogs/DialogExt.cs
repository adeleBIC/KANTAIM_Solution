using KANTAIM.WEB.Dialogs;
using KANTAIM.WEB.Pages;
using MudBlazor;

namespace MudBlazor
{
    public static class DialogExt
    {
        public static async Task Confirm(this IDialogService srvce, string text, Action Sucess)
        {
            DialogParameters parameters = new DialogParameters()
            {
                {"Text", text }
            };

            var options = new DialogOptions()
            {
                DisableBackdropClick = true,
                MaxWidth = MaxWidth.ExtraSmall,
                NoHeader = true
            };

            var dialog = await srvce.ShowAsync<Confirm>("", parameters, options);
            var result = await dialog.Result;

            if (result.Canceled) return;

            Sucess?.Invoke();
        }
    }
}
