using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TContainer")]
    public class Container : IObject
    {
        [Key]
        [Column("TContainerID")]
        public int Id { get; set; }

        public int Number { get; set; }

        public int FillStatus { get; set; }
        public int Status { get; set; }

        public bool InJail { get; set; }

        public bool InMaintenance { get; set; }

        [Required]
        [StringLength(50)]
        public string QRcode { get; set; }

        public DateTime? LastEvent { get; set; }

        public string? Comment { get; set; }

        [Column("FKTContainerID")]
        public int? ContainerID { get; set; }
        [ForeignKey(nameof(ContainerID))]
        public virtual Container? BigContainer { get; set; }

        [Column("FKTContainerTypeID")]
        public int? ContainerTypeID { get; set; }
        [ForeignKey(nameof(ContainerTypeID))]
        public virtual ContainerType? ContainerType { get; set; }

        [Column("FKTActionID")]
        public int ActionID { get; set; }
        [ForeignKey(nameof(ActionID))]
        public virtual ContainerAction ContainerAction { get; set; }

        [Column("FKTCellID")]
        public int? CellID { get; set; }
        [ForeignKey(nameof(CellID))]
        public virtual Cell? CellStock { get; set; }

        [Column("FKTProductID")]
        public int? ProductID { get; set; }
        [ForeignKey(nameof(ProductID))]
        public virtual Product? Product { get; set; }

        [Column("FKTColorID")]
        public int? ProdColorID { get; set; }
        [ForeignKey(nameof(ProdColorID))]
        public virtual ProdColor? ProdColor { get; set; }

        [Column("FKTPressID")]
        public int? PressID { get; set; }
        [ForeignKey(nameof(PressID))]
        public virtual Press? Press { get; set; }

        [Column("FKTMachineID")]
        public int? MachineID { get; set; }
        [ForeignKey(nameof(PressID))]
        public virtual Machine? Machine { get; set; }
    }
}
