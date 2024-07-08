using KANTAIM.DAL.Model;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.ViewModels
{
    public class ColorProductVM
    {
        private ColorProduct model;
        public static explicit operator ColorProduct(ColorProductVM vm) => vm.model;

        public ColorProductVM() : this(new ColorProduct()) { }
        public ColorProductVM(ColorProduct model)
        {
            this.model = model;
            colorID = model.ColorID;
            color = model.Color;
            productID = model.ProductID;
            product = model.Product;
        }

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;

        private int colorID;
        public int ColorID
        {
            get { return colorID; }
            set { colorID = value; IsEditing = true; }
        }
        private ProdColor color;
        public virtual ProdColor Color
        {
            get { return color; }
        }

        private int productID;
        public int ProductID
        {
            get { return productID; }
            set { productID = value; IsEditing = true; }
        }
        private Product product;
        public virtual Product Product
        {
            get { return product; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> list = new List<ValidationResult>();

            if (ColorID > 0) list.Add(new ValidationResult("La couleur est obligatoire", new string[] { "ColorID" }));
            if (ProductID > 0) list.Add(new ValidationResult("Le produit est obligatoire", new string[] { "ProductID" }));

            if (list.Count <= 0)
            {

                model.ColorID = ColorID;
                model.ProductID = ProductID;
            }

            return list;
        }
    }
}
