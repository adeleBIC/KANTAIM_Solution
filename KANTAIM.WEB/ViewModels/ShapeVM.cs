using MudBlazor;
using KANTAIM.DAL.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.AccessControl;
using System.Text;

namespace KANTAIM.WEB.ViewModels
{
    public class ShapeVM : IValidatableObject
    {
        public static explicit operator Shape(ShapeVM vm) => vm.model;

        Shape model;

        public ShapeVM(IEnumerable<Product> Products) : this(new Shape(), Products) { }
        public ShapeVM(Shape model, IEnumerable<Product> Products)
        {
            this.model = model;
            this.Products = Products.ToList();

            name = model.Name;
            number = model.Number;
            totalmark = model.TotalMark;
            usedmark = model.UsedMark;
            cycle = model.Cycle;
            opentime = model.OpenTime;
            objective = model.Objective;
            active = model.Active;
            productID = model.ProductID;
            comment = model.Comment;
        }
        public List<Product> Products { get; set; }

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;
        [Required]
        [Label("Lettre")]
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; IsEditing = true; }
        }

        [Label("Matricule")]
        private int? number;
        public int? Number
        {
            get { return number; }
            set { number = value; IsEditing = true; }
        }
        [Required]
        [Label("Nombre d'empreintes total")]
        private int totalmark;
        public int Totalmark
        {
            get { return totalmark; }
            set { totalmark = value; IsEditing = true; }
        }
        [Required]
        [Label("Nombre d'empreintes utilisées")]
        private int usedmark;
        public int Usedmark
        {
            get { return usedmark; }
            set { usedmark = value; IsEditing = true; }
        }
        [Required]
        [Label("Temps de cycle")]
        private double cycle;
        public double Cycle
        {
            get { return cycle; }
            set { cycle = value; IsEditing = true; }
        }
        [Required]
        [Label("Temps d'ouverture(H)")]
        private decimal opentime;
        public decimal Opentime
        {
            get { return opentime; }
            set { opentime = value; IsEditing = true; }
        }
        [Required]
        [Label("Objectif")]
        private decimal objective;
        public decimal Objective
        {
            get { return objective; }
            set { objective = value; IsEditing = true; }
        }
        [Required]
        [Label("Produit fabriqué")]
        private int productID;
        public int ProductID
        {
            get { return productID; }
            set { productID = value; IsEditing = true; }
        }
        [Label("Produit fabriqué")]
        public string ProduitName { get => model.Product?.Name; }

        [Label("Actif")]
        private bool? active;
        public bool? Active
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
            List<ValidationResult> list = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(name)) list.Add(new ValidationResult("La lettre est obligatoire", new string[] { "name" }));
            if (totalmark < 1 || totalmark > 200) list.Add(new ValidationResult("La valeur du champ Nombre d'empreintes total doit être comprise entre 1 et 200", new string[] { "totalmark" }));
            if (usedmark < 1 || usedmark > 200) list.Add(new ValidationResult("La valeur du champ Nombre d'empreintes utilisées doit être comprise entre 1 et 200", new string[] { "cycle" }));
            if (cycle < 1 || cycle > 100) list.Add(new ValidationResult("La valeur du champ Temps de cycle doit être comprise entre 1 et 100 et le décimaux doit être sépareé par virgule au lieu de point", new string[] { "name" }));
            if (opentime < 0 || opentime >24) list.Add(new ValidationResult("La valeur du champ Temps d'ouverture (H) doit être comprise entre 0 et 24", new string[] { "opentime" }));

            if (productID == 0) list.Add(new ValidationResult("Produit obligatoire", new string[] { "fkproduitid" }));

            if (list.Count <= 0)
            {
                model.Id = Id;
                model.Name = name;
                model.Number = number;
                model.TotalMark = totalmark;
                model.UsedMark = usedmark;
                model.Cycle = cycle;
                model.OpenTime = opentime;
                model.Objective = objective;
                model.Active = active;
                model.ProductID = productID;
                model.Comment = comment;
            }

            return list;
        }
    }

}
