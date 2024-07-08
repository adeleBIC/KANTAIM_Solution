using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TPresses")]
    public class Press : IObject
    {
        [Key]
        [Column("TPressID")]
        public int Id { get; set; }

        public int Number { get; set; }


        public int ConsignNumber { get; set; }


        public bool Active { get; set; }

        [StringLength(15)]
        public string? IPAdress { get; set; }

        
        [StringLength(50)]
        public string? QRcode { get; set; }

        public string? Comment { get; set; }

        [Required]
        [Column("FKTShapeID")]
        public int ShapeID { get; set; }
        [ForeignKey(nameof(ShapeID))]
        public virtual Shape Shape { get; set; }

        [Required]
        [Column("FKTWorkshopID")]
        public int WorkshopID { get; set; }
        [ForeignKey(nameof(WorkshopID))]
        public virtual Workshop Workshop { get; set; }

    }
}
