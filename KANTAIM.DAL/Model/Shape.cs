using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TShapes")]
    public class Shape : IObject
    {
        [Key]
        [Column("TShapeID")]
        public int Id { get; set; }

        public int? Number { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int TotalMark { get; set; }

        public int UsedMark { get; set; }

        public double Cycle { get; set; }

        public decimal OpenTime { get; set; }

        public decimal Objective { get; set; }

        public bool? Active { get; set; }

        public string? Comment { get; set; }


        [Column("FKTProductID")]
        public int ProductID { get; set; }
        [ForeignKey(nameof(ProductID))]
        public virtual Product Product { get; set; }
    }
}
