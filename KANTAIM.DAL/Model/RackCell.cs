using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("RackCell")]
    public class RackCell
    {
        [ForeignKey("Rack")]
        public int RackId { get; set; }
        [ForeignKey("Cellule")]
        public int CellId { get; set; }
        public Rack Rack { get; set; }
        public Cell Cell { get; set; }

    }
}
