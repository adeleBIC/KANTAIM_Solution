using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TMachine")]
    public class Machine : IObject
    {
        [Key]
        [Column("TMachineID")]
        public int Id { get; set; }

        [Required]
        public int Number { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public bool Active { get; set; }
        public bool IsInkjet { get; set; }

        [Required]
        [StringLength(15)]
        public string IPAdress { get; set; }

        [Required]
        [StringLength(50)]
        public string QRcode { get; set; }

        public string? Comment { get; set; }

        [Column("FKTProductID")]
        public int? ProductID { get; set; }
        [ForeignKey(nameof(ProductID))]
        public virtual Product? Product { get; set; }
    }
}
