using KANTAIM.DAL.Model;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.ViewModels
{
    public class RackVM
    {
        public static explicit operator Rack(RackVM vm) => vm.model;

        Rack model;

        public RackVM(IEnumerable<Workshop> workshop) : this(new Rack(), workshop) { }
        public RackVM(Rack model, IEnumerable<Workshop> workshop)
        {
            this.model = model;
            name = model.Name;
            active = model.Active;
            workshopID = model.WorkshopID;
            comment = model.Comment;
            Workshops = workshop.ToList();

        }

        public List<Workshop> Workshops { get; set; }

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

        private bool active;
        [Required]
        [Label("Actif")]
        public bool Active
        {
            get { return active; }
            set { active = value; IsEditing = true; }
        }

        private string? comment;
        [Label("Commentaire")]
        public string? Comment
        {
            get { return comment; }
            set { comment = value; IsEditing = true; }
        }

        [Required]
        [Label("Workshop")]
        private Workshop workshop;
        public Workshop Workshop
        {
            get { return workshop; }
            set { workshop = value; IsEditing = true; }
        }

        [Required]
        [Label("WorkshopID")]
        private int workshopID;
        public int WorkshopID
        {
            get { return workshopID; }
            set { workshopID = value; IsEditing = true; }
        }

        public string WorkshopName { get => workshop?.Name ?? ""; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(name)) yield return new ValidationResult("Le nom est obligatoire", new string[] { "Name" });
            if (workshopID < 0) yield return new ValidationResult("L'atelier est obligatoire", new string[] { "WorkshopID" });
            else
            {
                model.Name = name;
                model.Comment = comment;
                model.Active = active;
                model.WorkshopID = workshopID;
            }
        }
    }
}
