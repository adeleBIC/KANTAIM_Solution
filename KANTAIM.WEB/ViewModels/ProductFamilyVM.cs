using MudBlazor;
using KANTAIM.DAL.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.ViewModels
{
    public class ProductFamilyVM : IValidatableObject
    {
        public static explicit operator ProductFamily(ProductFamilyVM vm) => vm.model;

        ProductFamily model;

        public ProductFamilyVM() => model = new ProductFamily();
        public ProductFamilyVM(ProductFamily model)
        {
            this.model = model;
            name = model.Name;
            active = model.Active;
            comment = model.Comment;
        }

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;
        [Required]
        [Label("Nom")]
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; IsEditing = true; }
        }
        [Label("Actif")]
        private bool active;
        public bool Active
        {
            get { return active; }
            set { active = value; IsEditing = true; }
        }
        [Label("Commentaire")]
        private string? comment;
        public string? Comment
        {
            get { return comment; }
            set { comment = value; IsEditing = true; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(name)) yield return new ValidationResult("Le nom est obligatoire", new string[] { "Name" });
            else
            {
                model.Name = name;
                model.Active = active;
                model.Comment = comment;
            }
        }
    }
}
