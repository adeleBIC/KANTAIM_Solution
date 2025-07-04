using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TColor")]
    public class ProdColor : IObject
    {
        [Key]
        [Column("TColorID")]
        public int Id { get; set; }

        
        [StringLength(50)]
        public string? Name { get; set; }
        
        [Required]
        [Column("Color")]
        [StringLength(50)]
        public string ColorNumber { get; set; }

        [Column("Priority")]
        public int? Priority { get; set; }

    }
}
