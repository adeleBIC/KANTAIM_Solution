using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TWorkshops")]
    public class Workshop : IObject
    {
        [Key]
        [Column("TWorkshopID")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(15)]
        public string? IPAdressConsign { get; set; }

        public int? Generation { get; set; }

        public string? Comment { get; set; }
    }
}
