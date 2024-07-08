using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace KANTAIM.DAL.Model
{
    [Table("TCellProduct")]
    public class CellProduct : IObject
    {
        [Key]
        [Column("TCellProductId")]
        public int Id { get; set; }

        [Column("CellID")]
        public int CellID { get; set; }
        [ForeignKey(nameof(CellID))]
        public virtual Cell Cell { get; set; }

        [Column("ProductID")]
        public int ProductID { get; set; }
        [ForeignKey(nameof(ProductID))]
        public virtual Product Product { get; set; }
    }
}
