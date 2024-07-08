using KANTAIM.DAL.Model;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KANTAIM.WEB.ViewModels
{
    public class CellProductVM
    {
        private CellProduct model;
        public static explicit operator CellProduct(CellProductVM vm) => vm.model;

        public CellProductVM() : this(new CellProduct()) { }
        public CellProductVM(CellProduct model)
        {
            this.model = model;
            cellID = model.CellID;
            cell = model.Cell;
            productID = model.ProductID;
            product = model.Product;
        }

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;

        private int cellID;
        public int CellID
        {
            get { return cellID; }
            set { cellID = value; IsEditing = true; }
        }
        private Cell cell;
        public virtual Cell Cell
        {
            get { return cell; }
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

            if (CellID > 0) list.Add(new ValidationResult("La cellule est obligatoire", new string[] { "CellID" }));
            if (ProductID > 0) list.Add(new ValidationResult("Le produit est obligatoire", new string[] { "ProductID" }));

            if (list.Count <= 0)
            {

                model.CellID = CellID;
                model.ProductID = ProductID;
            }

            return list;
        }
    }
}
