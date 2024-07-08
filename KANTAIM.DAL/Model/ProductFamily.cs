using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TProductFamilies")]
    public class ProductFamily : IObject
    {
        [Key]
        [Column("TProductFamiliesID")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public bool Active { get; set; }

        public string? Comment { get; set; }
    }
}
