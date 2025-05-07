using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TRack")]
    public class Rack : IObject
    {
        [Key]
        [Column("TRackID")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public bool Active { get; set; }

        public string? Comment { get; set; }

        [Column("FKTWorkshopID")]
        public int WorkshopID { get; set; }
        [ForeignKey(nameof(WorkshopID))]
        public virtual Workshop Workshop { get; set; }

        public ICollection<RackProfil> RackProfils { get; set; }
        public ICollection<RackCell> RackCells { get; set; }

    }
}
