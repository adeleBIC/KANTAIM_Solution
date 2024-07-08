using MudBlazor;
using KANTAIM.DAL.Model;
using KANTAIM.DAL.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.AccessControl;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KANTAIM.WEB.ViewModels
{
    public class DataProdVM : IValidatableObject
    {
        public static explicit operator DataProd(DataProdVM vm) => vm.model;

        DataProd model;

        public DataProdVM(IEnumerable<Product> products, IEnumerable<Press> presses) : this ( new DataProd(), products, presses) { }
        public DataProdVM(DataProd model, IEnumerable<Product> products, IEnumerable<Press> presses)
        {
            this.model = model;
            this.Products = products.ToList();
            this.Presses = presses.ToList();

            pressID = model.PressID;
            productID = model.ProductID;
            pressID = model.PressID;
            numDayShift = model.NumDayShift;
            numWeekshift = model.NumWeekShift;
            counter = model.Counter;
            trs = model.TRS;
            opentime = model.OpenTime;
            objective = model.Objective;
            objOK = model.ObjOk;
            dateProd = model.DateProd;
            dateExtract = model.DateExtract;
            comment = model.Comment;


        }
       

        public List<Product> Products { get; set; }
        public List<Press> Presses { get; set; }

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;
        [Required]
        [Label("Numero de presse")]
        private int pressID;
        public int PressID
        {
            get { return pressID; }
            set { pressID = value; IsEditing = true; }
        }

        [Required]
        [Label("Produit")]
        private int productID;
        public int ProductID
        {
            get { return productID; }
            set { productID = value; IsEditing = true; model.Product = this.Products.FirstOrDefault(p => p.Id == value); }
        }
        [Label("Produit")]
        public string ProduitName { get
            {
                if (model.Product != null)
                    return model.Product.Name;
                else return "N/A";
            }
        }


        [Required]
        [Label("NumDayShift")]
        private int numDayShift;
        public int NumDayShift
        {
            get { return numDayShift; }
            set { numDayShift = value; IsEditing = true; }
        }


        [Required]
        [Label("numWeekshift")]
        private int numWeekshift;
        public int NumWeekshift
        {
            get { return numWeekshift; }
            set { numWeekshift = value; IsEditing = true; }
        }


        [Required]
        [Label("Compteur")]
        private int counter;
        public int Counter
        {
            get { return counter; }
            set { counter = value; IsEditing = true; }
        }


        [Required]
        [Label("TRS")]
        private decimal trs;
        public decimal TRS
        {
            get { return trs; }
            set { trs = value; IsEditing = true; }
        }
        
        [Required]
        [Label("Objectif atteint")]
        private bool objOK;
        public bool ObjOK
        {
            get { return objOK; }
            set { objOK = value; IsEditing = true; }
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
        [Label("0")]
        private DateTime dateProd;
        public DateTime DateProd
        {
            get { return dateProd; }
            set { dateProd = value; IsEditing = true; }
        }

        [Required]
        [Label("dataExtract")]
        private DateTime dateExtract;
        public DateTime DateExtract
        {
            get { return dateExtract; }
            set { dateExtract = value; IsEditing = true; }

        }

        [Label("Commentaire")]
        private string? comment;
        public string? Comment
        {
            get { return comment; }
            set { comment = value; IsEditing = true; }

        }

        public PressVM SelectedPress { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> list = new List<ValidationResult>();

            if (pressID <= 0) list.Add(new ValidationResult("La valeur du champ Nombre d'empreintes total doit être superieur que 0", new string[] { "pressID" }));
            if (counter <= 0) list.Add(new ValidationResult("La valeur du champ compteur doit être superieur que 0", new string[] { "counter" }));
            if (opentime <= 0 || opentime > 24) list.Add(new ValidationResult("La valeur du champ Temps d'ouverture (H) doit être comprise entre 0 et 24", new string[] { "opentime" }));
            if (objective <= 0) list.Add(new ValidationResult("La valeur du champ objective doit être superieur que 0", new string[] { "opentime" }));
            if (productID <= 0) list.Add(new ValidationResult("Niveau obligatoire", new string[] { "fkproduitid" }));
            if (numDayShift <= 0) list.Add(new ValidationResult("Equipe obligatoire", new string[] { "numDayShift" }));

            if (list.Count <= 0)
            {
                ModelUpdate();
            }

            return list;
        }

        public void ModelUpdate()
        {
            model.ProductID = productID;
            model.PressID = pressID;
            model.PressID = pressID;
            model.NumDayShift = numDayShift;

            model.NumWeekShift = numWeekshift;

            model.Counter = counter;
            model.OpenTime = opentime;
            model.Objective = objective;

            model.TRS = trs;
            model.ObjOk = objOK;

            model.DateProd = dateProd;
            model.DateExtract = dateExtract;
            model.Comment = comment;
        }

    }
}
