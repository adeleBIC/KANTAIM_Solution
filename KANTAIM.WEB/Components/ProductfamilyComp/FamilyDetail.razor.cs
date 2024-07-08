using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using KANTAIM.WEB.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.Components.ProductfamilyComp
{
    public partial class FamilyDetail
    {
        [Inject] public ProductFamilyService _productFamilyService { get; set; }
        [Parameter] public ProductFamilyVM ProductFamily { get; set; }
        [Parameter] public EventCallback OnSaved { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }
        [Inject] ISnackbar _snackService { get; set; }


        protected override void OnParametersSet()
        {
            base.OnParametersSet();
        }

        public async Task Save(EditContext ctx)
        {
            ValidationContext validationContext = new ValidationContext(ProductFamily);
            var validationResults = ProductFamily.Validate(validationContext).ToList();

            if (validationResults.Count == 0)
            {
                ProductFamily u = (ProductFamily)ProductFamily;
                _productFamilyService.UpSert(u);
                ProductFamily.IsEditing = false;
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
            ProductFamily.IsEditing = false;
            await OnCancel.InvokeAsync();
        }
    }
}