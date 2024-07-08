using KANTAIM.DAL.Model;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.ViewModels
{
    public partial class ContenaireTypeVM
    {
        public static explicit operator ContainerType(ContenaireTypeVM vm) => vm.model;

        ContainerType model;

        public ContenaireTypeVM() => model = new ContainerType();
        public ContenaireTypeVM(ContainerType model)
        {
            this.model = model;
            name = model.Name;
            typeNumber = model.TypeNumber;
            nbrMaxContainer = model.NbrMaxContainer;
            ymax = model.Ymax;
            isContainable = model.IsContainable;
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

        private int? typeNumber;
        [Label("TypeNumber")]
        public int? TypeNumber
        {
            get { return typeNumber; }
            set { typeNumber = value; IsEditing = true; }
        }

        private int nbrMaxContainer;
        [Required]
        [Label("NbrMaxContainer")]
        public int NbrMaxContainer
        {
            get { return nbrMaxContainer; }
            set { nbrMaxContainer = value; IsEditing = true; }
        }

        private int ymax;
        [Required]
        [Label("Ymax")]
        public int Ymax
        {
            get { return ymax; }
            set { ymax = value; IsEditing = true; }
        }
        [Label("IsContainable")]
        private bool isContainable;
        public bool IsContainable
        {
            get { return isContainable; }
            set { isContainable = value; IsEditing = true; }
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
            List<ValidationResult> list = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(Name)) list.Add(new ValidationResult("Le nom est obligatoire", new string[] { "Name" }));
            if (Ymax == 0) list.Add(new ValidationResult("Y max est obligatoire", new string[] { "Ymax" }));
            if (list.Count <= 0)
            {
                model.Name = Name;
                model.TypeNumber = TypeNumber;
                model.NbrMaxContainer = NbrMaxContainer;
                model.Ymax = Ymax;
                model.IsContainable = isContainable;
                model.Comment = comment;
            }

            return list;
        }

    }
}
