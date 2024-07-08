using MudBlazor;
using KANTAIM.DAL.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KANTAIM.WEB.ViewModels
{
    public class WorkshopVM
    {
        public static explicit operator Workshop(WorkshopVM vm) => vm.model;

        Workshop model;

        public WorkshopVM() => model = new Workshop();
        public WorkshopVM(Workshop model)
        {
            this.model = model;
            name = model.Name;
            comment = model.Comment;
        }

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;

        private string name;
        [Required]
        [Label("Nom")]
        public string Name
        {
            get { return name; }
            set { name = value; IsEditing = true; }
        }

        private string? comment;
        [Label("Commentaire")]
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
                model.Comment = comment;
            }
        }
    }
}
