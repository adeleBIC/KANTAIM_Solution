using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TUserAccessLvls")]
    public class UserAccessLvl : IObject
    {
        [Key]
        [Column("TUserAccessLvlID")]
        public int Id { get; set; }

        public int AccesLvL { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public string? Comment { get; set; }
    }
}
