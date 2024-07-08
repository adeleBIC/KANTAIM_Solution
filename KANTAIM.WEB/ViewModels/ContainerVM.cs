using KANTAIM.DAL.Model;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.ViewModels
{
    public class ContainerVM : IValidatableObject
    {
        public static explicit operator Container(ContainerVM vm) => vm.model;
        private Container model;
        public List<Container> Containers { get; set; }
        public List<ContainerType> ContainerTypes { get; set; }
        public List<ContainerAction> Actions { get; set; }
        public List<Cell> Cells { get; set; }

        public ContainerVM(IEnumerable<Container> containers, IEnumerable<ContainerType> containerTypes, IEnumerable<Cell> cells, IEnumerable<ContainerAction> actions) : this(new Container(), containers, containerTypes, cells, actions) { }
        public ContainerVM(Container model, IEnumerable<Container> containers, IEnumerable<ContainerType> containerTypes, IEnumerable<Cell> cells, IEnumerable<ContainerAction> actions)
        {
            this.model = model;
            this.Containers = containers.ToList();
            this.ContainerTypes = containerTypes.ToList();
            this.Cells = cells.ToList();
            this.Actions = actions.ToList();

            actionID = model.ActionID;
            
            number = model.Number;
            containerID = model.ContainerID;
            containerTypeID = model.ContainerTypeID;
            cellID = model.CellId;
            fillStatus = model.FillStatus;
            status = model.Status;
            inJail = model.InJail;
            qRCode = model.QRcode;
            inMaintenance = model.InMaintenance;
            comment = model.Comment;

        }

 

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;

        [Required]
        [Label("FKTContainerID")]
        private int? containerID;
        public int? ContainerID
        {
            get { return containerID; }
            set { containerID = value; IsEditing = true; }
        }

        [Label("FKTContainerTypeID")]
        private int? containerTypeID;
        public int? ContainerTypeID
        {
            get { return containerTypeID; }
            set { containerTypeID = value; IsEditing = true; }
        }

        [Required]
        [Label("FKTCellID")]
        private int? cellID;
        public int? CellID
        {
            get { return cellID; }
            set { cellID = value; IsEditing = true; }
        }

        [Label("Number")]
        private int number;
        public int Number
        {
            get { return number; }
            set { number = value; IsEditing = true; }
        }

        [Label("FKTActionID")]
        private int actionID;
        public int ActionID
        {
            get { return actionID; }
            set { actionID = value; IsEditing = true; }
        }

        [Label("FillStatus")]
        private int fillStatus;
        public int FillStatus
        {
            get { return fillStatus; }
            set { fillStatus = value; IsEditing = true; }
        }

        [Label("Status")]
        private int status;
        public int Status
        {
            get { return status; }
            set { status = value; IsEditing = true; }
        }

        [Label("InJail")]
        private bool inJail;
        public bool InJail
        {
            get { return inJail; }
            set { inJail = value; IsEditing = true; }
        }

        

        [Label("QRCode")]
        private string qRCode;
        public string QRCode
        {
            get { return qRCode; }
            set { qRCode = value; IsEditing = true; }
        }

        [Label("InMaintenance")]
        private bool inMaintenance;
        public bool InMaintenance
        {
            get { return inMaintenance; }
            set { inMaintenance = value; IsEditing = true; }
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

            //if (string.IsNullOrWhiteSpace(QRCode)) list.Add(new ValidationResult("Le QRCode est obligatoire", new string[] { "QRCode" }));
            if (Number == 0) list.Add(new ValidationResult("Number obligatoire", new string[] { "Number" }));
            if (Containers.Any(c => c.Number == Number && c.ContainerTypeID == ContainerTypeID && c.Id != Id))
            {
                list.Add(new ValidationResult("Le combination de numéro et type de conteneur doit être unique.", new string[] { "Number", "ContainerTypeID" }));
            }
            if (list.Count <= 0)
            {
                model.Number = number;
                model.ContainerID = containerID;
                model.ContainerTypeID = containerTypeID;
                model.CellId = cellID;
                
                model.ActionID = actionID;
                model.FillStatus = fillStatus;
                model.Status = status;
                model.InJail = inJail;
                model.QRcode = "1#" + number + "#" + containerTypeID + "$";
                model.InMaintenance = inMaintenance;
                model.Comment = comment;
            }

            return list;
        }
    }
}
