using MudBlazor;
using KANTAIM.DAL.Model;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.ViewModels
{
    public class ProductVM
    {
        public static explicit operator Product(ProductVM vm) => vm.model;

        Product model;

        public ProductVM() : this(new Product()) { }
        public ProductVM(Product model)
        {
            this.model = model;

            number = model.Number;
            name = model.Name;
            productFamilyID = model.ProductFamilyID;
            jDECode = model.JDECode;
            quantityPerContainer = model.QuantityPerContainer;
            qRCode = model.QRCode;
            active = model.Active;
            comment = model.Comment;
        }
        //public List<ProductFamily> ProductFamilies { get; set; }

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;
        [Required]
        [Label("Nom de produit")]
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; IsEditing = true; }
        }
        [Required]
        [Label("Number")]
        private int number;
        public int Number
        {
            get { return number; }
            set { number = value; IsEditing = true; }
        }
        [Required]
        [Label("Numéro consignateur")]
        private int? productFamilyID;
        public int? ProductFamilyID
        {
            get { return productFamilyID; }
            set { productFamilyID = value; IsEditing = true; }
        }

        [Label("Quantity Per Container")]
        private int quantityPerContainer;
        public int QuantityPerContainer
        {
            get { return quantityPerContainer; }
            set { quantityPerContainer = value; IsEditing = true; }
        }
        [Required]
        [Label("QRCode")]
        [StringLength(15)]
        private string qRCode;
        //public string QRCode { get => $"5#{model.Number}$"; }
        
        public string QRCode
        {
            get { return qRCode; }
            set { qRCode = value; IsEditing = true; }
        }
        
        [Label("Code JDE")]
        [StringLength(15)]
        private string? jDECode;
        public string? JDECode
        {
            get { return jDECode; }
            set { jDECode = value; IsEditing = true; }
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
            List<ValidationResult> list = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(name)) list.Add(new ValidationResult("Nom de produit est obligatoire", new string[] { "name" }));
            if (string.IsNullOrWhiteSpace(jDECode)) list.Add(new ValidationResult("Le code JDE est obligatoire", new string[] { "jDECode" }));
            //if (string.IsNullOrWhiteSpace(QRCode)) list.Add(new ValidationResult("Le QRCode est obligatoire", new string[] { "QRcode" }));
            if (Number == 0) list.Add(new ValidationResult("Number obligatoire", new string[] { "Number" }));
            if (list.Count <= 0)
            {
                this.model = model;
                model.Number = number;
                model.Name = name;
                model.ProductFamilyID = productFamilyID;
                model.JDECode = jDECode;
                model.QuantityPerContainer = quantityPerContainer;
                model.QRCode = QRCode;
                //model.QRCode = "5#" + number + "$";
                model.Active = active;
                model.Comment = comment;
            }

            return list;
        }
    }
}
