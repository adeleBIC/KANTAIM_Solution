using KANTAIM.DAL.Model;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.ViewModels
{
    public class MachineVM
    {
        public static explicit operator Machine(MachineVM vm) => vm.model;
        private Machine model;

        public MachineVM(IEnumerable<Product> products, IEnumerable<Machine> machines) : this(new Machine(), products, machines) { }
        public MachineVM(Machine model, IEnumerable<Product> products, IEnumerable<Machine> machines)
        {
            this.model = model;
            this.Products = products.ToList();
            this.machines = machines.ToList();

            number = model.Number;
            name = model.Name;
            active = model.Active;
            iPAdress = model.IPAdress;
            qRCode = model.QRcode;
            productID = model.ProductID;
            isInkjet = model.IsInkjet;
            comment = model.Comment;
        }

        public List<Product> Products { get; set; }
        private List<Machine> machines { get; set; }

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;


        [Required]
        [Label("Number")]
        private int number;
        public int Number
        {
            get { return number; }
            set { number = value; IsEditing = true; }
        }
        [Required]
        [Label("Name")]
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; IsEditing = true; }
        }

        [Required]
        [Label("Active")]
        private bool active;
        public bool Active
        {
            get { return active; }
            set { active = value; IsEditing = true; }
        }

        [Label("IPAdress")]
        private string iPAdress;
        public string IPAdress
        {
            get { return iPAdress; }
            set { iPAdress = value; IsEditing = true; }
        }

        [Label("QRCode")]
        private string qRCode;
        public string QRCode
        {
            get { return qRCode; }
            set { qRCode = value; IsEditing = true; }
        }
        
        [Required]
        [Label("Niveau dFKTProductIDaccès")]
        private int? productID;
        public int? ProductID
        {
            get { return productID; }
            set { productID = value; IsEditing = true; }
        }

        
        [Label("FKTProductID")]
        public string? ProductName { get => model.Product == null ? null : model.Product.Name; }

        [Required]
        [Label("IsInkjet")]
        private bool isInkjet;
        public bool IsInkjet
        {
            get { return isInkjet; }
            set { isInkjet = value; IsEditing = true; }
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

            if (productID == 0) list.Add(new ValidationResult("Le nom est obligatoire", new string[] { "fKTProductID" }));
            if (Number == 0) list.Add(new ValidationResult("Niveau obligatoire", new string[] { "Number" }));
            if (machines.Any(c => c.Number == Number && c.Id != Id))
            {
                list.Add(new ValidationResult("Le numéro de machine doit être unique.", new string[] { "Number" }));
            }
            if (Name == null) list.Add(new ValidationResult("Name obligatoire", new string[] { "Name" }));

            if (list.Count <= 0)
            {
                model.Number = number;
                model.Name = name;
                model.Active = active;
                model.IPAdress = iPAdress;
                model.QRcode = qRCode;
                model.ProductID = productID;
                model.IsInkjet = isInkjet;
                model.Comment = comment;
            }

            return list;
        }
    }
}
