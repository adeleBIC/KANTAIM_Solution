using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Reflection.PortableExecutable;

namespace KANTAIM.DAL.Model
{
    [Table("TLog")]
    public class Log : IObject
    {
        [Key]
        [Column("TLogID")]
        public int Id { get; set; }

        public DateTime EventTime { get; set; }
        public int FillStatus { get; set; }

        public string? Comment { get; set; }

        [Column("FKTContainerID")]
        public int ContainerID { get; set; }
        [ForeignKey(nameof(ContainerID))]
        public virtual Container Container { get; set; }

        //[Column("FKTActionID")]
        //public int ContainerActionID { get; set; }
        //[ForeignKey(nameof(ContainerActionID))]
        //public virtual ContainerAction ContainerAction { get; set; }

        public int Operation { get; set; }

        [Column("FKTProductID")]
        public int? ProductID { get; set; }
        [ForeignKey(nameof(ProductID))]
        public virtual Product? Product { get; set; }

        [Column("FKTPressID")]
        public int? PressID { get; set; }
        [ForeignKey(nameof(PressID))]
        public virtual Press? Press { get; set; }

        [Column("FKTShapeID")]
        public int? ShapeID { get; set; }
        [ForeignKey(nameof(ShapeID))]
        public virtual Shape? Shape { get; set; }

        [Column("FKTCellID")]
        public int? CellID { get; set; }
        [ForeignKey(nameof(CellID))]
        public virtual Cell? Cell { get; set; }

        [Column("FKTColorID")]
        public int? ProdColorID { get; set; }
        [ForeignKey(nameof(ProdColorID))]
        public virtual ProdColor? ProdColor { get; set; }

        [Column("FKTMachineID")]
        public int? MachineID { get; set; }
        [ForeignKey(nameof(MachineID))]
        public virtual Machine? Machine { get; set; }

    }
}
