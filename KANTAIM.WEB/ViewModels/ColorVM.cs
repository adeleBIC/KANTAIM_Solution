using KANTAIM.DAL.Model;
using MudBlazor;
using System.ComponentModel.DataAnnotations;


namespace KANTAIM.WEB.ViewModels
{
    public class ColorVM : IValidatableObject
    {
        private ProdColor model;
        public static explicit operator ProdColor(ColorVM vm) => vm.model;

        public ColorVM() : this (new ProdColor()) { }
        public ColorVM(ProdColor model)
        {
            this.model = model;
            name = model.Name;
            colorNumber = model.ColorNumber;  
            priority = model.Priority;
        }
        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;
        
        [Label("Name")]
        private string? name;
        public string? Name
        {
            get { return name; }
            set { name = value; IsEditing = true; }
        }

        [Required]
        [Label("ColorNumber")]
        private string colorNumber;
        public string ColorNumber
        {
            get { return colorNumber; }
            set { colorNumber = value; IsEditing = true; }
        }

        [Label("Priority")]
        private int? priority;
        public int? Priority
        {
            get { return priority; }
            set { priority = value; IsEditing = true; }
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(colorNumber)) yield return new ValidationResult("Le color number est obligatoire", new string[] { "colorNumber" });
            else
            {
                model.Name = name;
                model.ColorNumber = colorNumber;
                model.Priority = priority;
            }
        }

    }
}
