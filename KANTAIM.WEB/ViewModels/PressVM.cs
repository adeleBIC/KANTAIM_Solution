using MudBlazor;
using KANTAIM.DAL.Model;
using System.ComponentModel.DataAnnotations;
using static MudBlazor.CategoryTypes;

namespace KANTAIM.WEB.ViewModels
{
    public class PressVM
    {
        public static explicit operator Press(PressVM vm) => vm.model;

        Press model;

        public PressVM(IEnumerable<Shape> Shapes) : this(new Press(), Shapes) { }
        public PressVM(Press model, IEnumerable<Shape> Shapes)
        {
            this.model = model;
            this.Shapes = Shapes.ToList();

            number = model.Number;
            consignNumber = model.ConsignNumber;
            iPAdress = model.IPAdress;
            qRcode = model.QRcode;
            shapeID = model.ShapeID;
            workshopID = model.WorkshopID;
            active = model.Active;
            comment = model.Comment;
        }

        public PressVM(Product u)
        {
            this.u = u;
        }

        public List<Shape> Shapes { get; set; }

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;
        [Required]
        [Label("Numéro de presse")]
        private int number;
        public int Number
        {
            get { return number; }
            set { number = value; IsEditing = true; }
        }

        [Label("Numéro consignateur")]
        private int consignNumber;
        public int ConsignNumber
        {
            get { return consignNumber; }
            set { consignNumber = value; IsEditing = true; }
        }

        [Label("Adresse IP")]
        [StringLength(15)]
        private string? iPAdress;
        public string? IPAdress
        {
            get { return iPAdress; }
            set { iPAdress = value; IsEditing = true; }
        }

        [Label("QRCode")]
        [StringLength(15)]
        private string? qRcode;
        public string? QRcode
        {
            get { return qRcode; }
            set { qRcode = value; IsEditing = true; }
        }

        [Required]
        [Label("Moule")]
        private int shapeID;
        public int ShapeID
        {
            get { return shapeID; }
            set { shapeID = value; IsEditing = true; }
        }
        [Label("Produit fabriqué")]
        public string ShapeName { get => $"{model.Shape?.Number} - {model.Shape?.Name}"; }

        private int workshopID;
        public int WorkshopID
        {
            get { return workshopID; }
            set { workshopID = value; IsEditing = true; }
        }

        [Label("Actif")]
        private bool? active;
        public bool? Active
        {
            get { return active; }
            set { active = value; IsEditing = true; }
        }
        [Label("Commentaire")]
        private string? comment;
        private Product u;

        public string? Comment
        {
            get { return comment; }
            set { comment = value; IsEditing = true; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> list = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(IPAdress)) list.Add(new ValidationResult("L'adresse IP est obligatoire", new string[] { "name" }));
            if (number < 1 || number > 999) list.Add(new ValidationResult("Le numéro de presse doit être compris entre 1 et 999", new string[] { "number" }));
            if (consignNumber < 1 || consignNumber > 99) list.Add(new ValidationResult("Le numéro consignateur doit être compris entre 1 et 99", new string[] { "consignNumber" }));
            if (shapeID == 0) list.Add(new ValidationResult("le moule est obligatoire", new string[] { "fkproduitid" }));

            if (list.Count <= 0)
            {
                model.Id = Id;
                model.Number = number;
                model.ConsignNumber = consignNumber;
                model.IPAdress = IPAdress;
                model.QRcode = qRcode;
                model.ShapeID = shapeID;
                model.WorkshopID = workshopID;
                model.Active = active.Value;
                model.Comment = comment;
            }

            return list;
        }
    }
}
