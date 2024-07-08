using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TProducts")]
    public class Product : IObject
    {
        [Key]
        [Column("TProductID")]
        public int Id { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string JDECode { get; set; }

        [Required]
        [StringLength(50)]
        public string QRCode { get; set; }

        public bool Active { get; set; }

        public int QuantityPerContainer { get; set; }

        public string? Comment { get; set; }

        [Column("FKTProductFamiliesID")]
        public int? ProductFamilyID { get; set; }
        [ForeignKey(nameof(ProductFamilyID))]
        public virtual ProductFamily ProductFamily { get; set; }

        public virtual ICollection<Cell> Cells { get; set; } = new List<Cell>();


    }
}
