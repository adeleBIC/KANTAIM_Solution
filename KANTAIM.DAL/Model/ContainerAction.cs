using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TAction")]
    public class ContainerAction : IObject
    {
        [Key]
        [Column("TActionID")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int Status { get; set; }

        public string? Comment { get; set; }
    }
}
