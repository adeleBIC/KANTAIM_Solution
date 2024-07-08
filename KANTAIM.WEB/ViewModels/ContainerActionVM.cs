using KANTAIM.DAL.Model;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.ViewModels
{
    public class ContainerActionVM
    {
        private ContainerAction model;
        public static explicit operator ContainerAction(ContainerActionVM vm) => vm.model;

        public ContainerActionVM() : this(new ContainerAction()) { }
        public ContainerActionVM(ContainerAction model)
        {
            this.model = model;
            name = model.Name;
            status = model.Status;
            comment = model.Comment;
        }
        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;
        [Required]
        [Label("Name")]
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; IsEditing = true; }
        }

        [Required]
        [Label("Status")]
        private int status;
        public int Status
        {
            get { return status; }
            set { status = value; IsEditing = true; }
        }

        [Label("Comment")]
        private string? comment;
        public string? Comment
        {
            get { return comment; }
            set { comment = value; IsEditing = true; }
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> list = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(Name)) list.Add(new ValidationResult("Le nom est obligatoire", new string[] { "Name" }));
            if (list.Count <= 0)
            {
                model.Name = Name;
                model.Status = Status;
                model.Comment = comment;
            }

            return list;
        }
    }
}
