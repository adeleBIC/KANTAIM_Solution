using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TColorProduct")]
    public class ColorProduct : IObject
    {
        [Key]
        [Column("TColorProductId")]
        public int Id { get; set; }

        [Column("ColorID")]
        public int ColorID { get; set; }
        [ForeignKey(nameof(ColorID))]
        public virtual ProdColor Color { get; set; }

        [Column("ProductID")]
        public int ProductID { get; set; }
        [ForeignKey(nameof(ProductID))]
        public virtual Product Product { get; set; }
    }
}
