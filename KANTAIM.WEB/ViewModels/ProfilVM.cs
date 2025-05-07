using KANTAIM.DAL.Model;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.ViewModels
{
    public class ProfilVM
    {
        public static explicit operator Profil(ProfilVM vm) => vm.model;

        Profil model;

        public ProfilVM() => model = new Profil();
        public ProfilVM(Profil model)
        {
            this.model = model;
            name = model.Name;
            active = model.Active;
            rackProfils = model.RackProfils.ToList();
            comment = model.Comment;

            Racks = new List<Rack>(RackProfils.Select(x => x.Rack).ToList());
            SelectedRackNames = new List<string>(RackProfils.Select(x => x.Rack).Select(x => x.Name).ToList());
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

        private List<RackProfil> rackProfils;
        [Label("Commentaire")]
        public List<RackProfil> RackProfils
        {
            get { return rackProfils; }
            set { rackProfils = value; IsEditing = true; }
        }

        public List<Rack> Racks { get; set; }

        public string RacksName
        {
            get
            {
                if (Racks == null || Racks.Count() == 0) return "Aucun";
                else return string.Join(", ", Racks.Select(x => x.Name));
            }
        }

        public List<string> SelectedRackNames { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(name)) yield return new ValidationResult("Le nom est obligatoire", new string[] { "Name" });
            if (SelectedRackNames == null || SelectedRackNames.Count() == 0) yield return new ValidationResult("Au moins un rack est obligatoire", new string[] { "Rack" });
            else
            {
                model.Name = name;
                model.Comment = comment;
                model.Active = active;
                //model.RackProfils = rackProfils.ToList();
            }
        }
    }
}
