using KANTAIM.DAL.Model;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace KANTAIM.WEB.ViewModels
{
    public class CellVM
    {
        private Cell model;
        public static explicit operator Cell(CellVM vm) => vm.model;

        public CellVM() : this(new Cell()) { }
        public CellVM(Cell model)
        {
            this.model = model;
            name = model.Name;
            status = model.Status;
            x = model.X;
            y = model.Y;
            nbMax = model.NbMax;
            isJail = model.IsJail;
            qRCode = model.QRcode;
            forEmpty = model.ForEmpty;
            isPhantom = model.IsPhantom;
            isMaintenance = model.IsMaintenance;
            comment = model.Comment;
            containers = model.Containers.ToList();
        }
        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;
        [Required]
        [Label("Name")]
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; IsEditing = true; }
        }

        [Required]
        [Label("Status")]
        private int status;
        public int Status
        {
            get { return status; }
            set { status = value; IsEditing = true; }
        }

        [Label("X")]
        private int x;
        public int X
        {
            get { return x; }
            set { x = value; IsEditing = true; }
        }

        [Label("Y")]
        private int y;
        public int Y
        {
            get { return y; }
            set { y = value; IsEditing = true; }
        }

        [Label("NbMax")]
        private int nbMax;
        public int NbMax
        {
            get { return nbMax; }
            set { nbMax = value; IsEditing = true; }
        }

        [Label("IsJail")]
        private bool isJail;
        public bool IsJail
        {
            get { return isJail; }
            set { isJail = value; IsEditing = true; }
        }

        [Label("ForEmpty")]
        private bool forEmpty;
        public bool ForEmpty
        {
            get { return forEmpty; }
            set { forEmpty = value; IsEditing = true; }
        }

        [Label("IsPhantom")]
        private bool isPhantom;
        public bool IsPhantom
        {
            get { return isPhantom; }
            set { isPhantom = value; IsEditing = true; }
        }

        [Label("IsMaintenance")]
        private bool isMaintenance;
        public bool IsMaintenance
        {
            get { return isMaintenance; }
            set { isMaintenance = value; IsEditing = true; }
        }
        [Label("QRCode")]
        private string? qRCode;
        public string? QRCode
        {
            get { return qRCode; }
            set { qRCode = value; IsEditing = true; }
        }

        [Label("Comment")]
        private string? comment;
        public string? Comment
        {
            get { return comment; }
            set { comment = value; IsEditing = true; }
        }

        [Label("Containers")]
        private List<Container> containers;
        public List<Container> Containers
        {
            get { return containers; }
        }
        public int ContainerCount { get; set; }

        public string DynamicStatus
        {
            get
            {
                if (ContainerCount == 0)
                    return "Vide";
                else if (ContainerCount < NbMax)
                    return "En Remplissage";
                else
                    return "Plein";
            }
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> list = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(Name)) list.Add(new ValidationResult("Le nom est obligatoire", new string[] { "Name" }));
            if (list.Count <= 0)
            {

                model.Name = Name;
                model.X = x;
                model.Y = y;
                model.NbMax = nbMax;
                model.Status = Status;
                model.IsJail = isJail;
                model.QRcode = "4#" + x + "#" + y + "$";
                model.ForEmpty = forEmpty;
                model.IsPhantom = isPhantom;
                model.IsMaintenance = isMaintenance;
                model.Comment = comment;
            }

            return list;
        }
    }
}
