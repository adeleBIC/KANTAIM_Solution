using KANTAIM.DAL.Model;
using MudBlazor;
using Radzen;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.ViewModels
{
    public class LogVM
    {
        public static explicit operator Log(LogVM vm) => vm.model;
        private Log model;

        public List<Log> Logs { get; set; }
        public List<Product> Products { get; set; }
        public List<Press> Presses { get; set; }
        public List<Shape> Shapes { get; set; }
        public List<Machine> Machines { get; set; }
        public List<Cell> Cells { get; set; }
        public List<Container> Containers { get; set; }
        public List<ProdColor> Colors { get; set; }


        public LogVM(IEnumerable<Product> products, IEnumerable<Press> presses, IEnumerable<Shape> shapes, IEnumerable<Machine> machines, IEnumerable<Cell> cells, IEnumerable<Container> containers, IEnumerable<ProdColor> colors) : this(new Log(), products, presses, shapes, machines, cells, containers, colors) { }
        public LogVM(Log model, IEnumerable<Product> products, IEnumerable<Press> presses, IEnumerable<Shape> shapes, IEnumerable<Machine> machines, IEnumerable<Cell> cells, IEnumerable<Container> containers, IEnumerable<ProdColor> colors)
        {
            this.model = model;
            this.Products = products.ToList();
            this.Presses = presses.ToList();
            this.Shapes = shapes.ToList();
            this.Machines = machines.ToList();
            this.Cells = cells.ToList();
            this.Containers = containers.ToList();
            this.Colors = colors.ToList();

            operation = model.Operation;
            fillStatus = model.FillStatus;
            eventTime = model.EventTime;
            containerID = model.ContainerID;
            productID = model.ProductID;
            pressID = model.PressID;
            shapeID = model.ShapeID;
            cellID = model.CellID;
            colorID = model.ProdColorID;
            machineID = model.MachineID;
            comment = model.Comment;
            container = model.Container;
            product = model.Product ?? new Product();
            press = model.Press ?? new Press();
            shape = model.Shape ?? new Shape();
            cell = model.Cell ?? new Cell();
            prodColor = model.ProdColor ?? new ProdColor();
            machine = model.Machine ?? new Machine();

        }

        

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;

        [Required]
        [Label("Operation")]
        private int operation;
        public int Operation
        {
            get { return operation; }
            set { operation = value; IsEditing = true; }
        }

        [Label("FillStatus")]
        private int fillStatus;
        public int FillStatus
        {
            get { return fillStatus; }
            set { fillStatus = value; IsEditing = true; }
        }

        [Required]
        [Label("EventTime")]
        private DateTime eventTime;
        public DateTime EventTime
        {
            get { return eventTime; }
            set { eventTime = value; IsEditing = true; }
        }

        [Label("FKTContainer")]
        private int containerID;
        public int ContainerID
        {
            get { return containerID; }
            set { containerID = value; IsEditing = true; }
        }

        [Label("Container")]
        private Container container;
        public Container Container
        {
            get { return container; }
            set { container = value; }
        }

        [Label("FKTProductID")]
        private int? productID;
        public int? ProductID
        {
            get { return productID; }
            set { productID = value; IsEditing = true; }
        }

        [Label("Product")]
        private Product product;
        public Product Product
        {
            get { return product; }
            set { product = value; }
        }

        [Label("FKTPressID")]
        private int? pressID;
        public int? PressID
        {
            get { return pressID; }
            set { pressID = value; IsEditing = true; }
        }

        [Label("Presse")]
        private Press press;
        public Press Press
        {
            get { return press; }
            set { press = value; }
        }

        [Label("FKTShapeID")]
        private int? shapeID;
        public int? ShapeID
        {
            get { return shapeID; }
            set { shapeID = value; IsEditing = true; }
        }

        [Label("Moule")]
        private Shape shape;
        public Shape Shape
        {
            get { return shape; }
            set { shape = value; }
        }

        [Label("FKTCellID")]
        private int? cellID;
        public int? CellID
        {
            get { return cellID; }
            set { cellID = value; IsEditing = true; }
        }

        [Label("Cellule")]
        private Cell cell;
        public Cell Cell
        {
            get { return cell; }
            set { cell = value; }
        }

        [Label("FKTColorID")]
        private int? colorID;
        public int? ColorID
        {
            get { return colorID; }
            set { colorID = value; IsEditing = true; }
        }

        [Label("Couleur")]
        private ProdColor prodColor;
        public ProdColor ProdColor
        {
            get { return prodColor; }
            set { prodColor = value; }
        }

        [Label("FKTMachineID")]
        private int? machineID;
        public int? MachineID
        {
            get { return machineID; }
            set { machineID = value; IsEditing = true; }
        }

        [Label("Machine")]
        private Machine machine;
        public Machine Machine
        {
            get { return machine; }
            set { machine = value; }
        }

        private string? comment;
        [Label("Commentaire")]
        public string? Comment
        {
            get { return comment; }
            set { comment = value; IsEditing = true; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> list = new List<ValidationResult>();

            if (EventTime == null) list.Add(new ValidationResult("Le EventTime est obligatoire", new string[] { "EventTime" }));
            if (ContainerID == 0) list.Add(new ValidationResult("Container obligatoire", new string[] { "ContainerID" }));
            //if (Operation == 0) list.Add(new ValidationResult("Operation obligatoire", new string[] { "Operation" }));
            if (FillStatus == 0) list.Add(new ValidationResult("État de remplissage obligatoire", new string[] { "FillStatus" }));
            if (list.Count <= 0)
            {
                model.Operation = operation;
                model.FillStatus = fillStatus;
                model.EventTime = eventTime;
                model.ContainerID = containerID;
                model.ProductID = productID;
                model.PressID = pressID;
                model.ShapeID = shapeID;
                model.CellID = cellID;
                model.ProdColorID = colorID;
                model.MachineID = machineID;
                model.Comment = comment;
            }

            return list;
        }
    }
}
