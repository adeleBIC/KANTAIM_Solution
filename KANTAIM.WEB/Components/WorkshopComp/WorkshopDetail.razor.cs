using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using KANTAIM.WEB.ViewModels;
using KANTAIM.DAL.Services;
using KANTAIM.DAL.Model;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Components.WorkshopComp
{
    public partial class WorkshopDetail
    {
        [Inject] public WorkshopService _workshopService { get; set; }
        [Parameter] public WorkshopVM Workshop { get; set; }
        [Parameter] public EventCallback OnSaved { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }
        [Inject] ISnackbar _snackService { get; set; }


        protected override void OnParametersSet()
        {
            base.OnParametersSet();
        }

        public async Task Save(EditContext ctx)
        {
            ValidationContext validationContext = new ValidationContext(Workshop);
            var validationResults = Workshop.Validate(validationContext).ToList();

            if (validationResults.Count == 0)
            {
                Workshop u = (Workshop)Workshop;
                _workshopService.UpSert(u);
                Workshop.IsEditing = false;
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

            await OnSaved.InvokeAsync();
        }
        public async Task Cancel()
        {
            Workshop.IsEditing = false;
            await OnCancel.InvokeAsync();
        }
    }
}