using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KANTAIM.DAL.Model
{
    [Table("TProfil")]
    public class Profil : IObject
    {
        [Key]
        [Column("TProfilID")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public bool Active { get; set; }

        public string? Comment { get; set; }

        public ICollection<RackProfil> RackProfils { get; set; }
    }
}
