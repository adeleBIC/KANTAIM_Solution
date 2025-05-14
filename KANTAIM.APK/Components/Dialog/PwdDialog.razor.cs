using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace KANTAIM.APK.Components.Dialog
{
    public partial class PwdDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        private string _input = string.Empty;
        private string CorrectPassword = "7319";

        private string[][] PadLayout = new[]
        {
        new[] { "1", "4", "7" },
        new[] { "2", "5", "8", "0" },
        new[] { "3", "6", "9" }
    };

        private void AddDigit(string digit)
        {
            if (_input.Length < 10)
                _input += digit;
        }

        private void Clear() => _input = string.Empty;

        private void Cancel() => MudDialog.Cancel();

        private void Validate()
        {
            MudDialog.Close(DialogResult.Ok(_input == CorrectPassword));
        }
    }
}