using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TCell")]
    public class Cell : IObject
    {
        [Key]
        [Column("TCellID")]
        public int Id { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }


        //[Column("FKTWorkshopID")]
        //public int WorkshopID { get; set; }
        //[ForeignKey(nameof(WorkshopID))]
        //public virtual Workshop Workshop { get; set; }


        public int NbMax { get; set; }

        public int Status { get; set; }

        public bool IsJail { get; set; }

        public string? Comment { get; set; }

       
        [StringLength(50)]
        public string? QRcode { get; set; }

        public bool ForEmpty { get; set; }

        public bool IsPhantom { get; set; }
        public bool IsMaintenance { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Container> Containers { get; set; } = new List<Container>();
        public ICollection<RackCell> RackCells { get; set; }
    }
}
